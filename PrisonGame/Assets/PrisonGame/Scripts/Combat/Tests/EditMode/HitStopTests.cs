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
        public void ChargedPunchesFreezeLongerAndPushFurtherThanQuickOnes()
        {
            FistAttack quick = FistAttack.Punch(0.05f);
            FistAttack full = FistAttack.Punch(0.8f);
            Assert.AreEqual(AttackKind.Quick, quick.Kind);
            Assert.AreEqual(0.06f, quick.HitStopSeconds, 1e-6f);
            Assert.AreEqual(0.08f, quick.Knockback, 1e-6f);
            Assert.AreEqual(AttackKind.Charged, full.Kind);
            Assert.AreEqual(1f, full.Charge, 1e-6f);
            Assert.AreEqual(0.1f, full.HitStopSeconds, 1e-6f);
            Assert.AreEqual(0.3f, full.Knockback, 1e-6f);
        }

        [Test]
        public void ShoveMovesAboutAMetreWithoutDamage()
        {
            FistAttack shove = FistAttack.Shove;
            Assert.AreEqual(0f, shove.Damage);
            Assert.AreEqual(FistAttack.ShoveDistance, shove.Knockback);
            Assert.That(shove.HitStopSeconds, Is.GreaterThan(0f).And.LessThan(FistAttack.Punch(0f).HitStopSeconds));
        }
    }
}
