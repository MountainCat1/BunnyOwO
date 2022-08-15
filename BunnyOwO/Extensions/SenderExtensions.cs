using Microsoft.Extensions.DependencyInjection;

namespace BunnyOwO.Extensions;

public static class SenderExtensions
{
    public static IServiceCollection AddSender(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ISender, Sender>();
        return serviceCollection;
    }
}