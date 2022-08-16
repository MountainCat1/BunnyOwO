using BunnyOwO.Configuration;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BunnyOwO.FluentValidation;

public class EventReceiverWithFluentValidation<TEvent> : EventReceiver<TEvent> where TEvent : class, IEvent
{
    private readonly IValidator<TEvent> _eventValidator;

    public EventReceiverWithFluentValidation(IOptions<RabbitMQConfiguration> rabbitMqConfiguration, ILogger<EventReceiver<TEvent>> logger, IEventHandler<TEvent> eventHandler, IValidator<TEvent> eventValidator) : base(rabbitMqConfiguration, logger, eventHandler)
    {
        _eventValidator = eventValidator;
    }


    public override async Task<bool> ValidateEventAsync(TEvent @event)
    {
        return (await _eventValidator.ValidateAsync(@event)).IsValid;
    }
}