using System;
using NUnit.Framework;
using PrisonGame.Core;
using UnityEngine;

namespace PrisonGame.Combat.Tests
{
    [Category("Combat")]
    public sealed class HealthTests
    {
        private static readonly StableId s_player = new StableId("player-1");

        private static DamageInfo Hit(float amount) =>
            new DamageInfo(s_player, "fists", AttackKind.Punch, amount, Vector3.zero, Vector3.forward, 0d);

        [Test]
        public void StartsFullAndAlive()
        {
            var health = new Health(100f);
            Assert.AreEqual(100f, health.Current);
            Assert.AreEqual(LifeState.Alive, health.State);
        }

        [Test]
        public void DamageReducesHealthAndReportsAppliedAmount()
        {
            var health = new Health(100f);
            DamageResult result = health.Apply(Hit(10f));
            Assert.AreEqual(90f, health.Current);
            Assert.AreEqual(10f, result.Applied);
            Assert.IsFalse(result.WentDown);
            Assert.IsFalse(result.Ignored);
        }

        [Test]
        public void ZeroHealthLeavesFighterDownedNotDead()
        {
            var health = new Health(20f);
            int downs = 0;
            health.WentDown += _ => downs++;
            health.Apply(Hit(15f));
            DamageResult result = health.Apply(Hit(15f));
            Assert.AreEqual(0f, health.Current);
            Assert.AreEqual(5f, result.Applied, "Only the remaining health is applied.");
            Assert.IsTrue(result.WentDown);
            Assert.AreEqual(LifeState.Downed, health.State);
            Assert.AreEqual(1, downs);
        }

        [Test]
        public void AttacksDoNotHurtADownedFighter()
        {
            var health = new Health(10f);
            health.Apply(Hit(10f));
            int damagedEvents = 0, downs = 0;
            health.Damaged += (_, __) => damagedEvents++;
            health.WentDown += _ => downs++;
            DamageResult result = health.Apply(Hit(50f));
            Assert.IsTrue(result.Ignored);
            Assert.AreEqual(0f, result.Applied);
            Assert.AreEqual(LifeState.Downed, health.State);
            Assert.AreEqual(0, damagedEvents);
            Assert.AreEqual(0, downs, "Going down is reported exactly once.");
        }

        [Test]
        public void KillOnlyWorksOnADownedFighterAndOnlyOnce()
        {
            var health = new Health(10f);
            int kills = 0;
            StableId killer = default;
            health.Killed += id => { kills++; killer = id; };
            Assert.IsFalse(health.Kill(s_player), "A standing fighter cannot be killed by the kill action.");
            health.Apply(Hit(10f));
            Assert.IsTrue(health.Kill(s_player));
            Assert.IsFalse(health.Kill(s_player));
            Assert.AreEqual(LifeState.Dead, health.State);
            Assert.AreEqual(1, kills);
            Assert.AreEqual(s_player, killer);
        }

        [Test]
        public void RestoreFullRecoversAliveOrDownedButNotDead()
        {
            var health = new Health(30f);
            health.Apply(Hit(12f));
            Assert.IsTrue(health.RestoreFull());
            Assert.AreEqual(30f, health.Current);
            health.Apply(Hit(30f));
            Assert.IsTrue(health.RestoreFull());
            Assert.AreEqual(LifeState.Alive, health.State);
            health.Apply(Hit(30f));
            health.Kill(s_player);
            Assert.IsFalse(health.RestoreFull());
            Assert.AreEqual(LifeState.Dead, health.State);
            Assert.AreEqual(0f, health.Current);
        }

        [Test]
        public void ZeroDamageIsAllowedAndChangesNothing()
        {
            var health = new Health(10f);
            DamageResult result = health.Apply(Hit(0f));
            Assert.AreEqual(10f, health.Current);
            Assert.AreEqual(0f, result.Applied);
            Assert.IsFalse(result.Ignored);
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void InvalidDamageIsRejected(float amount)
        {
            var health = new Health(10f);
            Assert.Throws<ArgumentOutOfRangeException>(() => health.Apply(Hit(amount)));
            Assert.AreEqual(10f, health.Current);
        }

        [TestCase(0f)]
        [TestCase(-5f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void InvalidMaximumIsRejected(float max)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Health(max));
        }
    }
}
