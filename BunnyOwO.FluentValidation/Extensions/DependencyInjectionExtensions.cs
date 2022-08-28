using System.Reflection;
using BunnyOwO.Extensions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BunnyOwO.FluentValidation.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMessageHandlersReceiversWithValidation(this IServiceCollection serviceCollection,
        Type eventHandlersAssemblyMarker,
        params Type[] eventValidatorAssemblyMarkers)
    {
        return AddMessageHandlersReceiversWithValidation(serviceCollection, 
            eventHandlersAssemblyMarker.Assembly, 
            eventValidatorAssemblyMarkers.Select(x => x.Assembly).ToArray());
    }
    
    public static IServiceCollection AddMessageHandlersReceiversWithValidation(this IServiceCollection serviceCollection,
        Assembly eventHandlersAssemblyMarker,
        Assembly[] eventValidatorAssemblyMarkers)
    {
        serviceCollection.AddValidators(eventValidatorAssemblyMarkers);
        
        serviceCollection.AddMessageHandlers(eventHandlersAssemblyMarker);
        serviceCollection.AddMessageReceivers(typeof(MessageReceiverWithFluentValidation<>), eventHandlersAssemblyMarker);

        return serviceCollection;
    }
    
    public static IServiceCollection AddMessageSenderWithValidation(this IServiceCollection serviceCollection,
        Type[] eventValidatorAssemblyMarkers)
    {
        return AddMessageSenderWithValidation(serviceCollection,
            eventValidatorAssemblyMarkers.Select(x => x.Assembly).ToArray());
    }
    
    public static IServiceCollection AddMessageSenderWithValidation(this IServiceCollection serviceCollection,
        Assembly[] eventValidatorAssemblyMarkers)
    {
        serviceCollection.AddValidators(eventValidatorAssemblyMarkers);
        
        serviceCollection.AddMessageSender<MessageSenderWithFluentValidation>();
        
        return serviceCollection;
    }

    public static IServiceCollection AddBunnyOwOWithValidation(this IServiceCollection serviceCollection,
        Assembly eventHandlersAssemblyMarker,
        Assembly[] eventValidatorAssemblyMarkers)
    {
        serviceCollection.AddValidators(eventValidatorAssemblyMarkers);
        
        serviceCollection.AddMessageSender<MessageSenderWithFluentValidation>();
        serviceCollection.AddMessageHandlers(eventHandlersAssemblyMarker);
        serviceCollection.AddMessageReceivers(typeof(MessageReceiverWithFluentValidation<>), eventHandlersAssemblyMarker);
        
        return serviceCollection;
    }
    
    public static IServiceCollection AddBunnyOwOWithValidation(this IServiceCollection serviceCollection,
        Type eventHandlersAssemblyMarker,
        params Type[] eventValidatorAssemblyMarkers)
    {
        return AddBunnyOwOWithValidation(serviceCollection, 
            eventHandlersAssemblyMarker.Assembly, 
            eventValidatorAssemblyMarkers.Select(type => type.Assembly).ToArray());
    }
    
    public static IServiceCollection AddBunnyOwOWithValidation(this IServiceCollection serviceCollection,
        Type eventHandlersAssemblyMarker)
    {
        return AddBunnyOwOWithValidation(serviceCollection,
            eventHandlersAssemblyMarker,
            eventHandlersAssemblyMarker);
    }


    private static IServiceCollection AddValidators(this IServiceCollection serviceCollection, params Assembly[] markerAssembly)
    {
        var validatorTypes = markerAssembly
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.GetInterfaces().Any(x => x == typeof(IValidator)));
        
        foreach (var validatorType in validatorTypes)
        {
            var genericArgument = validatorType.BaseType.GetGenericArguments()[0];
            
            var validatorGenericType = typeof(IValidator<>).MakeGenericType(genericArgument);

            serviceCollection.AddScoped(validatorGenericType, validatorType);
        }

        return serviceCollection;
    }
}