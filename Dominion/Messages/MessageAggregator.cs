using System;

namespace Dominion.Messages
{
    public class MessageAggregator
    {
        private static IMessageBroker _messageBroker;

        public static void SetBroker(IMessageBroker messageBroker)
        {
            _messageBroker = messageBroker;
        }

        public static void Subscribe(Type @event, Type handler)
        {
            if (_messageBroker == null)
                throw new InvalidOperationException("messageBroker has not been set");
            _messageBroker.Subscribe(@event, handler);
        }

        public static void Publish<T>(T @event) where T : class, IDomainEvent
        {
            if (_messageBroker == null)
                throw new InvalidOperationException("messageBroker has not been set");
            _messageBroker.Publish(@event);
        }
    }
}