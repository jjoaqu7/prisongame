using System;
using NUnit.Framework;

namespace PrisonGame.Combat.Tests
{
    [Category("Combat")]
    public sealed class StaggerMeterTests
    {
        [Test]
        public void OnePunchDoesNotFillTheMeterButFourInARowDo()
        {
            var meter = new StaggerMeter();
            float punch = UnarmedAttack.Punch.StaggerPower;
            Assert.IsFalse(meter.Add(punch));
            Assert.IsFalse(meter.Add(punch));
            Assert.IsFalse(meter.Add(punch));
            Assert.IsTrue(meter.Add(punch));
            Assert.AreEqual(0f, meter.Value, "A full meter empties for the next build-up.");
        }

        [Test]
        public void AKickFillsTheMeterAtOnce()
        {
            var meter = new StaggerMeter();
            Assert.IsTrue(meter.Add(UnarmedAttack.Kick.StaggerPower));
        }

        [Test]
        public void MeterHoldsForADelayThenDrains()
        {
            var meter = new StaggerMeter();
            meter.Add(50f);
            meter.Tick(StaggerMeter.DecayDelaySeconds);
            Assert.AreEqual(50f, meter.Value, 1e-4f, "No drain during the delay.");
            meter.Tick(1f);
            Assert.AreEqual(50f - StaggerMeter.DecayPerSecond, meter.Value, 1e-4f);
            meter.Tick(10f);
            Assert.AreEqual(0f, meter.Value);
        }

        [Test]
        public void OnlyTheTimeAfterTheDelayDrainsInOneLongTick()
        {
            var meter = new StaggerMeter();
            meter.Add(50f);
            meter.Tick(StaggerMeter.DecayDelaySeconds + 0.5f);
            Assert.AreEqual(50f - StaggerMeter.DecayPerSecond * 0.5f, meter.Value, 1e-4f);
        }

        [Test]
        public void ANewHitRestartsTheDelay()
        {
            var meter = new StaggerMeter();
            meter.Add(25f);
            meter.Tick(0.9f);
            meter.Add(25f);
            meter.Tick(0.9f);
            Assert.AreEqual(50f, meter.Value, 1e-4f);
        }

        [Test]
        public void ZeroBuildUpChangesNothingAndNegativeIsRejected()
        {
            var meter = new StaggerMeter();
            Assert.IsFalse(meter.Add(0f));
            Assert.AreEqual(0f, meter.Value);
            Assert.Throws<ArgumentOutOfRangeException>(() => meter.Add(-1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => meter.Tick(-1f));
        }
    }
}
