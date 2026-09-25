using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace PrisonGame.Combat.Tests
{
    [Category("Combat")]
    public sealed class FighterStateMachineTests
    {
        // His proposed punch: 0.7 s wind-up, then a short active window.
        private static readonly AttackTiming Punch = new AttackTiming(0.7f, 0.15f, 0.4f);

        private static List<string> Record(FighterStateMachine machine)
        {
            var log = new List<string>();
            machine.Changed += (from, to) => log.Add(from + ">" + to);
            return log;
        }

        [Test]
        public void StartsIdle()
        {
            var machine = new FighterStateMachine();
            Assert.AreEqual(FighterState.Idle, machine.State);
            Assert.IsFalse(machine.HitWindowOpen);
        }

        [Test]
        public void AttackRunsWindUpActiveRecoveryThenIdle()
        {
            var machine = new FighterStateMachine();
            Assert.IsTrue(machine.TryStartAttack(Punch));
            Assert.AreEqual(FighterState.WindUp, machine.State);
            machine.Tick(0.69f);
            Assert.AreEqual(FighterState.WindUp, machine.State, "Cannot hit before the wind-up ends.");
            Assert.IsFalse(machine.HitWindowOpen);
            machine.Tick(0.02f);
            Assert.AreEqual(FighterState.Active, machine.State);
            Assert.IsTrue(machine.HitWindowOpen);
            machine.Tick(0.15f);
            Assert.AreEqual(FighterState.Recovery, machine.State);
            Assert.IsFalse(machine.HitWindowOpen);
            machine.Tick(0.4f);
            Assert.AreEqual(FighterState.Idle, machine.State);
        }

        [Test]
        public void LongTickReportsEveryPhaseInOrder()
        {
            var machine = new FighterStateMachine();
            List<string> log = Record(machine);
            machine.TryStartAttack(Punch);
            machine.Tick(5f);
            CollectionAssert.AreEqual(new[] { "Idle>WindUp", "WindUp>Active", "Active>Recovery", "Recovery>Idle" }, log);
            Assert.AreEqual(FighterState.Idle, machine.State);
        }

        [Test]
        public void LeftoverTimeCarriesIntoTheNextPhase()
        {
            var machine = new FighterStateMachine();
            machine.TryStartAttack(Punch);
            machine.Tick(0.75f);
            Assert.AreEqual(FighterState.Active, machine.State);
            Assert.AreEqual(0.05f, machine.TimeInState, 1e-5f);
        }

        [Test]
        public void CannotStartAnotherAttackUntilIdle()
        {
            var machine = new FighterStateMachine();
            machine.TryStartAttack(Punch);
            Assert.IsFalse(machine.TryStartAttack(Punch));
            machine.Tick(0.8f);
            Assert.IsFalse(machine.TryStartAttack(Punch), "No attack during the active window.");
            machine.Tick(0.1f);
            Assert.AreEqual(FighterState.Recovery, machine.State);
            Assert.IsFalse(machine.TryStartAttack(Punch), "Recovery leaves the attacker open.");
            machine.Tick(1f);
            Assert.IsTrue(machine.TryStartAttack(Punch));
        }

        [Test]
        public void ZeroWindUpOpensTheHitWindowOnTheNextTick()
        {
            var machine = new FighterStateMachine();
            machine.TryStartAttack(new AttackTiming(0f, 0.1f, 0f));
            machine.Tick(0f);
            Assert.AreEqual(FighterState.Active, machine.State);
        }

        [Test]
        public void BlockHoldsUntilReleased()
        {
            var machine = new FighterStateMachine();
            Assert.IsTrue(machine.StartBlock());
            machine.Tick(30f);
            Assert.AreEqual(FighterState.Blocking, machine.State);
            Assert.IsFalse(machine.TryStartAttack(Punch));
            machine.EndBlock();
            Assert.AreEqual(FighterState.Idle, machine.State);
        }

        [Test]
        public void StaggerInterruptsAWindUpAndThenReturnsToIdle()
        {
            var machine = new FighterStateMachine();
            machine.TryStartAttack(Punch);
            machine.Tick(0.3f);
            Assert.IsTrue(machine.Stagger(0.5f));
            Assert.AreEqual(FighterState.Staggered, machine.State);
            machine.Tick(0.49f);
            Assert.AreEqual(FighterState.Staggered, machine.State);
            machine.Tick(0.02f);
            Assert.AreEqual(FighterState.Idle, machine.State);
        }

        [Test]
        public void DownedAndDeadFightersCannotActOrBeStaggered()
        {
            var machine = new FighterStateMachine();
            machine.GoDown();
            Assert.IsFalse(machine.TryStartAttack(Punch));
            Assert.IsFalse(machine.StartBlock());
            Assert.IsFalse(machine.Stagger(0.5f));
            machine.Tick(100f);
            Assert.AreEqual(FighterState.Downed, machine.State);
            machine.Die();
            Assert.AreEqual(FighterState.Dead, machine.State);
            Assert.IsFalse(machine.Recover(), "Dead stays dead.");
            Assert.AreEqual(FighterState.Dead, machine.State);
        }

        [Test]
        public void RecoverReturnsADownedFighterToIdle()
        {
            var machine = new FighterStateMachine();
            machine.GoDown();
            Assert.IsTrue(machine.Recover());
            Assert.AreEqual(FighterState.Idle, machine.State);
        }

        [Test]
        public void InvalidInputsAreRejected()
        {
            var machine = new FighterStateMachine();
            Assert.Throws<ArgumentException>(() => machine.TryStartAttack(new AttackTiming(0.5f, 0f, 0.2f)));
            Assert.Throws<ArgumentException>(() => machine.TryStartAttack(new AttackTiming(-0.1f, 0.1f, 0.2f)));
            Assert.Throws<ArgumentOutOfRangeException>(() => machine.Tick(-0.01f));
            Assert.Throws<ArgumentOutOfRangeException>(() => machine.Tick(float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => machine.Stagger(0f));
            Assert.AreEqual(FighterState.Idle, machine.State);
        }
    }
}
