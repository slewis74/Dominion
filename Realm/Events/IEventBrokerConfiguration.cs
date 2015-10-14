using System;

namespace Realm.Events
{
    public interface IEventBrokerConfiguration
    {
        void Subscribe(Type @event, Type handler);
    }
}