using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Realm.Events;
using Shouldly;

namespace Realm.Tests.EventBrokerScenarios
{
    [TestClass]
    public class EventBrokerWithChildScopePerPublishScenarios
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
            _subject = new EventBroker(_container, EventPublishingChildScopeBehaviour.ChildScopePerPublish);
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
        public void SyncLifetimeScopeIsntOfTheContainer()
        {
            _subject.Subscribe(typeof(TestEvent), typeof(TestSyncHandler));

            var e = new TestEvent();
            _subject.Publish(e);

            TestSyncHandler.LifetimeScopeWasContainer.ShouldBe(false);
        }

        [TestMethod]
        public void SyncLifetimeScopeIsntSameAsPrevious()
        {
            _lastLifetimeScopeTag = null;
            _subject.Subscribe(typeof(TestEvent), typeof(TestSyncHandler));

            var e = new TestEvent();
            _subject.Publish(e);

            TestSyncHandler.LifetimeScopeTagWasSameAsPrevious.ShouldBe(false);
        }

        [TestMethod]
        public void AsyncLifetimeScopeIsntOfTheContainer()
        {
            _subject.Subscribe(typeof(TestEvent), typeof(TestAsyncHandler));
            TestAsyncHandler.ResetEvent = new ManualResetEvent(false);

            var e = new TestEvent();
            _subject.Publish(e);

            TestAsyncHandler.ResetEvent.WaitOne(500);
            TestAsyncHandler.LifetimeScopeWasContainer.ShouldBe(false);
        }

        [TestMethod]
        public void AsyncLifetimeScopeIsSameAsPrevious()
        {
            _lastLifetimeScopeTag = null;
            _subject.Subscribe(typeof(TestEvent), typeof(TestSyncHandler));
            _subject.Subscribe(typeof(TestEvent), typeof(TestAsyncHandler));
            TestAsyncHandler.ResetEvent = new ManualResetEvent(false);

            var e = new TestEvent();
            _subject.Publish(e);

            TestAsyncHandler.ResetEvent.WaitOne(500);
            TestAsyncHandler.LifetimeScopeTagWasSameAsPrevious.ShouldBe(true);
        }

        private static object _lastLifetimeScopeTag;

        public class TestEvent : IDomainEvent { }

        public class TestSyncHandler : IHandle<TestEvent>
        {
            public TestSyncHandler(ILifetimeScope lifetimeScope)
            {
                LifetimeScopeWasContainer = lifetimeScope.Tag == _container.Tag;
                LifetimeScopeTagWasSameAsPrevious = lifetimeScope.Tag == _lastLifetimeScopeTag;
                _lastLifetimeScopeTag = lifetimeScope.Tag;
            }

            public static bool LifetimeScopeWasContainer { get; set; }
            public static bool LifetimeScopeTagWasSameAsPrevious { get; set; }
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
                LifetimeScopeTagWasSameAsPrevious = lifetimeScope.Tag == _lastLifetimeScopeTag;
                _lastLifetimeScopeTag = lifetimeScope.Tag;
            }

            public static ManualResetEvent ResetEvent { get; set; }
            public static bool LifetimeScopeWasContainer { get; set; }
            public static bool LifetimeScopeTagWasSameAsPrevious { get; set; }
            public static bool HandleGotCalled { get; set; }

#pragma warning disable 1998
            public async Task HandleAsync(TestEvent args)
#pragma warning restore 1998
            {
                Thread.Sleep(50);
                HandleGotCalled = true;

                if (ResetEvent != null)
                    ResetEvent.Set();
            }
        }
    }
}