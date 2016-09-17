using System;
using System.Collections.Generic;
using Dominion.Messages;

namespace Dominion.Tests.EventSourcing.EventStoreScenarios.SampleDomain
{
    public class SampleAggregate : IAggregate<Guid>,
        IHandle<SampleAggregateChangedNameEvent>
    {
        public SampleAggregate()
        {
            Id = Guid.NewGuid();
        }

        public SampleAggregate(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; }

        public IEnumerable<SampleAggregateChangedEvent> ChangeName(string name)
        {
            return new[] {new SampleAggregateChangedNameEvent(this, name)};
        }

        public void Handle(SampleAggregateChangedNameEvent eventInstance)
        {
            Name = eventInstance.NewName;
        }
    }

    public class SampleAggregateCreatedEvent : AggregateCreatedEvent<SampleAggregate, Guid>
    {
        protected SampleAggregateCreatedEvent()
        {}

        protected SampleAggregateCreatedEvent(SampleAggregate aggregate) : base(aggregate)
        {}
    }

    public abstract class SampleAggregateChangedEvent : AggregateChangedEvent<SampleAggregate, Guid>
    {
        protected SampleAggregateChangedEvent()
        {}

        protected SampleAggregateChangedEvent(SampleAggregate aggregate) : base(aggregate)
        {}
    }

    public class SampleAggregateChangedNameEvent : SampleAggregateChangedEvent
    {
        public SampleAggregateChangedNameEvent()
        {}

        public SampleAggregateChangedNameEvent(SampleAggregate aggregate, string newName) : base(aggregate)
        {
            NewName = newName;
        }

        public string NewName { get; set; }
    }
}