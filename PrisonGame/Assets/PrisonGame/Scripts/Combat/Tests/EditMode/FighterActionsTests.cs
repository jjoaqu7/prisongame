using System;
using NUnit.Framework;
using PrisonGame.Core;

namespace PrisonGame.Combat.Tests
{
    [Category("Combat")]
    public sealed class FighterActionsTests
    {
        private FighterStateMachine _state;
        private FighterActions _actions;
        private static readonly CombatIntent s_idle = new CombatIntent(true);
        private static readonly CombatIntent s_hold = new CombatIntent(true, attackHeld: true);
        private static readonly CombatIntent s_press = new CombatIntent(true, attackPressed: true, attackHeld: true);

        [SetUp]
        public void SetUp()
        {
            _state = new FighterStateMachine();
            _actions = new FighterActions(new StableId("player"), _state);
        }

        private void QuickPunch()
        {
            _actions.Tick(0f, s_press);
            _actions.Tick(0.03f, s_idle);
        }

        [Test]
        public void PressImmediatelyStartsPullbackButDoesNotHit()
        {
            _actions.Tick(0f, s_press);
            Assert.AreEqual(FighterState.Charging, _state.State);
            Assert.IsTrue(_actions.IsCharging);
            Assert.IsFalse(_actions.CanHit);
        }

        [Test]
        public void TapReleasesAQuickTenDamagePunch()
        {
            QuickPunch();
            Assert.AreEqual(FighterState.WindUp, _state.State);
            Assert.AreEqual(AttackKind.Quick, _actions.CurrentAttack.Kind);
            Assert.AreEqual(10f, _actions.CurrentAttack.Damage);
        }

        [Test]
        public void FullChargeCapsAtTwentyTwoDamageWithLongerRecovery()
        {
            _actions.Tick(0f, s_press);
            _actions.Tick(10f, s_hold);
            Assert.AreEqual(FighterState.Charging, _state.State, "Holding must not auto-fire.");
            _actions.Tick(0f, s_idle);
            Assert.AreEqual(AttackKind.Charged, _actions.CurrentAttack.Kind);
            Assert.AreEqual(22f, _actions.CurrentAttack.Damage);
            Assert.Greater(_actions.CurrentAttack.Timing.Recovery, FistAttack.Punch(0f).Timing.Recovery);
        }

        [Test]
        public void PartialChargeIncreasesDamage()
        {
            _actions.Tick(0f, s_press);
            _actions.Tick(0.4f, s_idle);
            Assert.That(_actions.CurrentAttack.Damage, Is.InRange(10.1f, 21.9f));
        }

        [Test]
        public void OneSwingCanHitEachTargetOnlyOnce()
        {
            QuickPunch();
            var target = new StableId("dummy");
            Assert.IsFalse(_actions.TryRegisterHit(target));
            _actions.Tick(0.07f, s_idle);
            Assert.IsTrue(_actions.TryRegisterHit(target));
            Assert.IsFalse(_actions.TryRegisterHit(target));
            Assert.IsTrue(_actions.TryRegisterHit(new StableId("other")));
            Assert.IsFalse(_actions.TryRegisterHit(_actions.Actor));
            Assert.IsFalse(_actions.TryRegisterHit(default));
        }

        [Test]
        public void NextSwingCanHitTheSameTargetAgain()
        {
            var target = new StableId("dummy");
            QuickPunch();
            _actions.Tick(0.07f, s_idle);
            Assert.IsTrue(_actions.TryRegisterHit(target));
            _actions.Tick(1f, s_idle);
            QuickPunch();
            _actions.Tick(0.07f, s_idle);
            Assert.IsTrue(_actions.TryRegisterHit(target));
        }

        [Test]
        public void ALongFrameStillReportsItsActiveWindowOnce()
        {
            int windows = 0;
            _actions.ActiveWindowOpened += () =>
            {
                Assert.IsTrue(_actions.CanHit);
                windows++;
            };
            QuickPunch();
            _actions.Tick(1f, s_idle);
            Assert.AreEqual(1, windows);
            Assert.AreEqual(FighterState.Idle, _state.State);
            Assert.IsFalse(_actions.CanHit);
        }

