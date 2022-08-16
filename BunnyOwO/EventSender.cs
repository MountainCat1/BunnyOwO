using System;
using System.Text;
using System.Text.Json;
using BunnyOwO.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace BunnyOwO;
public interface ISender
{
    void PublishEvent<TEvent>(string routingKey, string exchange, TEvent @event)
        where TEvent : IEvent;

    Task<bool> ValidateEventAsync<TEvent>(TEvent @event)
        where TEvent : IEvent;
}

/// <summary>
/// Basic implementation of <see cref="ISender"/>
/// </summary>
public class EventSender : ISender
{
    private readonly RabbitMQConfiguration _rabbitMqConfiguration;
    private readonly ILogger<EventSender> _logger;
    
    private readonly IModel _channel;

    public EventSender(IOptions<RabbitMQConfiguration> rabbitMqConfiguration, ILogger<EventSender> logger)
    {
        _rabbitMqConfiguration = rabbitMqConfiguration.Value;
        _logger = logger;
        
        try
        {
            var factory = new ConnectionFactory()
            {
                HostName = _rabbitMqConfiguration.HostName,
                Password = _rabbitMqConfiguration.Password,
                UserName = _rabbitMqConfiguration.UserName,
                VirtualHost = _rabbitMqConfiguration.VirtualHost
            };
            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();
            
            if(_channel is null)
                _logger.LogError("Failed to instantiate channel");
        }
        catch (Exception ex)
        {
            logger.LogError(-1, ex, "Cannot initialize RabbitMQClient channel");
        }
        _logger = logger;
    }

    public virtual void PublishEvent<TEvent>(string routingKey, string exchange, TEvent @event)
        where TEvent : IEvent
    {
        _logger.LogInformation($"Publishing RabbitMQ message with routing key: {routingKey}...");

        string msgJson = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(msgJson);
        
        _channel.BasicPublish(exchange: exchange,
            routingKey: routingKey,
            basicProperties: null,
            body: body);
    }

    public virtual async Task<bool> ValidateEventAsync<TEvent>(TEvent @event)
        where TEvent : IEvent
    {
        return true;
    }
    
}
