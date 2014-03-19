using NUnit.Framework;
using Regard.Consumer.Logic;
using Regard.Consumer.Logic.Api;

namespace Regard.Consumer.Tests.Events
{
    [TestFixture]
    class TestRegardEvent
    {
        [Test]
        public static void CanCreate()
        {
            var evt = RegardEvent.Create("Test");
            Assert.IsNotNull(evt);
        }

        [Test]
        public static void RetrieveRawData()
        {
            var evt = RegardEvent.Create("Test");
            Assert.AreEqual("Test", evt.RawData());
        }

        [Test]
        public static void RetrieveArbitraryData()
        {
            var evt = RegardEvent.Create("Test");
            evt = evt.With("Test.Arbitrary", "Arbitrary");
            Assert.IsNotNull(evt);
            Assert.AreEqual("Arbitrary", evt["Test.Arbitrary"]);
        }

        [Test]
        public static void DistinctFields()
        {
            var evt = RegardEvent.Create("Test");
            evt = evt.WithPayload("TestPayload");
            evt = evt.WithOrganization("TestOrg");
            evt = evt.WithProduct("TestProduct");
            evt = evt.WithError("TestError");

            Assert.AreEqual("Test", evt.RawData());
            Assert.AreEqual("TestPayload", evt.Payload());
            Assert.AreEqual("TestOrg", evt.Organization());
            Assert.AreEqual("TestProduct", evt.Product());
            Assert.AreEqual("TestError", evt.Error());
        }

        [Test]
        public static void Immutable()
        {
            var evt = RegardEvent.Create("Test");
            var evt2 = evt.WithRawData("Different");

            Assert.IsNotNull(evt2);
            Assert.AreEqual("Test", evt.RawData());
            Assert.AreEqual("Different", evt2.RawData());
        }
    }
}