        [Test]
        public void LateRecoveryPressBuffersATapEvenIfReleasedBeforeRecoveryEnds()
        {
            QuickPunch();
            _actions.Tick(0.34f, s_idle);
            Assert.AreEqual(FighterState.Recovery, _state.State);
            _actions.Tick(0f, s_press);
            _actions.Tick(0.1f, s_idle);
            Assert.AreEqual(2, _actions.SwingNumber);
            Assert.AreEqual(FighterState.WindUp, _state.State);
        }

        [Test]
        public void ExpiredBufferDoesNotAttackLaterWhileHeld()
        {
            QuickPunch();
            _actions.Tick(0.19f, s_idle);
            _actions.Tick(0f, s_press);
            _actions.Tick(0.3f, s_hold);
            Assert.AreEqual(1, _actions.SwingNumber);
            Assert.AreEqual(FighterState.Idle, _state.State);
        }

        [Test]
        public void BlockCancelsChargingAndSuppressesAttackAndShove()
        {
            _actions.Tick(0f, s_press);
            _actions.Tick(0.1f, new CombatIntent(true, true, true, true, true));
            Assert.AreEqual(FighterState.Blocking, _state.State);
            Assert.AreEqual(0, _actions.SwingNumber);
            _actions.Tick(0.1f, s_idle);
            Assert.AreEqual(FighterState.Idle, _state.State);
        }

        [Test]
        public void HeldBlockStartsWhenAttackRecoveryFinishes()
        {
            QuickPunch();
            _actions.Tick(0.1f, new CombatIntent(true, blockHeld: true));
            Assert.AreEqual(FighterState.Active, _state.State);
            _actions.Tick(0.5f, new CombatIntent(true, blockHeld: true));
            Assert.AreEqual(FighterState.Blocking, _state.State);
        }

        [Test]
        public void ShoveHasRecoveryAndNoHealthDamage()
        {
            _actions.Tick(0f, new CombatIntent(true, shovePressed: true));
            Assert.AreEqual(AttackKind.Shove, _actions.CurrentAttack.Kind);
            Assert.AreEqual(0f, _actions.CurrentAttack.Damage);
            _actions.Tick(0.2f, new CombatIntent(true, shovePressed: true));
            Assert.AreEqual(FighterState.Recovery, _state.State);
            Assert.AreEqual(1, _actions.SwingNumber);
        }

        [Test]
        public void DisabledWorldActionsCancelChargeAndBuffer()
        {
            _actions.Tick(0f, s_press);
            _actions.Tick(0.1f, default);
            _actions.Tick(1f, s_idle);
            Assert.AreEqual(0, _actions.SwingNumber);
            Assert.AreEqual(FighterState.Idle, _state.State);
        }

        [Test]
        public void StaggerInterruptsChargeAndDoesNotFireOnRecovery()
        {
            _actions.Tick(0f, s_press);
            _state.Stagger(0.2f);
            _actions.Tick(0.3f, s_hold);
            _actions.Tick(0f, s_idle);
            Assert.AreEqual(FighterState.Idle, _state.State);
            Assert.AreEqual(0, _actions.SwingNumber);
        }

        [Test]
        public void DownedAndDeadActorsCannotAttack()
        {
            _state.GoDown();
            _actions.Tick(1f, new CombatIntent(true, true, true, false, true));
            Assert.AreEqual(FighterState.Downed, _state.State);
            _state.Die();
            _actions.Tick(1f, s_press);
            Assert.AreEqual(FighterState.Dead, _state.State);
            Assert.AreEqual(0, _actions.SwingNumber);
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void RejectsInvalidElapsedTime(float seconds)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _actions.Tick(seconds, s_idle));
        }
    }
}
