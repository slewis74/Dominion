using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;

namespace Dominion.Messages
{
    internal class EventBroker
    {
        private readonly IDictionary<Type, IList<Type>> _eventHandlers = new Dictionary<Type, IList<Type>>();
        private readonly IDictionary<Type, IList<Type>> _asyncEventHandlers = new Dictionary<Type, IList<Type>>();
        private readonly ILifetimeScope _lifetimeScope;
        private readonly MessagePublishingChildScopeBehaviour _childScopeBehaviour;

        public EventBroker(ILifetimeScope lifetimeScope, MessagePublishingChildScopeBehaviour childScopeBehaviour)
        {
            _lifetimeScope = lifetimeScope;
            _childScopeBehaviour = childScopeBehaviour;
        }

        public void Subscribe(Type @event, Type handler)
        {
            if (!typeof(IDomainEvent).IsAssignableFrom(@event))
                throw new ArgumentException("{0} must implement IDomainEvent", @event.Name);

            if (typeof(IHandle<>).MakeGenericType(@event).IsAssignableFrom(handler))
            {
                if (_eventHandlers.ContainsKey(@event))
                {
                    _eventHandlers[@event].Add(handler);
                }
                else
                {
                    _eventHandlers.Add(@event, new List<Type> { handler });
                }
            }

            if (typeof(IHandleAsync<>).MakeGenericType(@event).IsAssignableFrom(handler))
            {
                if (_asyncEventHandlers.ContainsKey(@event))
                {
                    _asyncEventHandlers[@event].Add(handler);
                }
                else
                {
                    _asyncEventHandlers.Add(@event, new List<Type> { handler });
                }
            }
        }

        public async Task PublishAsync<T>(T @event) where T : class, IDomainEvent
        {
            if (_childScopeBehaviour == MessagePublishingChildScopeBehaviour.ChildScopePerMessage)
            {
                using (var childLifetimeScope = _lifetimeScope.BeginLifetimeScope())
                {
                    await DoPublishAsync(@event, childLifetimeScope).ConfigureAwait(continueOnCapturedContext: false);
                }
            }
            else
            {
                await DoPublishAsync(@event, _lifetimeScope).ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        private async Task DoPublishAsync<T>(T @event, ILifetimeScope lifetimeScope) where T : class, IDomainEvent
        {
            var foundHandlers = GetEventHandlers(@event.GetType()).ToList();
            if (foundHandlers.Any())
            {
                foreach (var handler in foundHandlers)
                {
                    ExecuteEventHandler(@event, handler, lifetimeScope);
                }
            }

            foundHandlers = GetAsyncEventHandlers(@event.GetType()).ToList();
            if (foundHandlers.Any())
            {
                foreach (var handler in foundHandlers)
                {
                    await ExecuteEventHandlerAsync(@event, handler, lifetimeScope).ConfigureAwait(continueOnCapturedContext: false);
                }
            }
        }

        private void ExecuteEventHandler(IDomainEvent @event, Type handlerType, ILifetimeScope lifetimeScope)
        {
            if (_childScopeBehaviour == MessagePublishingChildScopeBehaviour.ChildScopePerHandler)
            {
                using (var childLifetimeScope = _lifetimeScope.BeginLifetimeScope())
                {
                    CallEventHandler(@event, handlerType, childLifetimeScope);
                }
            }
            else
            {
                CallEventHandler(@event, handlerType, lifetimeScope);
            }
        }

        private static void CallEventHandler(IDomainEvent @event, Type handlerType, ILifetimeScope lifetimeScope)
        {
            var handler = (dynamic)lifetimeScope.Resolve(handlerType);
            handler.Handle((dynamic)@event);
        }

        private async Task ExecuteEventHandlerAsync(IDomainEvent @event, Type handlerType, ILifetimeScope lifetimeScope)
        {
            if (_childScopeBehaviour == MessagePublishingChildScopeBehaviour.ChildScopePerHandler)
            {
                using (var childLifetimeScope = _lifetimeScope.BeginLifetimeScope())
                {
                    await CallEventHandlerAsync(@event, handlerType, childLifetimeScope).ConfigureAwait(continueOnCapturedContext: false);
                }
            }
            else
            {
                await CallEventHandlerAsync(@event, handlerType, lifetimeScope).ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        private static async Task CallEventHandlerAsync(IDomainEvent @event, Type handlerType, ILifetimeScope lifetimeScope)
        {
            var handler = (dynamic)lifetimeScope.Resolve(handlerType);
            await handler.HandleAsync((dynamic)@event).ConfigureAwait(continueOnCapturedContext: false);
        }

        private IEnumerable<Type> GetEventHandlers(Type eventType)
        {
            var types = eventType.GetInterfaces().Union(new[] { eventType });

            foreach (var type in types)
            {
                IList<Type> foundHandlers;
                if (!_eventHandlers.TryGetValue(type, out foundHandlers))
                    continue;

                foreach (var handler in foundHandlers)
                    yield return handler;
            }
        }

        private IEnumerable<Type> GetAsyncEventHandlers(Type eventType)
        {
            var types = eventType.GetInterfaces().Union(new[] { eventType });

            foreach (var type in types)
            {
                IList<Type> foundHandlers;
                if (!_asyncEventHandlers.TryGetValue(type, out foundHandlers))
                    continue;

                foreach (var handler in foundHandlers)
                    yield return handler;
            }
        }
    }
}