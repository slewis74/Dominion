using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Realm.Events;
using Shouldly;

namespace Realm.Tests.EventBrokerScenarios
{
    [TestClass]
    public class EventBrokerWithoutChildScopesScenarios
    {
        private static IContainer _container;
        private EventBroker _subject;

        [TestInitialize]
        public void SetUp()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<TestSyncHandler>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<TestAsyncHandler>().AsSelf().InstancePerLifetimeScope();

            _container = builder.Build();
            _subject = new EventBroker(_container, EventPublishingChildScopeBehaviour.NoChildScopes);
        }

        [TestMethod]
        public void HandlerGetsCalled()
        {
            TestSyncHandler.HandleGotCalled = false;

            _subject.Subscribe(typeof(TestEvent), typeof(TestSyncHandler));

            var e = new TestEvent();
            _subject.Publish(e);

            TestSyncHandler.HandleGotCalled.ShouldBe(true);
        }

        [TestMethod]
        public void SyncLifetimeScopeIsTheContainer()
        {
            _subject.Subscribe(typeof(TestEvent), typeof(TestSyncHandler));

            var e = new TestEvent();
            _subject.Publish(e);

            TestSyncHandler.LifetimeScopeWasContainer.ShouldBe(true);
        }

        [TestMethod]
        public void AsyncLifetimeScopeIsTheContainer()
        {
            _subject.Subscribe(typeof(TestEvent), typeof(TestAsyncHandler));
            TestAsyncHandler.ResetEvent = new ManualResetEvent(false);

            var e = new TestEvent();
            _subject.Publish(e);

            TestAsyncHandler.ResetEvent.WaitOne(500);
            TestAsyncHandler.LifetimeScopeWasContainer.ShouldBe(true);
        }

        [TestMethod]
        public void AsyncHandlerReturnsImmediately()
        {
            TestAsyncHandler.HandleGotCalled = false;

            _subject.Subscribe(typeof(TestEvent), typeof(TestAsyncHandler));

            var e = new TestEvent();
            _subject.Publish(e);

            TestAsyncHandler.HandleGotCalled.ShouldBe(false);
        }

        [TestMethod]
        public void AsyncHandlerRunInTheBackground()
        {
            TestAsyncHandler.HandleGotCalled = false;
            TestAsyncHandler.ResetEvent = new ManualResetEvent(false);

            _subject.Subscribe(typeof(TestEvent), typeof(TestAsyncHandler));

            var e = new TestEvent();
            _subject.Publish(e);

            TestAsyncHandler.ResetEvent.WaitOne(500);

            TestAsyncHandler.HandleGotCalled.ShouldBe(true);
        }

        public class TestEvent : IDomainEvent { }

        public class TestSyncHandler : IHandle<TestEvent>
        {
            public TestSyncHandler(ILifetimeScope lifetimeScope)
            {
                LifetimeScopeWasContainer = lifetimeScope.Tag == _container.Tag;
            }

            public static bool LifetimeScopeWasContainer { get; set; }
            public static bool HandleGotCalled { get; set; }

            public void Handle(TestEvent args)
            {
                HandleGotCalled = true;
            }
        }

        public class TestAsyncHandler : IHandleAsync<TestEvent>
        {
            public TestAsyncHandler(ILifetimeScope lifetimeScope)
            {
                LifetimeScopeWasContainer = lifetimeScope.Tag == _container.Tag;
            }

            public static ManualResetEvent ResetEvent { get; set; }
            public static bool LifetimeScopeWasContainer { get; set; }
            public static bool HandleGotCalled { get; set; }

#pragma warning disable 1998
            public async Task HandleAsync(TestEvent args)
#pragma warning restore 1998
            {
                Console.WriteLine("Sleeping...");
                Thread.Sleep(50);
                Console.WriteLine("Awake again...");
                HandleGotCalled = true;
                Console.WriteLine("HandleGotCalled = true");

                ResetEvent?.Set();
                Console.WriteLine("Event set...");
            }
        }
    }
}
