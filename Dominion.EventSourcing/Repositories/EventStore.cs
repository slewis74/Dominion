using System;
using System.Collections.Generic;
using System.Linq;
using Dominion.Messages;

namespace Dominion.EventSourcing.Repositories
{
    public class EventStore : IEventStore
    {
        private readonly Dictionary<Type, Dictionary<object, List<IAggregateEvent>>> _events;

        public EventStore()
        {
            _events = new Dictionary<Type, Dictionary<object, List<IAggregateEvent>>>();
        }

        public IEnumerable<IAggregateEvent<TId>> Get<TAggregate, TId>(TId id)
            where TAggregate : IAggregate<TId>
        {
            var aggregateIdType = typeof(TId);
            if (!_events.ContainsKey(aggregateIdType) || !_events[aggregateIdType].ContainsKey(id))
                return Enumerable.Empty<IAggregateEvent<TId>>();
            return _events[aggregateIdType][id].Cast<IAggregateEvent<TId>>();
        }

        public void Store<TId>(IAggregateEvent<TId> @event)
        {
            Store(new [] { @event });
        }

        public void Store<TId>(IEnumerable<IAggregateEvent<TId>> @events)
        {
            var aggregateIdType = typeof(TId);
            if (!_events.ContainsKey(aggregateIdType))
            {
                _events.Add(aggregateIdType, new Dictionary<object, List<IAggregateEvent>>());
            }

            var eventsByAggregateIdType = _events[aggregateIdType];

            foreach (var @event in events)
            {
                if (!eventsByAggregateIdType.ContainsKey(@event.AggregateId))
                {
                    eventsByAggregateIdType.Add(@event.AggregateId, new List<IAggregateEvent>());
                }

                eventsByAggregateIdType[@event.AggregateId].Add(@event);
            }
        }
    }

    public interface IEventStore
    {
        IEnumerable<IAggregateEvent<TId>> Get<TAggregate, TId>(TId id)
            where TAggregate : IAggregate<TId>;

        void Store<TId>(IAggregateEvent<TId> @event);

        void Store<TId>(IEnumerable<IAggregateEvent<TId>> @events);
    }
}