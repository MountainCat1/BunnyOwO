using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace BunnyOwO.Extensions;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds sender as well as message handlers and receivers using basic RabbitMq setup,
    /// requires that <see cref="Configuration.RabbitMQConfiguration" /> is configured
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection AddBunnyOwO(this IServiceCollection serviceCollection,
        Assembly eventHandlersAssemblyMarker)
    {
        serviceCollection.AddMessageSender();
        serviceCollection.AddMessageHandlers(eventHandlersAssemblyMarker);
        serviceCollection.AddMessageReceivers(eventHandlersAssemblyMarker);
        
        return serviceCollection;
    }
    
    /// <summary>
    /// Adds sender as well as message handlers and receivers using basic RabbitMq setup,
    /// requires that <see cref="Configuration.RabbitMQConfiguration" /> is configured
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection AddBunnyOwO(this IServiceCollection serviceCollection,
        Type eventHandlersAssemblyMarker)
    {
        serviceCollection.AddMessageSender();
        serviceCollection.AddMessageHandlers(eventHandlersAssemblyMarker.Assembly);
        serviceCollection.AddMessageReceivers(eventHandlersAssemblyMarker.Assembly);

        return serviceCollection;
    }
    
    /// <summary>
    /// Adds sender as well as message handlers and receivers using basic RabbitMq setup,
    /// requires that <see cref="Configuration.RabbitMQConfiguration" /> is configured
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection AddBunnyOwO(this IServiceCollection serviceCollection,
        IEnumerable<Type> eventHandlersAssemblyMarkers)
    {
        serviceCollection.AddMessageSender();
        serviceCollection.AddMessageHandlers(eventHandlersAssemblyMarkers.Select(type => type.Assembly).ToArray());
        serviceCollection.AddMessageReceivers(eventHandlersAssemblyMarkers.Select(type => type.Assembly).ToArray());

        return serviceCollection;
    }
    
    /// <summary>
    /// Adds message handlers and receivers using basic RabbitMq setup,
    /// requires that <see cref="Configuration.RabbitMQConfiguration" /> is configured
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection AddMessageHandlersAndReceivers(this IServiceCollection serviceCollection,
        IEnumerable<Type> eventHandlersAssemblyMarkers)
    {
        serviceCollection.AddMessageHandlers(eventHandlersAssemblyMarkers.Select(type => type.Assembly).ToArray());
        serviceCollection.AddMessageReceivers(eventHandlersAssemblyMarkers.Select(type => type.Assembly).ToArray());

        return serviceCollection;
    }
    
    /// <summary>
    /// Adds message handlers and receivers using basic RabbitMq setup,
    /// requires that <see cref="Configuration.RabbitMQConfiguration" /> is configured
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection AddMessageHandlersAndReceivers(this IServiceCollection serviceCollection,
        params Type[] eventHandlersAssemblyMarkers)
    {
        return AddMessageHandlersAndReceivers(serviceCollection, eventHandlersAssemblyMarkers.AsEnumerable());
    }
}