using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace BunnyOwO.Extensions;

public static class EventReceiverExtensions
{
    /// <summary>
    /// Adds <see cref="IEventReceiver"/> to service collection
    /// </summary>
    /// <param name="configure">Method used to configure <see cref="IEventReceiver"/></param>
    /// <typeparam name="T"><see cref="IEventHandler"/> implementation</typeparam>
    /// <returns></returns>
    public static IServiceCollection AddEventReceiver<T>(this IServiceCollection services,
        Action<T> configure)
        where T : class, IEventReceiver
    {
        services.AddHostedService(provider =>
        {
            var constructors = typeof(T)
                .GetConstructors();

            var constructorInfo = constructors.First();

            var constructorParameters = constructorInfo.GetParameters()
                .Select(parameterInfo => provider.GetRequiredService(parameterInfo.ParameterType))
                .ToArray();

            var receiver = (T)constructorInfo.Invoke(constructorParameters);

            configure(receiver);

            return receiver;
        });

        return services;
    }

    /// <summary>
    /// Registers event receivers based on registered event handlers
    /// </summary>
    /// <param name="receiverImplementation">Implementation that should be used for found
    /// <see cref="IEventHandler"/> inheriting classes</param>
    /// <param name="assemblies">Assemblies that will be searched for classes inheriting <see cref="IEventHandler"/></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException">Event was not found in specified assemblies for found Receiver</exception>
    public static IServiceCollection AddEventReceivers(this IServiceCollection services, Type receiverImplementation, params Assembly[] assemblies)
    {
        var provider = services.BuildServiceProvider();
        
        var eventHandlerTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsClass && type.IsAssignableTo(typeof(IEventHandler)));

        foreach (var eventHandlerType in eventHandlerTypes)
        {
            var eventType = eventHandlerType.GetInterfaces()
                .FirstOrDefault(type => type.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                ?.GetGenericArguments().First();

            if (eventType is null)
                throw new NullReferenceException($"Could not find event type for {eventHandlerType.Name}");

            var receiverType = receiverImplementation.MakeGenericType(eventType);

            Action<IEventReceiver> configureAction;
            using (var scope = provider.CreateScope())
            {
                var eventHandlerInterfaceType = eventHandlerType.GetInterfaces()
                    .FirstOrDefault(type => type.GetGenericTypeDefinition() == typeof(IEventHandler<>));
                
                configureAction = ((IEventHandler)scope.ServiceProvider.GetRequiredService(eventHandlerInterfaceType!)!)
                    .ConfigureReceiver;
            }

            MethodInfo method = typeof(EventReceiverExtensions)
                .GetMethod(nameof(AddReceiverHostedService), BindingFlags.Static | BindingFlags.Public)!;

            InvokeGenericMethod(method, receiverType, services, configureAction);
        }

        return services;
    }
    
    /// <summary>
    /// Registers event receivers based on registered event handlers
    /// </summary>
    /// <param name="assemblies">Assemblies that will be searched for classes inheriting <see cref="IEventHandler"/></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException">Event was not found in specified assemblies for found Receiver</exception>
    public static IServiceCollection AddEventReceivers(this IServiceCollection services, params Assembly[] assemblies)
    {
        return AddEventReceivers(services, typeof(EventReceiver<>), assemblies);
    }

    
    private static object? InvokeGenericMethod(MethodInfo method, Type genericTypeParameter, params object[] arguments)
    {
        return method.MakeGenericMethod(genericTypeParameter).Invoke(null, arguments);
    }
    
    public static void AddReceiverHostedService<T>(IServiceCollection services, Action<T>? configure = null)
        where T : class, IEventReceiver
    {
        services.AddHostedService(provider =>
        {
            var constructors = typeof(T)
                .GetConstructors();

            var constructorInfo = constructors.First();


            using var scope = provider.CreateScope();
            
            var constructorParameters = constructorInfo.GetParameters()
                .Select(parameterInfo => scope.ServiceProvider.GetRequiredService(parameterInfo.ParameterType))
                .ToArray();

            var receiver = (T)constructorInfo.Invoke(constructorParameters);

            configure?.Invoke(receiver);

            return receiver;
        });
    }
}