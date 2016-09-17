using System;
using System.Linq;
using Dominion.EventSourcing.Repositories;
using Dominion.Tests.EventSourcing.EventStoreScenarios.SampleDomain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Dominion.Tests.EventSourcing.EventStoreScenarios
{
    [TestClass]
    public class StoreScenarios
    {
        [TestMethod]
        public void EventsCanBeAddedAndRetrievedAsCorrectType()
        {
            var store = new EventStore();
            var aggregate = new SampleAggregate();
            var events = aggregate.ChangeName("something new");

            var aggregate2 = new SampleAggregate();

            store.Store(events);
            store.Store(aggregate2.ChangeName("something else"));

            var newAggregate = store.Get<SampleAggregate, Guid>(aggregate2.Id);
            newAggregate.First().GetType().ShouldBe(typeof(SampleAggregateChangedNameEvent));
        }

        [TestMethod]
        public void EventsCanBeRetrievedAndApplied()
        {
            var store = new EventStore();
            var aggregate = new SampleAggregate();
            store.Store(aggregate.ChangeName("something new"));

            var aggregate2 = new SampleAggregate();
            store.Store(aggregate2.ChangeName("something else"));

            var storedEvents = store.Get<SampleAggregate, Guid>(aggregate2.Id);

            var aggregate3 = new SampleAggregate(aggregate2.Id);
            var d = (dynamic)aggregate3;
            foreach (var @event in storedEvents)
            {
                d.Handle((dynamic)@event);
            }

            aggregate3.Name.ShouldBe("something else");
        }
    }
}