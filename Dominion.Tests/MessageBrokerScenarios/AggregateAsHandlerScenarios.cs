using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Dominion.Messages;
using Dominion.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Dominion.Tests.MessageBrokerScenarios
{
    [TestClass]
    public class AggregateAsHandlerScenarios
    {
        private static IContainer _container;
        private MessageBroker _subject;

        [TestInitialize]
        public void SetUp()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<TestRepository>().AsImplementedInterfaces().AsSelf().InstancePerLifetimeScope();

            TestRepository.HandleGotCalled = false;

            _container = builder.Build();
            _subject = new MessageBroker(_container, MessagePublishingChildScopeBehaviour.NoChildScopes);
            _subject.Subscribe(typeof(TestChangedEvent), typeof(TestAggregate));
            _subject.Subscribe(typeof(TestCreatedEvent), typeof(TestRepository));
        }

        [TestMethod]
        public async Task RepositoryHandlerGetsCalled()
        {
            var e = new TestCreatedEvent(new TestAggregate());
            await _subject.PublishAsync(e);

            TestRepository.HandleGotCalled.ShouldBe(true);
        }

        [TestMethod]
        public async Task AggregateHandlerGetsCalled()
        {
            var repo = _container.Resolve<IRepository<TestAggregate, Guid>>();
            var testAggregate = new TestAggregate();
            repo.Add(testAggregate);
            
            var e = new TestChangedEvent(testAggregate);
            await _subject.PublishAsync(e);

            TestRepository.HandleGotCalled.ShouldBe(false);
            testAggregate.HandleGotCalled.ShouldBe(true);
        }

        [TestMethod]
        public async Task CorrectAggregateHasItsHandlerCalled()
        {
            var repo = _container.Resolve<IRepository<TestAggregate, Guid>>();
            var testAggregate = new TestAggregate();
            repo.Add(testAggregate);
            var testAggregate2 = new TestAggregate();
            repo.Add(testAggregate2);
            
            var e = new TestChangedEvent(testAggregate2);
            await _subject.PublishAsync(e);

            TestRepository.HandleGotCalled.ShouldBe(false);
            testAggregate.HandleGotCalled.ShouldBe(false);
            testAggregate2.HandleGotCalled.ShouldBe(true);
        }

        public class TestCreatedEvent : AggregateCreatedEvent<TestAggregate, Guid>
        {
            public TestCreatedEvent(TestAggregate aggregate) : base(aggregate)
            {
            }
        }

        public class BaseChangedEvent<TAggregate> : AggregateChangedEvent<TAggregate, Guid>
            where TAggregate : IAggregate<Guid>
        {
            public BaseChangedEvent()
            {}

            public BaseChangedEvent(TAggregate aggregate) : base(aggregate)
            {}
        }

        public class TestChangedEvent : BaseChangedEvent<TestAggregate>
        {
            public TestChangedEvent(TestAggregate aggregate) : base(aggregate)
            {
            }
        }

        public class TestAggregate : IAggregate<Guid>, IHandle<TestChangedEvent>
        {
            public TestAggregate()
            {
                Id = Guid.NewGuid();
            }

            public TestAggregate(Guid id)
            {
                Id = id;
            }

            public Guid Id { get; }

            public bool HandleGotCalled { get; set; }

            public void Handle(TestChangedEvent args)
            {
                HandleGotCalled = true;
            }
        }

        public class TestRepository : IRepository<TestAggregate, Guid>, 
            IHandle<AggregateCreatedEvent<TestAggregate, Guid>>
        {
            private readonly Dictionary<Guid, TestAggregate> _aggregates;

            public TestRepository()
            {
                _aggregates = new Dictionary<Guid, TestAggregate>();
            }

            public TestAggregate Get(Guid id)
            {
                return _aggregates.ContainsKey(id) ? _aggregates[id] : null;
            }

            public IQueryable<TestAggregate> All()
            {
                return _aggregates.Values.AsQueryable();
            }

            public void Add(TestAggregate entity)
            {
                if (_aggregates.ContainsKey(entity.Id)) return;
                _aggregates.Add(entity.Id, entity);
            }

            public void Remove(TestAggregate entity)
            {
                if (!_aggregates.ContainsKey(entity.Id)) return;
                _aggregates.Remove(entity.Id);
            }

            public static bool HandleGotCalled { get; set; }

            public void Handle(AggregateCreatedEvent<TestAggregate, Guid> eventInstance)
            {
                Add(new TestAggregate());
                HandleGotCalled = true;
            }
        }
    }
}