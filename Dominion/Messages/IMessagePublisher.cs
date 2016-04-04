using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dominion.Messages
{
    public interface IMessagePublisher { 
        Task PublishAsync<TEvent>(TEvent eventInstance) where TEvent : class, IDomainEvent;

        Task<TResponse> RequestAsync<TRequest, TResponse>(IDomainRequest<TRequest, TResponse> request)
            where TRequest : IDomainRequest<TRequest, TResponse>
            where TResponse : IDomainResponse;

        Task<IEnumerable<TResponse>> MulticastRequestAsync<TRequest, TResponse>(IDomainRequest<TRequest, TResponse> request)
            where TRequest : IDomainRequest<TRequest, TResponse>
            where TResponse : IDomainResponse;

    }
}