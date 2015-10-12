using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;

namespace Realm.Events
{
    public class EventBroker : IEventBroker
    {
        private readonly IDictionary<Type, IList<Type>> _handlers = new Dictionary<Type, IList<Type>>();
        private readonly IDictionary<Type, IList<Type>> _asyncHandlers = new Dictionary<Type, IList<Type>>();
        private readonly ILifetimeScope _lifetimeScope;

        public EventBroker(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public void Subscribe(Type @event, Type handler)
        {
            if (!typeof(IDomainEvent).IsAssignableFrom(@event))
                throw new ArgumentException("{0} must implement IDomainEvent", @event.Name);

            if (typeof(IHandle<>).MakeGenericType(@event).IsAssignableFrom(handler))
            {
                if (_handlers.ContainsKey(@event))
                {
                    _handlers[@event].Add(handler);
                }
                else
                {
                    _handlers.Add(@event, new List<Type> { handler });
                }
            }

            if (typeof(IHandleAsync<>).MakeGenericType(@event).IsAssignableFrom(handler))
            {
                if (_asyncHandlers.ContainsKey(@event))
                {
                    _asyncHandlers[@event].Add(handler);
                }
                else
                {
                    _asyncHandlers.Add(@event, new List<Type> { handler });
                }
            }
        }

        public void Publish<T>(T @event) where T : class, IDomainEvent
        {
            var foundHandlers = GetHandlers(@event.GetType()).ToList();
            if (foundHandlers.Any())
            {
                foreach (var handler in foundHandlers)
                {
                    ExecuteHandler(@event, handler);
                }
            }

            foundHandlers = GetAsyncHandlers(@event.GetType()).ToList();
            if (foundHandlers.Any())
            {
                Task.Run(async () =>
                {
                    foreach (var handler in foundHandlers)
                    {
                        await ExecuteHandlerAsync(@event, handler);
                    }
                });
            }
        }

        private void ExecuteHandler(IDomainEvent @event, Type handlerType)
        {
            using (var childLifetimeScope = _lifetimeScope.BeginLifetimeScope())
            {
                var handler = (dynamic)childLifetimeScope.Resolve(handlerType);
                handler.Handle((dynamic)@event);
            }
        }

        private async Task ExecuteHandlerAsync(IDomainEvent @event, Type handlerType)
        {
            using (var childLifetimeScope = _lifetimeScope.BeginLifetimeScope())
            {
                var handler = (dynamic)childLifetimeScope.Resolve(handlerType);
                await handler.Handle((dynamic)@event);
            }
        }

        private IEnumerable<Type> GetHandlers(Type eventType)
        {
            var types = eventType.GetInterfaces().Union(new[] { eventType });

            foreach (var type in types)
            {
                IList<Type> foundHandlers;
                if (!_handlers.TryGetValue(type, out foundHandlers))
                    continue;

                foreach (var handler in foundHandlers)
                    yield return handler;
            }
        }

        private IEnumerable<Type> GetAsyncHandlers(Type eventType)
        {
            var types = eventType.GetInterfaces().Union(new[] { eventType });

            foreach (var type in types)
            {
                IList<Type> foundHandlers;
                if (!_asyncHandlers.TryGetValue(type, out foundHandlers))
                    continue;

                foreach (var handler in foundHandlers)
                    yield return handler;
            }
        }
    }
}