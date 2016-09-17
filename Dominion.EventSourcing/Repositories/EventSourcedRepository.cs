using Dominion.Messages;

namespace Dominion.EventSourcing.Repositories
{
    public class EventSourcedRepository<TId> : IEventSourcedRepository<TId>, IHandle<IAggregateEvent<TId>>
    {
        private readonly IEventStore _eventStore;

        public EventSourcedRepository(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public TAggregate Get<TAggregate>(TId id)
            where TAggregate : IAggregate<TId>, new()
        {
            var events = _eventStore.Get<TAggregate, TId>(id);
            var aggregate = new TAggregate();
            var d = (dynamic) aggregate;

            // replay events into the object
            foreach (var @event in events)
            {
                d.Handle((dynamic)@event);
            }

            return aggregate;
        }

        public void Handle(IAggregateEvent<TId> eventInstance)
        {
            _eventStore.Store(eventInstance);
        }
    }

    public interface IEventSourcedRepository<in TId>
    {
        TAggregate Get<TAggregate>(TId id)
            where TAggregate : IAggregate<TId>, new();
    }
}