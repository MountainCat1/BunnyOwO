using System.Reflection;
using BunnyOwO.Extensions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BunnyOwO.FluentValidation.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddBunnyOwOWithValidation(this IServiceCollection serviceCollection,
        Type eventHandlersAssemblyMarker,
        Type eventReceiversAssemblyMarker,
        params Type[] eventValidatorAssemblyMarkers)
    {
        return AddBunnyOwOWithValidation(serviceCollection, 
            eventHandlersAssemblyMarker.Assembly, 
            eventReceiversAssemblyMarker.Assembly, 
            eventValidatorAssemblyMarkers.Select(type => type.Assembly).ToArray());
    }
    
    public static IServiceCollection AddBunnyOwOWithValidation(this IServiceCollection serviceCollection,
        Assembly eventHandlersAssemblyMarker,
        Assembly eventReceiversAssemblyMarker,
        Assembly[] eventValidatorAssemblyMarkers)
    {
        serviceCollection.AddValidators(eventValidatorAssemblyMarkers);
        
        serviceCollection.AddSender<EventSenderWithFluentValidation>();
        serviceCollection.AddEventHandlers(eventHandlersAssemblyMarker);
        serviceCollection.AddEventReceivers(typeof(EventReceiverWithFluentValidation<>), eventReceiversAssemblyMarker);
        
        return serviceCollection;
    }
    
    public static IServiceCollection AddBunnyOwOWithValidation(this IServiceCollection serviceCollection,
        Type eventHandlersAssemblyMarker,
        Type eventReceiversAssemblyMarker)
    {
        return AddBunnyOwOWithValidation(serviceCollection,
            eventHandlersAssemblyMarker,
            eventReceiversAssemblyMarker,
            eventHandlersAssemblyMarker);
    }


    private static IServiceCollection AddValidators(this IServiceCollection serviceCollection, params Assembly[] markerAssembly)
    {
        var validatorTypes = markerAssembly
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsAssignableTo(typeof(AbstractValidator<>)));

        foreach (var validatorType in validatorTypes)
        {
            var genericArgument = validatorType.GetGenericArguments()[0];

            var validatorGenericType = typeof(IValidator<>).MakeGenericType(genericArgument);

            serviceCollection.AddScoped(validatorGenericType, validatorType);
        }

        return serviceCollection;
    }
}