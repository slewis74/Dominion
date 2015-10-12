namespace Realm.Events
{
    public interface IPublishDomainEvents
    {
        void SetBroker(IEventBroker broker);
    }
}