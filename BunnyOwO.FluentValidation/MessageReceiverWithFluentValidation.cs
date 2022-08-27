using BunnyOwO.Configuration;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BunnyOwO.FluentValidation;

public class MessageReceiverWithFluentValidation<TEvent> : MessageReceiver<TEvent> where TEvent : class, IMessage
{
    private readonly IServiceProvider _serviceProvider;
    public MessageReceiverWithFluentValidation(IOptions<RabbitMQConfiguration> rabbitMqConfiguration, ILogger<MessageReceiver<TEvent>> logger, IServiceCollection services, IServiceProvider serviceProvider) : base(rabbitMqConfiguration, logger, services)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task<bool> ValidateEventAsync(TEvent @event)
    {
        var eventValidator = _serviceProvider.GetService<IValidator<TEvent>>();

        if (eventValidator is null)
            return await base.ValidateEventAsync(@event);
        
        return (await eventValidator.ValidateAsync(@event)).IsValid;
    }

}