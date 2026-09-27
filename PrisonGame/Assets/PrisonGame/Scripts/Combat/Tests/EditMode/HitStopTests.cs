using System;
using NUnit.Framework;

namespace PrisonGame.Combat.Tests
{
    [Category("Combat")]
    public sealed class HitStopTests
    {
        [Test]
        public void FreezeConsumesTimeUntilItRunsOut()
        {
            var hitStop = new HitStop();
            hitStop.Begin(0.06f);
            Assert.IsTrue(hitStop.IsActive);
            Assert.AreEqual(0f, hitStop.Consume(0.04f));
            Assert.AreEqual(0.02f, hitStop.Remaining, 1e-6f);
            Assert.AreEqual(0.03f, hitStop.Consume(0.05f), 1e-6f, "Time beyond the freeze passes to the timeline.");
            Assert.IsFalse(hitStop.IsActive);
            Assert.AreEqual(0.1f, hitStop.Consume(0.1f), 1e-6f);
        }

        [Test]
        public void OverlappingHitsKeepTheLongestFreezeNotTheSum()
        {
            var hitStop = new HitStop();
            hitStop.Begin(0.06f);
            hitStop.Begin(0.1f);
            hitStop.Begin(0.05f);
            Assert.AreEqual(0.1f, hitStop.Remaining, 1e-6f);
        }

        [Test]
        public void InvalidTimesAreRejected()
        {
            var hitStop = new HitStop();
            Assert.Throws<ArgumentOutOfRangeException>(() => hitStop.Begin(-0.01f));
            Assert.Throws<ArgumentOutOfRangeException>(() => hitStop.Begin(float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => hitStop.Consume(-0.01f));
            Assert.Throws<ArgumentOutOfRangeException>(() => hitStop.Consume(float.PositiveInfinity));
            Assert.IsFalse(hitStop.IsActive);
        }

        [Test]
        public void KicksFreezeLongerAndPushFurtherThanPunches()
        {
            UnarmedAttack punch = UnarmedAttack.Punch;
            UnarmedAttack kick = UnarmedAttack.Kick;
            Assert.AreEqual(0.06f, punch.HitStopSeconds, 1e-6f);
            Assert.AreEqual(0.08f, punch.Knockback, 1e-6f);
            Assert.AreEqual(0.09f, kick.HitStopSeconds, 1e-6f);
            Assert.AreEqual(0.6f, kick.Knockback, 1e-6f);
        }

        [Test]
        public void ShoveMovesAboutAMetreWithoutDamage()
        {
            UnarmedAttack shove = UnarmedAttack.Shove;
            Assert.AreEqual(0f, shove.Damage);
            Assert.AreEqual(UnarmedAttack.ShoveDistance, shove.Knockback);
            Assert.That(shove.HitStopSeconds, Is.GreaterThan(0f).And.LessThan(UnarmedAttack.Punch.HitStopSeconds));
        }
    }
}
