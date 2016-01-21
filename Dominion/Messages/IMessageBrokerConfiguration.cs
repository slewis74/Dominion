using System;

namespace Dominion.Messages
{
    public interface IMessageBrokerConfiguration
    {
        void Subscribe(Type message, Type handler);
    }
}