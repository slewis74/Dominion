using System;

namespace Realm.Events
{
    public class EventAggregator
    {
        private static IEventBroker _eventBroker;

        public static void SetBroker(IEventBroker eventBroker)
        {
            _eventBroker = eventBroker;
        }

        public static void Subscribe(Type @event, Type handler)
        {
            if (_eventBroker == null)
                throw new InvalidOperationException("EventBroker has not been set");
            _eventBroker.Subscribe(@event, handler);
        }

        public static void Publish<T>(T @event) where T : class, IDomainEvent
        {
            if (_eventBroker == null)
                throw new InvalidOperationException("EventBroker has not been set");
            _eventBroker.Publish(@event);
        }
    }
}