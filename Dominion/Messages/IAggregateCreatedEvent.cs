namespace Dominion.Messages
{
    public interface IAggregateCreatedEvent : IAggregateEvent
    { }

    public interface IAggregateCreatedEvent<out TId> : IAggregateEvent<TId>, IAggregateCreatedEvent
    { }
}