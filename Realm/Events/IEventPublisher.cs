namespace Realm.Events
{
    public interface IEventPublisher { 
        void Publish<TEvent>(TEvent eventInstance) where TEvent : class, IDomainEvent;
    }
}