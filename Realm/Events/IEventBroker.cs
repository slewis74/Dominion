namespace Realm.Events
{
    public interface IEventBroker
    {
        void Publish<TEvent>(TEvent eventInstance) where TEvent : class, IDomainEvent;
    }
}