using System.Threading.Tasks;

namespace BunnyOwO;

public interface IEventHandler
{    public void ConfigureReceiver(IReceiver receiver);
}
public interface IEventHandler<in TEvent> : IEventHandler
    where TEvent : class, IEvent
{
    public Task<bool> HandleAsync(TEvent @event);
}
