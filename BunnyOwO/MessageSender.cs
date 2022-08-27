using System;
using System.Text;
using System.Text.Json;
using BunnyOwO.Configuration;
using BunnyOwO.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace BunnyOwO;
public interface IMessageSender
{
    void PublishEvent<TEvent>(TEvent @event, string routingKey, string exchange, IBasicProperties? basicProperties = null)
        where TEvent : IMessage;

    Task<bool> ValidateEventAsync<TEvent>(TEvent @event)
        where TEvent : IMessage;
}

/// <summary>
/// Basic implementation of <see cref="IMessageSender"/>
/// </summary>
public class MessageSender : IMessageSender, IDisposable
{
    private readonly RabbitMQConfiguration _rabbitMqConfiguration;
    private readonly ILogger<MessageSender> _logger;
    
    private readonly IModel _channel;
    private readonly IConnection _connection;

    public MessageSender(IOptions<RabbitMQConfiguration> rabbitMqConfiguration, ILogger<MessageSender> logger)
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
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            
            if(_channel is null)
                _logger.LogError("Failed to instantiate channel");
        }
        catch (Exception ex)
        {
            logger.LogError(-1, ex, "Cannot initialize RabbitMQClient channel");
        }
        _logger = logger;
    }

    public virtual void PublishEvent<TEvent>(TEvent @event, string routingKey, string exchange,
        IBasicProperties? basicProperties = null)
        where TEvent : IMessage
    {
        _logger.LogInformation($"Publishing RabbitMQ message with routing key: {routingKey}...");

        if (!ValidateEventAsync(@event).Result)
            throw new MessageValidationException("Message validation failed");
        
        string msgJson = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(msgJson);
        
        _channel.BasicPublish(exchange: exchange,
            routingKey: routingKey,
            basicProperties: basicProperties,
            body: body);
        
        _channel.Close();
        _connection.Close();
    }

    public virtual async Task<bool> ValidateEventAsync<TEvent>(TEvent @event)
        where TEvent : IMessage
    {
        return true;
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}
