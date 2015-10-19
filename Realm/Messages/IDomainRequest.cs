namespace Realm.Messages
{
    public interface IDomainRequest
    { }

    public interface IDomainRequest<in TRequest, TResponse> : IDomainRequest
        where TRequest : IDomainRequest<TRequest, TResponse>
        where TResponse : IDomainResponse
    {
        
    }
}