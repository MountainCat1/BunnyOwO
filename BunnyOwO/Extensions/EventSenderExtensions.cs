﻿using Microsoft.Extensions.DependencyInjection;

namespace BunnyOwO.Extensions;

public static class EventSenderExtensions
{
    /// <summary>
    /// Adds <see cref="ISender"/> to service collection
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection AddSender(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ISender, EventSender>();
        return serviceCollection;
    }
}