using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace BunnyOwO.Extensions;

public static class MessageHandlerExtensions
{
    /// <summary>
    /// Registers all <see cref="IMessageHandler{TEvent}"/> found in specified assembly
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NullReferenceException">Found message handler doesn't inherit <see cref="IMessageHandler"/></exception>
    public static IServiceCollection AddMessageHandlers(this IServiceCollection services, params Assembly[] assemblies)
    {
        var eventHandlerTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsClass && type.IsAssignableTo(typeof(IMessageHandler)));

        foreach (var eventHandlerType in eventHandlerTypes)
        {
            var eventHandlerInterfaceType = eventHandlerType.GetInterfaces()
                .FirstOrDefault(type => type.GetGenericTypeDefinition() == typeof(IMessageHandler<>));
            
            if(eventHandlerInterfaceType is null)
                throw new NullReferenceException($"MessageHandler need to inherit {typeof(IMessageHandler<>).Name}");
            
            services.AddScoped(eventHandlerInterfaceType, eventHandlerType);
        }
        
        return services;
    }
    
}