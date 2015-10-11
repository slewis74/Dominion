using System.Threading.Tasks;

namespace Realm
{
    public interface IHandleAsync<in TEvent> : IHandelDomainEvents
        where TEvent : IDomainEvent
    {
        Task HandleAsync(TEvent eventInstance);
    }
}