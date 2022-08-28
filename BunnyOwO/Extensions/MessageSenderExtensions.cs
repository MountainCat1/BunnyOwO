using Microsoft.Extensions.DependencyInjection;

namespace BunnyOwO.Extensions;

public static class MessageSenderExtensions
{
    /// <summary>
    /// Adds <see cref="IMessageSender"/> to service collection
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection AddMessageSender(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IMessageSender, MessageSender>();
        return serviceCollection;
    }
    
    /// <summary>
    /// Adds sender to service collection
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection AddMessageSender<TSender>(this IServiceCollection serviceCollection)
        where TSender : class, IMessageSender
    {
        serviceCollection.AddScoped<IMessageSender, TSender>();
        return serviceCollection;
    }
}