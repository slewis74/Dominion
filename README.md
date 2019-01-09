Dominion
=====

Infrastructure for building Domains.

# Intro
Dominion contains interface definitions and implementations for a number of components that can be used when building Domains.


# Messages
Using Dominion, Domain Events can be brokered between publishers and registered subscribers.  A subscriber is a registered Type, not an instance of the Type, which implements the IHandle<> interface.  The instance is resolved at the time the event is published, for more details see MessagePublishingChildScopeBehaviour.

There is also support for Requests, which can be sent to subscribers to determine something beyond the knowledge held by the current aggregate/service.

## MessageBroker and MessagePublisher
Dominion contains an MessageBroker, which manages the type subscriptions for the event handlers, and publishes event instances to them.  Typically you'd access the broker via IMessageBroker during startup/configuration and via IMessagePublisher in all subsequent interactions.

```csharp
	messagePublisher.Publish(new SomeDomainEvent(someData));
```

## MessagePublishingChildScopeBehaviour
When you create/resolve an instance of the MessageBroker it requires 2 parameters.  The first is an Autofac ILifetimeScope and the second is an enum describing the child lifetime scope behaviour.

The child lifetime scope behaviour lets you control whether the resolved subscriber instances are resolved:

- Directly from the given ILifetimeScope (NoChildScopes)
- From a single new child lifetime scope (ChildScopePerMessage)
- From a new child lifetime scope for each subscriber (ChildScopePerHandler)

A typical usage scenario might involve centering your container configuration around InstancePerRequest, including the lifetime of the MessageBroker itself.  You might then use NoChildScopes when configuring the broker, so the handlers are all resolved in the same scope as the request.

The other 2 options would also work in when the broker is registered as InstancePerRequest, depending on what other components you're using and what their requirements are.

Whilst the broker can be registered as a Singleton, it is not recommended.  Handler dependencies to DbContexts etc, which are typcially registered InstancePerRequest, will then cause runtime issues.  Changing the DbContexts to InstancePerLifetimeScope can alleviate some of those issues, but will cause others.

# Entity Framework
When working with Entity Framework my preference is to hook into the ObjectMaterialized event and use it to "Property Inject" the IMessagePublisher into the entities that have been loaded from the database.

The is an extension method called WithEventPublisherOnMaterialized that can be used during registration as follows:

```csharp
    builder.RegisterType<DbContext>()
        .As<IDataContext>()
        .WithParameter("defaultConnection", "some connection string")
        .WithEventPublisherOnMaterialized()
        .InstancePerRequest();

```

# MessageBroker container registration
Last, but not least, some help with registering the Handler types.  Dominion contains some extension methods for helping with scanning an Assembly for Handler types, you'll want something similar to the following

```csharp
    builder.RegisterType<MessageBroker>()
		.AsImplementedInterfaces()
		.OnActivated(c =>
	    {
	        c.Instance
	            .SubscribeHandlersInAssembly(Assembly.GetAssembly(typeof(SomeType)))
	            .SubscribeHandlersInAssembly(Assembly.GetAssembly(typeof(ThisModule)));
	    })
		.InstancePerRequest();

```
The number of calls will depend on your app, and how many Assemblies you have with handlers that need to be registered.
