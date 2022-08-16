﻿using BunnyOwO.Configuration;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BunnyOwO.FluentValidation;

public class EventSenderWithFluentValidation : EventSender
{
    private IServiceProvider _serviceProvider;
    
    public EventSenderWithFluentValidation(IOptions<RabbitMQConfiguration> rabbitMqConfiguration, ILogger<EventSender> logger, IServiceProvider serviceProvider) : base(rabbitMqConfiguration, logger)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task<bool> ValidateEventAsync<TEvent>(TEvent @event)
    {
        var validator = _serviceProvider.GetRequiredService<IValidator<TEvent>>();

        return (await validator.ValidateAsync(@event)).IsValid;
    }
}