using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BunnyOwO.Configuration;
using BunnyOwO.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BunnyOwO;


public interface IMessageReceiver : IHostedService
{
    string QueueName { get; set; }
    Task<bool> ProcessAsync(string messageJson);
    void Register();
    void DeRegister();
}
public interface IMessageReceiver<TMessage> : IMessageReceiver, IDisposable
    where TMessage : IMessage
{
    Task<bool> ValidateEventAsync(TMessage message);
}

public class MessageReceiver<TEvent> : IMessageReceiver<TEvent>
    where TEvent : class, IMessage
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    
    private readonly ILogger<MessageReceiver<TEvent> > _logger;
    private readonly IServiceCollection _services;

    public MessageReceiver(
        IOptions<RabbitMQConfiguration> rabbitMqConfiguration, 
        ILogger<MessageReceiver<TEvent>> logger,
        IServiceCollection services)
    {
        _logger = logger;
        _services = services;

        var factory = new ConnectionFactory()
        {
            HostName = rabbitMqConfiguration.Value.HostName,
            UserName = rabbitMqConfiguration.Value.UserName,
            Password = rabbitMqConfiguration.Value.Password,
            Port = rabbitMqConfiguration.Value.Port,
            VirtualHost = rabbitMqConfiguration.Value.VirtualHost
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Register();
        return Task.CompletedTask;
    }
    public string QueueName{ get; set; }
    
    // How to process messages
    public virtual async Task<bool> ProcessAsync(string messageJson)
    {
        _logger.LogInformation($"Receiving message: {messageJson}");
        
        TEvent? message = JsonSerializer.Deserialize<TEvent>(messageJson);
        
        if (message == null)
            throw new SerializationException($"Cannot deserialize broker message to {typeof(TEvent).Name}");

        if (!await ValidateEventAsync(message))
            throw new MessageValidationException("Event validation failed");

        await using (var scope = _services.BuildServiceProvider().CreateAsyncScope())
        {
            var messageHandler = scope.ServiceProvider.GetService<IMessageHandler<TEvent>>();
            return await messageHandler.HandleAsync(message);
        }
    }
    

    public virtual async Task<bool> ValidateEventAsync(TEvent message)
    {
        return true;
    }

    public void Register()
    {
        _logger.LogInformation($"Registering {GetType().Name}...");

        var consumer = new EventingBasicConsumer(_channel);
        
        consumer.Received += OnConsumerOnReceived;
        _channel.BasicConsume(queue: QueueName, consumer: consumer);
    }

    private async void OnConsumerOnReceived(object model, BasicDeliverEventArgs ea)
    {
        var body = ea.Body;
        var message = Encoding.UTF8.GetString(body.ToArray());

        try
        {
            var result = await ProcessAsync(message);

            if (result)
            {
                _channel.BasicAck(ea.DeliveryTag, false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception was thrown while processing RabbitMQ message");
        }
    }

    public void DeRegister()
    {
        _connection.Close();
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _channel.Close();
        _connection.Close();

        Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _connection?.Dispose();
        _channel?.Dispose();
    }
}