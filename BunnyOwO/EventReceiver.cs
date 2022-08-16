using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BunnyOwO.Configuration;
using BunnyOwO.Exceptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BunnyOwO;


public interface IEventReceiver : IHostedService
{
    string QueueName { get; set; }
    Task<bool> ProcessAsync(string message);
    void Register();
    void DeRegister();
}
public interface IEventReceiver<TEvent> : IEventReceiver
    where TEvent : IEvent
{
    Task<bool> ValidateEventAsync(TEvent @event);
}

public class EventReceiver<TEvent> : IEventReceiver<TEvent> 
    where TEvent : class, IEvent
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    
    private readonly ILogger<EventReceiver<TEvent> > _logger;
    private readonly IEventHandler<TEvent> _eventHandler;

    public EventReceiver(
        IOptions<RabbitMQConfiguration> rabbitMqConfiguration, 
        ILogger<EventReceiver<TEvent>> logger,
        IEventHandler<TEvent> eventHandler)
    {
        _logger = logger;
        _eventHandler = eventHandler;
        
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
            throw new EventValidationException("Event validation failed");

        return await _eventHandler.HandleAsync(@event);
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
        _connection.Close();
        return Task.CompletedTask;
    }
}