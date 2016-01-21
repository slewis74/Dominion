using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dominion.Messages
{
    public interface IMessagePublisher { 
        Task Publish<TEvent>(TEvent eventInstance) where TEvent : class, IDomainEvent;

        Task<TResponse> Request<TRequest, TResponse>(IDomainRequest<TRequest, TResponse> request)
            where TRequest : IDomainRequest<TRequest, TResponse>
            where TResponse : IDomainResponse;

        Task<IEnumerable<TResponse>> MulticastRequest<TRequest, TResponse>(IDomainRequest<TRequest, TResponse> request)
            where TRequest : IDomainRequest<TRequest, TResponse>
            where TResponse : IDomainResponse;

    }
}