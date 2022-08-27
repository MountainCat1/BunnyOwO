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
    Task<bool> ProcessAsync(string message);
    void Register();
    void DeRegister();
}
public interface IMessageReceiver<TEvent> : IMessageReceiver, IDisposable
    where TEvent : IMessage
{
    Task<bool> ValidateEventAsync(TEvent @event);
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
    public virtual async Task<bool> ProcessAsync(string message)
    {
        _logger.LogInformation($"Receiving message: {message}");
        
        TEvent? @event = JsonSerializer.Deserialize<TEvent>(message);
        
        if (@event == null)
            throw new SerializationException($"Cannot deserialize broker message to {typeof(TEvent).Name}");

        if (!await ValidateEventAsync(@event))
            throw new MessageValidationException("Event validation failed");

        await using (var scope = _services.BuildServiceProvider().CreateAsyncScope())
        {
            var eventHandler = scope.ServiceProvider.GetService<IMessageHandler<TEvent>>();
            return await eventHandler.HandleAsync(@event);
        }
    }
    

    public virtual async Task<bool> ValidateEventAsync(TEvent @event)
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