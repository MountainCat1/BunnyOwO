using System.Threading.Tasks;

namespace BunnyOwO;

public interface IEventHandler
{
    public virtual void ConfigureReceiver(IEventReceiver eventReceiver)
    {
        // Intentionally empty
    }
}
public interface IEventHandler<in TEvent> : IEventHandler
    where TEvent : class, IEvent
{
    public Task<bool> HandleAsync(TEvent @event);
}
