using System;

namespace Dominion.Messages
{
    public class AggregateCreatedEvent<TAggregate, TId> : IAggregateCreatedEvent<TId>, IDomainEvent
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

        public Type AggregateType { get; protected set; }
        public TId AggregateId { get; protected set; }
    }
}