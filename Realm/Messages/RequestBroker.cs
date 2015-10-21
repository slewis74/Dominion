using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;

namespace Realm.Messages
{
    internal class RequestBroker
    {
        private readonly IDictionary<Type, IList<Type>> _requestHandlers = new Dictionary<Type, IList<Type>>();
        private readonly IDictionary<Type, IList<Type>> _asyncRequestHandlers = new Dictionary<Type, IList<Type>>();
        private readonly ILifetimeScope _lifetimeScope;
        private readonly MessagePublishingChildScopeBehaviour _childScopeBehaviour;

        public RequestBroker(ILifetimeScope lifetimeScope, MessagePublishingChildScopeBehaviour childScopeBehaviour)
        {
            _lifetimeScope = lifetimeScope;
            _childScopeBehaviour = childScopeBehaviour;
        }

        public void Subscribe(Type request, Type handler)
        {
            if (!typeof(IDomainRequest).IsAssignableFrom(request))
                throw new ArgumentException("{0} must implement IDomainRequest", request.Name);

            var genericTypeArguments = request.BaseType.GenericTypeArguments;
            var requestType = genericTypeArguments[0];
            var responseType = genericTypeArguments[1];

            if (typeof(IHandleRequest<,>).MakeGenericType(requestType, responseType).IsAssignableFrom(handler))
            {
                if (_requestHandlers.ContainsKey(request))
                {
                    _requestHandlers[request].Add(handler);
                }
                else
                {
                    _requestHandlers.Add(request, new List<Type> { handler });
                }
            }

            if (typeof(IHandleRequestAsync<,>).MakeGenericType(requestType, responseType).IsAssignableFrom(handler))
            {
                if (_asyncRequestHandlers.ContainsKey(request))
                {
                    _asyncRequestHandlers[request].Add(handler);
                }
                else
                {
                    _asyncRequestHandlers.Add(request, new List<Type> { handler });
                }
            }

        }

        public async Task<TResponse> Request<TRequest, TResponse>(IDomainRequest<TRequest, TResponse> request) 
            where TRequest : IDomainRequest<TRequest, TResponse> 
            where TResponse : IDomainResponse
        {
            if (_childScopeBehaviour == MessagePublishingChildScopeBehaviour.ChildScopePerMessage)
            {
                using (var childLifetimeScope = _lifetimeScope.BeginLifetimeScope())
                {
                    return await DoRequest(request, childLifetimeScope);
                }
            }
            else
            {
                return await DoRequest(request, _lifetimeScope);
            }
        }

        private async Task<TResponse> DoRequest<TRequest, TResponse>(IDomainRequest<TRequest, TResponse> request, ILifetimeScope lifetimeScope)
            where TRequest : IDomainRequest<TRequest, TResponse>
            where TResponse : IDomainResponse
        {
            var foundHandlers = GetRequestHandlers(request.GetType()).ToList();
            if (foundHandlers.Any())
            {
                foreach (var handler in foundHandlers)
                {
                    var response = ExecuteRequestHandler(request, handler, lifetimeScope);
                    if (response != null)
                        return response;
                }
            }

            foundHandlers = GetAsyncEventHandlers(request.GetType()).ToList();
            if (foundHandlers.Any())
            {
                foreach (var handler in foundHandlers)
                {
                    var response = await ExecuteRequestHandlerAsync(request, handler, lifetimeScope);
                    if (response != null)
                        return response;
                }
            }

            return default(TResponse);
        }

        public async Task<IEnumerable<TResponse>> MulticastRequest<TRequest, TResponse>(IDomainRequest<TRequest, TResponse> request)
            where TRequest : IDomainRequest<TRequest, TResponse>
            where TResponse : IDomainResponse
        {
            if (_childScopeBehaviour == MessagePublishingChildScopeBehaviour.ChildScopePerMessage)
            {
                using (var childLifetimeScope = _lifetimeScope.BeginLifetimeScope())
                {
                    return await DoMulticastRequest(request, childLifetimeScope);
                }
            }
            else
            {
                return await DoMulticastRequest(request, _lifetimeScope);
            }
        }

