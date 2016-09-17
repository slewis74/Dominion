using System;

namespace Dominion.Messages
{
    public abstract class AggregateChangedEvent<TAggregate, TId> : IAggregateChangedEvent<TId>, IDomainEvent
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
}