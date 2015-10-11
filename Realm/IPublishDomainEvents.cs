namespace Realm
{
    public interface IPublishDomainEvents
    {
        void SetBroker(IEventBroker broker);
    }
}