using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dominion.Messages;
using Shouldly;

namespace Dominion.Tests.RequestBrokerScenarios.Request
{
    [TestClass]
    public class RequestBrokerWithoutChildScopesScenarios
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
            _subject.Subscribe(typeof(TestRequest), typeof(TestSyncHandler));

            var e = new TestRequest();
            var response = await _subject.RequestAsync(e);

            response.ShouldNotBe(null);
        }

        [TestMethod]
        public async Task SyncLifetimeScopeIsTheContainer()
        {
            _subject.Subscribe(typeof(TestRequest), typeof(TestSyncHandler));

            var e = new TestRequest();
            var response = await _subject.RequestAsync(e);

            response.LifetimeScopeWasContainer.ShouldBe(true);
        }

        [TestMethod]
        public async Task AsyncLifetimeScopeIsTheContainer()
        {
            _subject.Subscribe(typeof(TestRequest), typeof(TestAsyncHandler));
            TestAsyncHandler.ResetEvent = new ManualResetEvent(false);

            var e = new TestRequest();
            var response = await _subject.RequestAsync(e);

            TestAsyncHandler.ResetEvent.WaitOne(500);
            response.LifetimeScopeWasContainer.ShouldBe(true);
        }

        [TestMethod]
        public async Task AsyncHandlerExecutesBeforeReturn()
        {
            _subject.Subscribe(typeof(TestRequest), typeof(TestAsyncHandler));

            var e = new TestRequest();
            var response = await _subject.RequestAsync(e);

            response.ShouldNotBe(null);
        }

        public class TestRequest : DomainRequest<TestRequest, TestResponse>
        { }

        public class TestResponse : IDomainResponse
        {
            public TestResponse(string id, bool lifetimeScopeWasContainer)
            {
                Id = id;
                LifetimeScopeWasContainer = lifetimeScopeWasContainer;
            }

            public string Id { get; private set; }
            public bool LifetimeScopeWasContainer { get; private set; }
        }

        public class TestSyncHandler : IHandleRequest<TestRequest, TestResponse>
        {
            private readonly bool _lifetimeScopeWasContainer;

            public TestSyncHandler(ILifetimeScope lifetimeScope)
            {
                _lifetimeScopeWasContainer = lifetimeScope.Tag == _container.Tag;
            }

            public TestResponse Handle(TestRequest args)
            {
                return new TestResponse("SyncHandler", _lifetimeScopeWasContainer);
            }
        }

        public class TestAsyncHandler : IHandleRequestAsync<TestRequest, TestResponse>
        {
            private bool _lifetimeScopeWasContainer;

            public TestAsyncHandler(ILifetimeScope lifetimeScope)
            {
                _lifetimeScopeWasContainer = lifetimeScope.Tag == _container.Tag;
            }

            public static ManualResetEvent ResetEvent { get; set; }

#pragma warning disable 1998
            public async Task<TestResponse> HandleAsync(TestRequest args)
#pragma warning restore 1998
            {
                Console.WriteLine("Sleeping...");
                Thread.Sleep(50);
                Console.WriteLine("Awake again...");

                ResetEvent?.Set();
                Console.WriteLine("Event set...");

                return new TestResponse("AsyncHandler", _lifetimeScopeWasContainer);
            }
        }
    }
}
