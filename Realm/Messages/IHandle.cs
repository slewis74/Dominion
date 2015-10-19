namespace Realm.Messages
{
    public interface IHandle<in TEvent> : IHandleDomainEvents
        where TEvent : IDomainEvent
    {
        void Handle(TEvent eventInstance);
    }
}