        public async Task<IEnumerable<TResponse>> DoMulticastRequest<TRequest, TResponse>(IDomainRequest<TRequest, TResponse> request, ILifetimeScope lifetimeScope)
            where TRequest : IDomainRequest<TRequest, TResponse>
            where TResponse : IDomainResponse
        {
            var responses = new List<TResponse>();
            var foundHandlers = GetRequestHandlers(request.GetType()).ToList();
            if (foundHandlers.Any())
            {
                foreach (var handler in foundHandlers)
                {
                    var response = ExecuteRequestHandler(request, handler, lifetimeScope);
                    if (response != null)
                        responses.Add(response);
                }
            }

            foundHandlers = GetAsyncEventHandlers(request.GetType()).ToList();
            if (foundHandlers.Any())
            {
                foreach (var handler in foundHandlers)
                {
                    var response = await ExecuteRequestHandlerAsync(request, handler, lifetimeScope);
                    if (response != null)
                        responses.Add(response);
                }
            }

            return Enumerable.Empty<TResponse>();
        }

        private TResponse ExecuteRequestHandler<TRequest, TResponse>(IDomainRequest<TRequest, TResponse> request, Type handlerType, ILifetimeScope lifetimeScope)
            where TRequest : IDomainRequest<TRequest, TResponse>
            where TResponse : IDomainResponse
        {
            if (_childScopeBehaviour == MessagePublishingChildScopeBehaviour.ChildScopePerHandler)
            {
                using (var childLifetimeScope = _lifetimeScope.BeginLifetimeScope())
                {
                    return CallRequestHandler(request, handlerType, childLifetimeScope);
                }
            }
            else
            {
                return CallRequestHandler(request, handlerType, lifetimeScope);
            }
        }

        private static TResponse CallRequestHandler<TRequest, TResponse>(IDomainRequest<TRequest, TResponse> request, Type handlerType, ILifetimeScope lifetimeScope)
            where TRequest : IDomainRequest<TRequest, TResponse>
            where TResponse : IDomainResponse
        {
            var handler = (dynamic)lifetimeScope.Resolve(handlerType);
            return handler.Handle((dynamic)request);
        }

        private async Task<TResponse> ExecuteRequestHandlerAsync<TRequest, TResponse>(IDomainRequest<TRequest, TResponse> request, Type handlerType, ILifetimeScope lifetimeScope)
            where TRequest : IDomainRequest<TRequest, TResponse>
            where TResponse : IDomainResponse
        {
            if (_childScopeBehaviour == MessagePublishingChildScopeBehaviour.ChildScopePerHandler)
            {
                using (var childLifetimeScope = _lifetimeScope.BeginLifetimeScope())
                {
                    return await CallRequestHandlerAsync(request, handlerType, childLifetimeScope);
                }
            }
            else
            {
                return await CallRequestHandlerAsync(request, handlerType, lifetimeScope);
            }
        }

        private static async Task<TResponse> CallRequestHandlerAsync<TRequest, TResponse>(IDomainRequest<TRequest, TResponse> request, Type handlerType, ILifetimeScope lifetimeScope)
            where TRequest : IDomainRequest<TRequest, TResponse>
            where TResponse : IDomainResponse
        {
            var handler = (dynamic)lifetimeScope.Resolve(handlerType);
            return await handler.HandleAsync((dynamic)request);
        }

        private IEnumerable<Type> GetRequestHandlers(Type requestType)
        {
            var types = requestType.GetInterfaces().Union(new[] { requestType });

            foreach (var type in types)
            {
                IList<Type> foundHandlers;
                if (!_requestHandlers.TryGetValue(type, out foundHandlers))
                    continue;

                foreach (var handler in foundHandlers)
                    yield return handler;
            }
        }

        private IEnumerable<Type> GetAsyncEventHandlers(Type requestType)
        {
            var types = requestType.GetInterfaces().Union(new[] { requestType });

            foreach (var type in types)
            {
                IList<Type> foundHandlers;
                if (!_asyncRequestHandlers.TryGetValue(type, out foundHandlers))
                    continue;

                foreach (var handler in foundHandlers)
                    yield return handler;
            }
        }
    }
}