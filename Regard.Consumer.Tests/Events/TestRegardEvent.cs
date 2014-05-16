using NUnit.Framework;
using Regard.Consumer.Logic;
using Regard.Consumer.Logic.Api;

namespace Regard.Consumer.Tests.Events
{
    [TestFixture]
    class TestRegardEvent
    {
        [Test]
        public static void CanCreateAnEmptyEvent()
        {
            // Can create an event
            var evt = RegardEvent.Create("Test");
            Assert.IsNotNull(evt);
        }

        [Test]
        public static void RawDataMatchesCreationValue()
        {
            // Can retrieve the raw data used when creating the object
            var evt = RegardEvent.Create("Test");
            Assert.AreEqual("Test", evt.RawData());
        }

        [Test]
        public static void CanSetAndRetrieveDataWithAnArbitraryKey()
        {
            // Can write to any key
            var evt = RegardEvent.Create("Test");
            evt = evt.With("Test.Arbitrary", "Arbitrary");
            Assert.IsNotNull(evt);
            Assert.AreEqual("Arbitrary", evt["Test.Arbitrary"]);
        }

        [Test]
        public static void DefaultValueForDataIsNull()
        {
            var evt = RegardEvent.Create("Test");
            Assert.AreEqual(null, evt["Test.Arbitrary"]);
        }

        [Test]
        public static void ArbitraryDataIsStoredInDistinctFields()
        {
            // Ensure that field data actually is stored and is retrievable from arbitrary keys
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
        public static void RawDataIsImmutable()
        {
            // Calling 'With' should create a new object with the new values and not update the old one
            var evt = RegardEvent.Create("Test");
            var evt2 = evt.WithRawData("Different");

            Assert.IsNotNull(evt2);
            Assert.AreEqual("Test", evt.RawData());
            Assert.AreEqual("Different", evt2.RawData());
        }

        [Test]
        public static void ArbitraryDataIsImmutable()
        {
            // Calling 'With' should create a new object with the new values and not update the old one
            var evt = RegardEvent.Create("Raw").With("TestField", "Test");
            var evt2 = evt.With("TestField", "Different");

            Assert.IsNotNull(evt2);
            Assert.AreEqual("Test", evt["TestField"]);
            Assert.AreEqual("Different", evt2["TestField"]);
        }

        [Test]
        public static void NonexistentFieldsAreAlsoImmutable()
        {
            // Fields should remain null after a 'with'
            var evt = RegardEvent.Create("Test");
            var evt2 = evt.With("Test.Arbitrary", "Arbitrary");

            Assert.IsNotNull(evt2);
            Assert.IsNull(evt["Test.Arbitrary"]);
            Assert.AreEqual("Arbitrary", evt2["Test.Arbitrary"]);
        }
    }
}
