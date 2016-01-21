using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Autofac;

namespace Dominion.Messages
{
    public class MessageBroker : IMessageBroker
    {
        private readonly EventBroker _eventBroker;
        private readonly RequestBroker _requestBroker;

        public MessageBroker(ILifetimeScope lifetimeScope, MessagePublishingChildScopeBehaviour childScopeBehaviour)
        {
            _eventBroker = new EventBroker(lifetimeScope, childScopeBehaviour);
            _requestBroker = new RequestBroker(lifetimeScope, childScopeBehaviour);
        }

        public void Subscribe(Type message, Type handler)
        {
            if (typeof(IDomainEvent).IsAssignableFrom(message))
                _eventBroker.Subscribe(message, handler);
            else if (typeof(IDomainRequest).IsAssignableFrom(message))
                _requestBroker.Subscribe(message, handler);
            else
                throw new ArgumentException("{0} must implement IDomainEvent or IRequest", message.Name);
        }

        public async Task Publish<TEvent>(TEvent eventInstance) where TEvent : class, IDomainEvent
        {
            await _eventBroker.Publish(eventInstance);
        }

        public async Task<TResponse> Request<TRequest, TResponse>(IDomainRequest<TRequest, TResponse> request) 
            where TRequest : IDomainRequest<TRequest, TResponse> 
            where TResponse : IDomainResponse
        {
            return await _requestBroker.Request(request);
        }

        public async Task<IEnumerable<TResponse>> MulticastRequest<TRequest, TResponse>(IDomainRequest<TRequest, TResponse> request) 
            where TRequest : IDomainRequest<TRequest, TResponse> 
            where TResponse : IDomainResponse
        {
            return await _requestBroker.MulticastRequest(request);
        }
    }
}