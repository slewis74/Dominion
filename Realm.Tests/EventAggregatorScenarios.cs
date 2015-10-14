using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Realm.Events;

namespace Realm.Tests
{
    [TestClass]
    public class EventAggregatorScenarios
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void EnsureAnExceptionIsRaisedIfTheEventBrokerIsntSet()
        {
            EventAggregator.Publish(new TestEvent());
        }

        [TestMethod]
        public void EnsureThatAMockedEventBrokerRegistersAPublishCorrectly()
        {
            var broker = Substitute.For<IEventBroker>();
            EventAggregator.SetBroker(broker);

            var testEvent = new TestEvent();
            EventAggregator.Publish(testEvent);

            broker.Received(1).Publish(Arg.Is(testEvent));
        }

        private class TestEvent : IDomainEvent { }
    }
}