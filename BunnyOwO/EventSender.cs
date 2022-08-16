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
    void PublishMessage(string routingKey, object message);
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
            logger.LogError(-1, ex, "RabbitMQClient init fail");
        }
        _logger = logger;
    }

    public virtual void PublishMessage(string routingKey, object message)
    {
        if(_channel is null)
            _logger.LogError("Failed to instantiate channel");
        
        _logger.LogInformation($"Publishing RabbitMQ message with routing key: {routingKey}...");

        string msgJson = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(msgJson);
        
        _channel.BasicPublish(exchange: "message",
            routingKey: routingKey,
            basicProperties: null,
            body: body);
    }
}
