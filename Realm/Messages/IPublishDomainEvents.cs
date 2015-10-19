namespace Realm.Messages
{
    public interface IPublishDomainEvents
    {
        void SetPublisher(IMessagePublisher broker);
    }
}