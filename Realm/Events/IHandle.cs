namespace Realm.Events
{
    public interface IHandle<in TEvent> : IHandelDomainEvents
        where TEvent : IDomainEvent
    {
        void Handle(TEvent eventInstance);
    }
}