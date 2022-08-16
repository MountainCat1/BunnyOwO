using System.Threading.Tasks;

namespace BunnyOwO;

public interface IEventHandler
{
    /// <summary>
    /// Configure <see cref="IEventReceiver"/> that will be assigned to 
    /// </summary>
    /// <param name="eventReceiver"><see cref="IEventReceiver"/> that will be configured</param>
    public virtual void ConfigureReceiver(IEventReceiver eventReceiver)
    {
        // Intentionally empty
    }
}
public interface IEventHandler<in TEvent> : IEventHandler
    where TEvent : class, IEvent
{
    /// <summary>
    /// Handle incoming broker <see cref="IEvent"/>
    /// </summary>
    /// <param name="event">Incoming event</param>
    /// <returns>Should event be consumed</returns>
    public Task<bool> HandleAsync(TEvent @event);
}
