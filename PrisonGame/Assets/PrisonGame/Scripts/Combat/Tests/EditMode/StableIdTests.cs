using System;
using NUnit.Framework;
using PrisonGame.Core;

namespace PrisonGame.Combat.Tests
{
    [Category("Combat")]
    public sealed class StableIdTests
    {
        [Test]
        public void NewIdsAreValidAndUnique()
        {
            StableId a = StableId.New(), b = StableId.New();
            Assert.IsTrue(a.IsValid);
            Assert.AreNotEqual(a, b);
        }

        [Test]
        public void EqualityIsByValue()
        {
            Assert.AreEqual(new StableId("neighbor"), new StableId("neighbor"));
            Assert.IsTrue(new StableId("neighbor") == new StableId("neighbor"));
            Assert.AreEqual(new StableId("neighbor").GetHashCode(), new StableId("neighbor").GetHashCode());
        }

        [Test]
        public void DefaultIsInvalidAndEmptyIsRejected()
        {
            Assert.IsFalse(default(StableId).IsValid);
            Assert.Throws<ArgumentException>(() => new StableId(" "));
        }
    }
}
