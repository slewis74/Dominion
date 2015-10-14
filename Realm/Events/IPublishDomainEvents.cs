namespace Realm.Events
{
    public interface IPublishDomainEvents
    {
        void SetPublisher(IEventPublisher broker);
    }
}