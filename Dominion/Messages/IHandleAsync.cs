using System.Threading.Tasks;

namespace Dominion.Messages
{
    public interface IHandleAsync<in TEvent> : IHandleDomainEvents
        where TEvent : IDomainEvent
    {
        Task HandleAsync(TEvent eventInstance);
    }
}