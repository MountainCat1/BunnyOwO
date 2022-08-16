using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace BunnyOwO.Extensions;

public static class EventHandlerExtensions
{
    /// <summary>
    /// Registers all event handlers found in specified assembly
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NullReferenceException">Found event handler doesn't inherit <see cref="IEventHandler"/></exception>
    public static IServiceCollection AddEventHandlers(this IServiceCollection services, params Assembly[] assemblies)
    {
        var eventHandlerTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsClass && type.IsAssignableTo(typeof(IEventHandler)));

        foreach (var eventHandlerType in eventHandlerTypes)
        {
            var eventHandlerInterfaceType = eventHandlerType.GetInterfaces()
                .FirstOrDefault(type => type.GetGenericTypeDefinition() == typeof(IEventHandler<>));
            
            if(eventHandlerInterfaceType is null)
                throw new NullReferenceException($"Event handlers need to inherit {typeof(IEventHandler<>).Name}");
            
            services.AddScoped(eventHandlerInterfaceType, eventHandlerType);
        }
        
        return services;
    }
    
}