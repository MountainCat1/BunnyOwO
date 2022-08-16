using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace BunnyOwO.Extensions;

public static class EventBusExtensions
{
    /// <summary>
    /// Adds sender as well as event handlers and receivers using basic RabbitMq setup,
    /// requires that <see cref="Configuration.RabbitMQConfiguration" /> is configured
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection AddBunnyOwO(this IServiceCollection serviceCollection,
        Assembly eventHandlersAssemblyMarker,
        Assembly eventReceiversAssemblyMarker)
    {
        serviceCollection.AddSender();
        serviceCollection.AddEventHandlers(eventHandlersAssemblyMarker);
        serviceCollection.AddEventReceivers(eventReceiversAssemblyMarker);
        
        return serviceCollection;
    }
    
    /// <summary>
    /// Adds sender as well as event handlers and receivers using basic RabbitMq setup,
    /// requires that <see cref="Configuration.RabbitMQConfiguration" /> is configured
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection AddBunnyOwO(this IServiceCollection serviceCollection,
        Type eventHandlersAssemblyMarker,
        Type eventReceiversAssemblyMarker)
    {
        serviceCollection.AddSender();
        serviceCollection.AddEventHandlers(eventHandlersAssemblyMarker.Assembly);
        serviceCollection.AddEventReceivers(eventReceiversAssemblyMarker.Assembly);

        return serviceCollection;
    }
    
    /// <summary>
    /// Adds sender as well as event handlers and receivers using basic RabbitMq setup,
    /// requires that <see cref="Configuration.RabbitMQConfiguration" /> is configured
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection AddBunnyOwO(this IServiceCollection serviceCollection,
        IEnumerable<Type> eventHandlersAssemblyMarkers,
        IEnumerable<Type> eventReceiversAssemblyMarkers)
    {
        serviceCollection.AddSender();
        serviceCollection.AddEventHandlers(eventHandlersAssemblyMarkers.Select(type => type.Assembly).ToArray());
        serviceCollection.AddEventReceivers(eventReceiversAssemblyMarkers.Select(type => type.Assembly).ToArray());

        return serviceCollection;
    }
}