using System.Threading.Tasks;

namespace Realm.Messages
{
    public interface IHandleAsync<in TEvent> : IHandleDomainEvents
        where TEvent : IDomainEvent
    {
        Task HandleAsync(TEvent eventInstance);
    }
}