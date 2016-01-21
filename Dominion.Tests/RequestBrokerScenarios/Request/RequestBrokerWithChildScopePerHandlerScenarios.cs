using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dominion.Messages;
using Shouldly;

namespace Dominion.Tests.RequestBrokerScenarios.Request
{
    [TestClass]
    public class RequestBrokerWithChildScopePerHandlerScenarios
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
            _subject = new MessageBroker(_container, MessagePublishingChildScopeBehaviour.ChildScopePerHandler);
        }

        [TestMethod]
        public async Task HandlerGetsCalled()
        {
            _subject.Subscribe(typeof(TestRequest), typeof(TestSyncHandler));

            var e = new TestRequest();
            var response = await _subject.Request(e);

            response.ShouldNotBe(null);
        }

        [TestMethod]
        public async Task FirstResponseIsReturned()
        {
            _lastLifetimeScopeTag = null;
            _subject.Subscribe(typeof(TestRequest), typeof(TestSyncHandler));
            _subject.Subscribe(typeof(TestRequest), typeof(TestAsyncHandler));
            TestAsyncHandler.ResetEvent = new ManualResetEvent(false);

            var e = new TestRequest();
            var response = await _subject.Request(e);

            TestAsyncHandler.ResetEvent.WaitOne(500);
            response.Id.ShouldBe("SyncHandler");
        }

        [TestMethod]
        public async Task SyncLifetimeScopeIsntOfTheContainer()
        {
            _subject.Subscribe(typeof(TestRequest), typeof(TestSyncHandler));

            var e = new TestRequest();
            var response = await _subject.Request(e);

            response.LifetimeScopeWasContainer.ShouldBe(false);
        }

        [TestMethod]
        public async Task AsyncLifetimeScopeIsntOfTheContainer()
        {
            _subject.Subscribe(typeof(TestRequest), typeof(TestAsyncHandler));
            TestAsyncHandler.ResetEvent = new ManualResetEvent(false);

            var e = new TestRequest();
            var response = await _subject.Request(e);

            TestAsyncHandler.ResetEvent.WaitOne(500);
            response.LifetimeScopeWasContainer.ShouldBe(false);
        }

        private static object _lastLifetimeScopeTag;

        public class TestRequest : DomainRequest<TestRequest, TestResponse>
        { }

        public class TestResponse : IDomainResponse
        {
            public TestResponse(string id, bool lifetimeScopeWasContainer, bool lifetimeScopeTagWasSameAsPrevious)
            {
                Id = id;
                LifetimeScopeWasContainer = lifetimeScopeWasContainer;
                LifetimeScopeTagWasSameAsPrevious = lifetimeScopeTagWasSameAsPrevious;
            }

            public string Id { get; private set; }
            public bool LifetimeScopeWasContainer { get; private set; }
            public bool LifetimeScopeTagWasSameAsPrevious { get; private set; }
        }

        public class TestSyncHandler : IHandleRequest<TestRequest, TestResponse>
        {
            private readonly bool _lifetimeScopeWasContainer;
            private readonly bool _lifetimeScopeTagWasSameAsPrevious;

            public TestSyncHandler(ILifetimeScope lifetimeScope)
            {
                _lifetimeScopeWasContainer = lifetimeScope.Tag == _container.Tag;
                _lifetimeScopeTagWasSameAsPrevious = lifetimeScope.Tag.Equals(_lastLifetimeScopeTag);
                _lastLifetimeScopeTag = lifetimeScope.Tag;
            }

            public TestResponse Handle(TestRequest args)
            {
                return new TestResponse("SyncHandler", _lifetimeScopeWasContainer, _lifetimeScopeTagWasSameAsPrevious);
            }
        }

        public class TestAsyncHandler : IHandleRequestAsync<TestRequest, TestResponse>
        {
            private readonly bool _lifetimeScopeWasContainer;
            private readonly bool _lifetimeScopeTagWasSameAsPrevious;

            public TestAsyncHandler(ILifetimeScope lifetimeScope)
            {
                _lifetimeScopeWasContainer = lifetimeScope.Tag == _container.Tag;
                _lifetimeScopeTagWasSameAsPrevious = lifetimeScope.Tag.Equals(_lastLifetimeScopeTag);
                _lastLifetimeScopeTag = lifetimeScope.Tag;
            }

            public static ManualResetEvent ResetEvent { get; set; }

#pragma warning disable 1998
            public async Task<TestResponse> HandleAsync(TestRequest args)
#pragma warning restore 1998
            {
                Thread.Sleep(50);

                if (ResetEvent != null)
                    ResetEvent.Set();

                return new TestResponse("AsyncHandler", _lifetimeScopeWasContainer, _lifetimeScopeTagWasSameAsPrevious);
            }
        }
    }
}