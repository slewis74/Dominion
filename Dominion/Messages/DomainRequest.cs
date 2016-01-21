using System;

namespace Dominion.Messages
{
    public abstract class DomainRequest<TRequest, TResponse> : IDomainRequest<TRequest, TResponse>
        where TRequest : IDomainRequest<TRequest, TResponse>
        where TResponse : IDomainResponse
    {
        protected DomainRequest()
        {
            // The self referencing in the TRequest is so the calls to RequestBroker.MulticastRequest can
            // imply the types from the request object instance.
            if (GetType() != typeof(TRequest))
                throw new ArgumentException("Requests must reference their own type as the first generic type parameter");
        }
    }
}