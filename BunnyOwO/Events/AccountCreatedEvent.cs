namespace BunnyOwO.Events;

public class AccountCreatedEvent : IEvent
{
    public Guid AccountGuid { get; set; }
}