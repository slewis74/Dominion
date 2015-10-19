namespace Realm.Messages
{
    public abstract class AggregateChangedEvent<TAggregate, TId> where TAggregate : IAggregate<TId>
    {
        protected AggregateChangedEvent(TAggregate aggregate)
        {
            Aggregate = aggregate;
        }

        public TAggregate Aggregate { get; private set; }
    }
}