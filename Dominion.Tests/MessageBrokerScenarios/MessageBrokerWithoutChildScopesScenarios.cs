using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Dominion.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Dominion.Tests.MessageBrokerScenarios
{
    [TestClass]
    public class MessageBrokerWithoutChildScopesScenarios
    {
        private static IContainer _container;
        private MessageBroker _subject;

        [TestInitialize]
        public void SetUp()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<TestSyncHandler>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<TestAsyncHandler>().AsSelf().InstancePerLifetimeScope();

            _container = builder.Build();
            _subject = new MessageBroker(_container, MessagePublishingChildScopeBehaviour.NoChildScopes);
        }

        [TestMethod]
        public async Task HandlerGetsCalled()
        {
            TestSyncHandler.HandleGotCalled = false;

            _subject.Subscribe(typeof(TestEvent), typeof(TestSyncHandler));

            var e = new TestEvent();
            await _subject.PublishAsync(e);

            TestSyncHandler.HandleGotCalled.ShouldBe(true);
        }

        [TestMethod]
        public async Task SyncLifetimeScopeIsTheContainer()
        {
            _subject.Subscribe(typeof(TestEvent), typeof(TestSyncHandler));

            var e = new TestEvent();
            await _subject.PublishAsync(e);

            TestSyncHandler.LifetimeScopeWasContainer.ShouldBe(true);
        }

        [TestMethod]
        public async Task AsyncLifetimeScopeIsTheContainer()
        {
            _subject.Subscribe(typeof(TestEvent), typeof(TestAsyncHandler));
            TestAsyncHandler.ResetEvent = new ManualResetEvent(false);

            var e = new TestEvent();
            await _subject.PublishAsync(e);

            TestAsyncHandler.ResetEvent.WaitOne(500);
            TestAsyncHandler.LifetimeScopeWasContainer.ShouldBe(true);
        }

        [TestMethod]
        public async Task AsyncHandlerExecutesBeforeReturn()
        {
            TestAsyncHandler.HandleGotCalled = false;

            _subject.Subscribe(typeof(TestEvent), typeof(TestAsyncHandler));

            var e = new TestEvent();
            await _subject.PublishAsync(e);

            TestAsyncHandler.HandleGotCalled.ShouldBe(true);
        }

        [TestMethod]
        public async Task AsyncHandlerRunInTheBackground()
        {
            TestAsyncHandler.HandleGotCalled = false;
            TestAsyncHandler.ResetEvent = new ManualResetEvent(false);

            _subject.Subscribe(typeof(TestEvent), typeof(TestAsyncHandler));

            var e = new TestEvent();
            await _subject.PublishAsync(e);

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
