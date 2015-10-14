Realm
=====

Infrastructure for building Domains.

#Intro
Realm contains interface definitions and implementations for a number of components that can be used when building Domains.

Using Realm, Domain Events can be brokered between publishers and registered subscribers.  A subscriber is a registered Type, not an instance of the Type, which implements the IHandle<> interface.

# EventBroker
Realm contains an EventBroker, which manages the type subscriptions for the event handlers, and publishes event instances to them.

```csharp
	eventBroker.Publish(new SomeDomainEvent(someData));
```

##EventPublishingChildScopeBehaviour
When you create/resolve an instance of the EventBroke it requires 2 parameters.  The first is an Autofac ILifetimeScope and the second is an enum describing the child lifetime scope behaviour.

Why are these required?  The EventBroker subscribe accepts Types that are being registered, so when an event is published it needs to be able to create instances of the Types to pass the event to.  I've chosen to do this by resolving from the Autofac LifetimeScope.

But sometimes, like when you're using the EventAggregator descibed below, the EventBroker is a Singleton, and the LifetimeScope is therefore the root scope.  If we have types registered in the container as InstancePerLifetimeScope, and they are used by the handlers, they then become singleton and live for the life of the container, i.e. across Publish calls.  This is generally not desired behaviour.  What you really want is a child lifetime scope either per handler instance or per call to Publish (i.e. all handlers created during the Publish use the same scope).  This is what the second parameter lets you control.

In the scenario where you are not using the EventAggregator, you may chose for example to have the EventBroker registered as InstancePerRequest.  In this case it may make sense to just resolve the handlers from the same scope, in which case you'd use NoChildScopes.  It is of course still totally valid to use either of the other 2 options, the choice will depend on your app.

#EventAggregator
There is also a static EventAggregator class, which provides a static interface to an EventBroker.  For example, you can do the following from a domain method

```csharp
	EventAggregator.Publish(new SomeDomainEvent(someData));
```

To use this approach you need to resolve an IEventBroker from the container pass it to the static SetBroker method on the EventAggregator class during startup.  This is simple to setup but has some limitations.

The limitation is that the static forces you to a Singleton instance of the EventBroker, which means you are forced to register things like DbContexts as InstancePerLifetimeScope, not InstancePerRequest.  This can be problemmatic on several levels, the main one being that the handlers end up with a different DbContext (ie connection) than the original call.

# Entity Framework
When working with Entity Framework my preference is to not use the static EventAggregator.  Instead, I prefer to hook into the ObjectMaterialized event and use it to "Property Inject" the IEventPublisher into the entities that have been loaded from the database.
 
To make that work you'll want something like the following:

```csharp
	public static class DbContextExtensions
    {
        public static void OnObjectOfTypeMaterialized<T>(this DbContext context, Action<T> callback)
			where T : class
        {
            (context as IObjectContextAdapter).ObjectContext.ObjectMaterialized += (sender, args) =>
            {
                var t = args.Entity as T;
                if (t != null)
                    callback(t);
            };
        }
    }

```

This listens to the ObjectMaterialized event, and if the object is of the generics type then it gets passed to the callback action.  We can then do the following when the DbContext is registered in Autofac. 

```csharp
    builder.RegisterType<DbContext>()
        .As<IDataContext>()
        .WithParameter("defaultConnection", "some connection string")
        .OnActivated(c =>
        {
            var publisher = c.Context.Resolve<IEventPublisher>();
            c.Instance.OnObjectOfTypeMaterialized<IPublishDomainEvents>(t => t.SetPublisher(publisher));
        })
        .InstancePerRequest();

```

**But wait, there's more!!!**  You don't need to write those yourself, Realm.EntityFramework contains not only the DbContextExtensions implementation shown above, it contains a WithEventPublisherOnMaterialized registration extension, so you don't even need to worry about the DbContextExtensions yourself, you just do the following    

```csharp
    builder.RegisterType<DbContext>()
        .As<IDataContext>()
        .WithParameter("defaultConnection", "some connection string")
        .WithEventPublisherOnMaterialized()
        .InstancePerRequest();

```

#EventBroker container registration
Last, but not least, some help with registering the Event Handler types.  Realm contains some extensions methods for helping with scanning an Assembly for Handler types, you'll want something similar to the following

```csharp
    builder.RegisterType<EventBroker>()
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