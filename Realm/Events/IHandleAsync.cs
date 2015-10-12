using System.Threading.Tasks;

namespace Realm.Events
{
    public interface IHandleAsync<in TEvent> : IHandelDomainEvents
        where TEvent : IDomainEvent
    {
        Task HandleAsync(TEvent eventInstance);
    }
}