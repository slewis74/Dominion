namespace Dominion.Messages
{
    public interface IHandle<in TEvent> : IHandleDomainEvents
    {
        void Handle(TEvent eventInstance);
    }
}