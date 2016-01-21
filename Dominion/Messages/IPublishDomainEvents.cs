namespace Dominion.Messages
{
    public interface IPublishDomainEvents
    {
        void SetPublisher(IMessagePublisher broker);
    }
}