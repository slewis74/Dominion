namespace Realm.Messages
{
    public interface IHandleRequest<in TRequest, out TResponse> : IHandleDomainRequests
        where TRequest : IDomainRequest<TRequest, TResponse>
        where TResponse : IDomainResponse
    {
        TResponse Handle(TRequest request);
    }
}