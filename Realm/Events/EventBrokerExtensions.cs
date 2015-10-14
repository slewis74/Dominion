using System;
using System.Linq;
using System.Reflection;

namespace Realm.Events
{
    public static class EventBrokerExtensions
    {
        public static IEventBroker SubscribeHandlersInAssembly(this IEventBroker eventBroker, Assembly assembly)
        {
            var handlers = assembly.GetTypes().Where(t => t.GetInterfaces().Any(IsHandler));
            foreach (var handler in handlers)
            {
                var events = handler.GetInterfaces().Where(IsHandler).Select(type => type.GenericTypeArguments.Single());
                foreach (var @event in events)
                    eventBroker.Subscribe(@event, handler);
            }

            return eventBroker;
        }

        private static bool IsHandler(Type type)
        {
            var handlerInterfaceTypes = new[] { typeof(IHandle<>), typeof(IHandleAsync<>) };
            return type.IsGenericType && handlerInterfaceTypes.Contains(type.GetGenericTypeDefinition());
        }
    }
}