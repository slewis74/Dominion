using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Dominion.Messages;

namespace Dominion.Tests
{
    [TestClass]
    public class MessageAggregatorScenarios
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void EnsureAnExceptionIsRaisedIfTheEventBrokerIsntSet()
        {
            MessageAggregator.Publish(new TestEvent());
        }

        [TestMethod]
        public void EnsureThatAMockedEventBrokerRegistersAPublishCorrectly()
        {
            var broker = Substitute.For<IMessageBroker>();
            MessageAggregator.SetBroker(broker);

            var testEvent = new TestEvent();
            MessageAggregator.Publish(testEvent);

            broker.Received(1).Publish(Arg.Is(testEvent));
        }

        private class TestEvent : IDomainEvent { }
    }
}