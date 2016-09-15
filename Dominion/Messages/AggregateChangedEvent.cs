using System;

namespace Dominion.Messages
{
    public abstract class AggregateChangedEvent<TAggregate, TId> : IAggregateChangedEvent, IDomainEvent
        where TAggregate : IAggregate<TId>
    {
        protected AggregateChangedEvent()
        {    
        }

        protected AggregateChangedEvent(TAggregate aggregate)
        {
            AggregateType = typeof(TAggregate);
            AggregateId = aggregate.Id;
        }

        public Type AggregateType { get; set; }
        public TId AggregateId { get; private set; }
    }

    public interface IAggregateChangedEvent
    {
    }

    public interface IAggregateCreatedEvent
    { }

    public class AggregateCreatedEvent<TAggregate, TId> : IAggregateCreatedEvent, IDomainEvent
        where TAggregate : IAggregate<TId>
    {
        protected AggregateCreatedEvent()
        {
        }

        protected AggregateCreatedEvent(TAggregate aggregate)
        {
            AggregateType = typeof(TAggregate);
            AggregateId = aggregate.Id;
        }

        public Type AggregateType { get; set; }
        public TId AggregateId { get; private set; }
    }
}