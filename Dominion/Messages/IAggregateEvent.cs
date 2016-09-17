using System;

namespace Dominion.Messages
{
    public interface IAggregateEvent
    {
        Type AggregateType { get; }
    }

    public interface IAggregateEvent<out TId> : IAggregateEvent
    {
        TId AggregateId { get; }
    }
}