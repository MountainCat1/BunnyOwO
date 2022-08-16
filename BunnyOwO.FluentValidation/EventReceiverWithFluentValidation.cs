using BunnyOwO.Configuration;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BunnyOwO.FluentValidation;

public class EventReceiverWithFluentValidation<TEvent> : EventReceiver<TEvent> where TEvent : class, IEvent
{
    private IServiceProvider _serviceProvider;

    public EventReceiverWithFluentValidation(IOptions<RabbitMQConfiguration> rabbitMqConfiguration, 
        ILogger<EventReceiver<TEvent>> logger,
        IEventHandler<TEvent> eventHandler,  
        IServiceProvider serviceProvider) : base(rabbitMqConfiguration, logger, eventHandler)
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