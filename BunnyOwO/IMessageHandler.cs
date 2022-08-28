using System.Threading.Tasks;

namespace BunnyOwO;

public interface IMessageHandler
{
    /// <summary>
    /// Configure <see cref="IMessageReceiver"/> that will be assigned to 
    /// </summary>
    /// <param name="messageReceiver"><see cref="IMessageReceiver"/> that will be configured</param>
    public virtual void ConfigureReceiver(IMessageReceiver messageReceiver)
    {
        // Intentionally empty
    }
}
public interface IMessageHandler<in TMessage> : IMessageHandler
    where TMessage : class, IMessage
{
    /// <summary>
    /// Handle incoming broker <see cref="IMessage"/>
    /// </summary>
    /// <param name="message">Incoming event</param>
    /// <returns>Should event be consumed</returns>
    public Task<bool> HandleAsync(TMessage message);
}
