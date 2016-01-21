using System.Threading.Tasks;

namespace Dominion.Messages
{
    public interface IHandleRequestAsync<in TRequest, TResponse> : IHandleDomainRequests
        where TRequest : IDomainRequest<TRequest, TResponse>
        where TResponse : IDomainResponse
    {
        Task<TResponse> HandleAsync(TRequest request);
    }
}