using Microsoft.Extensions.DependencyInjection;

namespace BunnyOwO.Extensions;

public static class EventSenderExtensions
{
    public static IServiceCollection AddSender(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ISender, EventSender>();
        return serviceCollection;
    }
}