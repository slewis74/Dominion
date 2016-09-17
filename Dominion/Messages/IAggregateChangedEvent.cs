namespace Dominion.Messages
{
    public interface IAggregateChangedEvent
    {
    }

    public interface IAggregateChangedEvent<out TId> : IAggregateEvent<TId>, IAggregateChangedEvent
    {
    }
}