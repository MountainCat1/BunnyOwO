using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace BunnyOwO.Extensions;

public static class EventReceiverExtensions
{
    /// <summary>
    /// Adds <see cref="IMessageReceiver"/> to service collection
    /// </summary>
    /// <param name="configure">Method used to configure <see cref="IMessageReceiver"/></param>
    /// <typeparam name="T"><see cref="IMessageHandler"/> implementation</typeparam>
    /// <returns></returns>
    public static IServiceCollection AddMessageReceiver<T>(this IServiceCollection services,
        Action<T> configure)
        where T : class, IMessageReceiver
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
    /// Registers message receivers based on registered event handlers
    /// </summary>
    /// <param name="receiverImplementation">Implementation that should be used for found
    /// <see cref="IMessageHandler"/> inheriting classes</param>
    /// <param name="assemblies">Assemblies that will be scanned for classes inheriting <see cref="IMessageHandler"/></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException">Event was not found in specified assemblies for found Receiver</exception>
    public static IServiceCollection AddMessageReceivers(this IServiceCollection services, Type receiverImplementation, params Assembly[] assemblies)
    {
        var provider = services.BuildServiceProvider();
        
        var eventHandlerTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsClass && type.IsAssignableTo(typeof(IMessageHandler)));

        foreach (var eventHandlerType in eventHandlerTypes)
        {
            var eventType = eventHandlerType.GetInterfaces()
                .FirstOrDefault(type => type.GetGenericTypeDefinition() == typeof(IMessageHandler<>))
                ?.GetGenericArguments().First();

            if (eventType is null)
                throw new NullReferenceException($"Could not find event type for {eventHandlerType.Name}");

            var receiverType = receiverImplementation.MakeGenericType(eventType);

            Action<IMessageReceiver> configureAction;
            using (var scope = provider.CreateScope())
            {
                var eventHandlerInterfaceType = eventHandlerType.GetInterfaces()
                    .FirstOrDefault(type => type.GetGenericTypeDefinition() == typeof(IMessageHandler<>));
                
                configureAction = ((IMessageHandler)scope.ServiceProvider.GetRequiredService(eventHandlerInterfaceType!)!)
                    .ConfigureReceiver;
            }

            MethodInfo method = typeof(EventReceiverExtensions)
                .GetMethod(nameof(AddReceiverHostedService), BindingFlags.Static | BindingFlags.Public)!;

            InvokeGenericMethod(method, receiverType, services, configureAction);
        }

        return services;
    }
    
    /// <summary>
    /// Registers message receivers based on registered message handlers
    /// </summary>
    /// <param name="assemblies">Assemblies that will be scanned for classes inheriting <see cref="IMessageHandler"/></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException">Event was not found in specified assemblies for found Receiver</exception>
    public static IServiceCollection AddMessageReceivers(this IServiceCollection services, params Assembly[] assemblies)
    {
        return AddMessageReceivers(services, typeof(MessageReceiver<>), assemblies);
    }

    
    private static object? InvokeGenericMethod(MethodInfo method, Type genericTypeParameter, params object[] arguments)
    {
        return method.MakeGenericMethod(genericTypeParameter).Invoke(null, arguments);
    }
    
    public static void AddReceiverHostedService<T>(IServiceCollection services, Action<T>? configure = null)
        where T : class, IMessageReceiver
    {
        services.AddHostedService(provider =>
        {
            var constructors = typeof(T)
                .GetConstructors();

            var constructorInfo = constructors.First();


            using var scope = provider.CreateScope();
            
            var constructorParameters = constructorInfo.GetParameters()
                .Select(parameterInfo => parameterInfo.ParameterType == typeof(IServiceCollection)
                    ? services
                    : scope.ServiceProvider.GetRequiredService(parameterInfo.ParameterType))
                .ToArray();

            var receiver = (T)constructorInfo.Invoke(constructorParameters);

            configure?.Invoke(receiver);

            return receiver;
        });
    }
}