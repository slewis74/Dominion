using System;
using System.Linq;
using System.Reflection;

namespace Dominion.Messages
{
    public static class MessageBrokerExtensions
    {
        public static MessageBroker SubscribeHandlersInAssembly(this MessageBroker messageBroker, Assembly assembly)
        {
            var handlers = assembly.GetTypes().Where(t => t.GetInterfaces().Any(IsHandler));
            foreach (var handler in handlers)
            {
                var events = handler.GetInterfaces().Where(IsHandler).Select(type => type.GenericTypeArguments.Single());
                foreach (var @event in events)
                    messageBroker.Subscribe(@event, handler);
            }

            return messageBroker;
        }

        private static bool IsHandler(Type type)
        {
            var handlerInterfaceTypes = new[] { typeof(IHandle<>), typeof(IHandleAsync<>) };
            return type.IsGenericType && handlerInterfaceTypes.Contains(type.GetGenericTypeDefinition());
        }
    }
}