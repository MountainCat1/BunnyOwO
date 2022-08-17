using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace BunnyOwO.Extensions;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds sender as well as event handlers and receivers using basic RabbitMq setup,
    /// requires that <see cref="Configuration.RabbitMQConfiguration" /> is configured
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection AddBunnyOwO(this IServiceCollection serviceCollection,
        Assembly eventHandlersAssemblyMarker)
    {
        serviceCollection.AddSender();
        serviceCollection.AddEventHandlers(eventHandlersAssemblyMarker);
        serviceCollection.AddEventReceivers(eventHandlersAssemblyMarker);
        
        return serviceCollection;
    }
    
    /// <summary>
    /// Adds sender as well as event handlers and receivers using basic RabbitMq setup,
    /// requires that <see cref="Configuration.RabbitMQConfiguration" /> is configured
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection AddBunnyOwO(this IServiceCollection serviceCollection,
        Type eventHandlersAssemblyMarker)
    {
        serviceCollection.AddSender();
        serviceCollection.AddEventHandlers(eventHandlersAssemblyMarker.Assembly);
        serviceCollection.AddEventReceivers(eventHandlersAssemblyMarker.Assembly);

        return serviceCollection;
    }
    
    /// <summary>
    /// Adds sender as well as event handlers and receivers using basic RabbitMq setup,
    /// requires that <see cref="Configuration.RabbitMQConfiguration" /> is configured
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection AddBunnyOwO(this IServiceCollection serviceCollection,
        IEnumerable<Type> eventHandlersAssemblyMarkers)
    {
        serviceCollection.AddSender();
        serviceCollection.AddEventHandlers(eventHandlersAssemblyMarkers.Select(type => type.Assembly).ToArray());
        serviceCollection.AddEventReceivers(eventHandlersAssemblyMarkers.Select(type => type.Assembly).ToArray());

        return serviceCollection;
    }
    
    /// <summary>
    /// Adds event handlers and receivers using basic RabbitMq setup,
    /// requires that <see cref="Configuration.RabbitMQConfiguration" /> is configured
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection AddEventHandlersAndReceivers(this IServiceCollection serviceCollection,
        IEnumerable<Type> eventHandlersAssemblyMarkers)
    {
        serviceCollection.AddEventHandlers(eventHandlersAssemblyMarkers.Select(type => type.Assembly).ToArray());
        serviceCollection.AddEventReceivers(eventHandlersAssemblyMarkers.Select(type => type.Assembly).ToArray());

        return serviceCollection;
    }
    
    /// <summary>
    /// Adds event handlers and receivers using basic RabbitMq setup,
    /// requires that <see cref="Configuration.RabbitMQConfiguration" /> is configured
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection AddEventHandlersAndReceivers(this IServiceCollection serviceCollection,
        params Type[] eventHandlersAssemblyMarkers)
    {
        return AddEventHandlersAndReceivers(serviceCollection, eventHandlersAssemblyMarkers.AsEnumerable());
    }
}