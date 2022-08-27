using BunnyOwO.Configuration;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BunnyOwO.FluentValidation;

public class MessageSenderWithFluentValidation : MessageSender
{
    private readonly IServiceProvider _serviceProvider;
    
    public MessageSenderWithFluentValidation(IOptions<RabbitMQConfiguration> rabbitMqConfiguration, ILogger<MessageSender> logger, IServiceProvider serviceProvider) : base(rabbitMqConfiguration, logger)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task<bool> ValidateEventAsync<TEvent>(TEvent @event)
    {
        var eventValidator = _serviceProvider.GetService<IValidator<TEvent>>();

        if (eventValidator is null)
            return await base.ValidateEventAsync(@event);
        
        return (await eventValidator.ValidateAsync(@event)).IsValid;
    }
    
    
}