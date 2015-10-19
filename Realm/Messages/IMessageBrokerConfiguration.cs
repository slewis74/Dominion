using System;

namespace Realm.Messages
{
    public interface IMessageBrokerConfiguration
    {
        void Subscribe(Type message, Type handler);
    }
}