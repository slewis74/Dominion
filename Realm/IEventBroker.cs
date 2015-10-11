namespace Realm
{
    public interface IEventBroker
    {
        void Publish<TEvent>(TEvent eventInstance) where TEvent : class, IDomainEvent;
    }
}