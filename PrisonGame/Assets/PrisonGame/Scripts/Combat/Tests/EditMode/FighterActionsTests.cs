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
        private static readonly CombatIntent s_punch = new CombatIntent(true, punchPressed: true);
        private static readonly CombatIntent s_kick = new CombatIntent(true, kickPressed: true);

        [SetUp]
        public void SetUp()
        {
            _state = new FighterStateMachine();
            _actions = new FighterActions(new StableId("player"), _state);
        }

        private void Punch()
        {
            _actions.Tick(0f, s_punch);
        }

        [Test]
        public void PunchPressStartsTheWindUpAtOnceButDoesNotHitYet()
        {
            Punch();
            Assert.AreEqual(FighterState.WindUp, _state.State);
            Assert.AreEqual(AttackKind.Punch, _actions.CurrentAttack.Kind);
            Assert.IsFalse(_actions.CanHit);
        }

        [Test]
        public void PunchDealsTenDamage()
        {
            Punch();
            Assert.AreEqual(10f, _actions.CurrentAttack.Damage);
            Assert.AreEqual(1.1f, _actions.CurrentAttack.Reach, 1e-6f);
        }

        [Test]
        public void KickDoesSlightlyMoreDamageReachesFurtherAndIsSlower()
        {
            UnarmedAttack punch = UnarmedAttack.Punch;
            _actions.Tick(0f, s_kick);
            UnarmedAttack kick = _actions.CurrentAttack;
            Assert.AreEqual(AttackKind.Kick, kick.Kind);
            Assert.AreEqual(FighterState.WindUp, _state.State);
            Assert.AreEqual(12f, kick.Damage);
            Assert.Greater(kick.Damage, punch.Damage);
            Assert.Greater(kick.Reach, punch.Reach);
            Assert.Greater(kick.Timing.WindUp, punch.Timing.WindUp, "The kick can be seen coming.");
            Assert.Greater(kick.Timing.Recovery, punch.Timing.Recovery, "A missed kick leaves you open longer.");
            Assert.Greater(kick.Knockback, punch.Knockback);
        }

        [Test]
        public void KickWinsWhenBothArePressedInTheSameFrame()
        {
            _actions.Tick(0f, new CombatIntent(true, punchPressed: true, kickPressed: true));
            Assert.AreEqual(AttackKind.Kick, _actions.CurrentAttack.Kind);
            Assert.AreEqual(1, _actions.SwingNumber);
        }

        [Test]
        public void OneSwingCanHitEachTargetOnlyOnce()
        {
            Punch();
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
            Punch();
            _actions.Tick(0.07f, s_idle);
            Assert.IsTrue(_actions.TryRegisterHit(target));
            _actions.Tick(1f, s_idle);
            _actions.Tick(0f, s_kick);
            _actions.Tick(0.31f, s_idle);
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
            Punch();
            _actions.Tick(1f, s_idle);
            Assert.AreEqual(1, windows);
            Assert.AreEqual(FighterState.Idle, _state.State);
            Assert.IsFalse(_actions.CanHit);
        }

        [Test]
        public void LateRecoveryPressIsBufferedIntoTheNextPunch()
        {
            Punch();
            _actions.Tick(0.34f, s_idle);
            Assert.AreEqual(FighterState.Recovery, _state.State);
            _actions.Tick(0f, s_punch);
            _actions.Tick(0.1f, s_idle);
            Assert.AreEqual(2, _actions.SwingNumber);
            Assert.AreEqual(FighterState.WindUp, _state.State);
            Assert.AreEqual(AttackKind.Punch, _actions.CurrentAttack.Kind);
        }

        [Test]
        public void KickPressedDuringRecoveryIsBufferedAsAKick()
        {
            Punch();
            _actions.Tick(0.34f, s_idle);
            _actions.Tick(0f, s_kick);
            _actions.Tick(0.1f, s_idle);
            Assert.AreEqual(2, _actions.SwingNumber);
            Assert.AreEqual(AttackKind.Kick, _actions.CurrentAttack.Kind);
        }

        [Test]
        public void ExpiredBufferDoesNotAttackLater()
        {
            Punch();
            _actions.Tick(0.19f, s_idle);
            _actions.Tick(0f, s_punch);
            _actions.Tick(0.3f, s_idle);
            Assert.AreEqual(1, _actions.SwingNumber);
            Assert.AreEqual(FighterState.Idle, _state.State);
        }

        [Test]
        public void BlockSuppressesPunchKickAndShove()
        {
            _actions.Tick(0.1f, new CombatIntent(true, true, true, true, true));
            Assert.AreEqual(FighterState.Blocking, _state.State);
            Assert.AreEqual(0, _actions.SwingNumber);
            _actions.Tick(0.1f, s_idle);
            Assert.AreEqual(FighterState.Idle, _state.State);
        }

        [Test]
        public void HeldBlockStartsWhenAttackRecoveryFinishes()
        {
            Punch();
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
        public void DisabledWorldActionsCancelTheSwingAndBuffer()
        {
            int windows = 0;
            _actions.ActiveWindowOpened += () => windows++;
            _actions.Tick(0f, s_kick);
            _actions.Tick(0.1f, default);
            Assert.AreEqual(FighterState.Idle, _state.State);
            _actions.Tick(1f, s_idle);
            Assert.AreEqual(0, windows, "A cancelled kick never reaches its hit window.");
            Assert.AreEqual(FighterState.Idle, _state.State);
        }

        [Test]
        public void StaggerInterruptsAKickWindUp()
        {
            int windows = 0;
            _actions.ActiveWindowOpened += () => windows++;
            _actions.Tick(0f, s_kick);
            _state.Stagger(0.2f);
            _actions.Tick(0.3f, s_idle);
            _actions.Tick(0f, s_idle);
            Assert.AreEqual(FighterState.Idle, _state.State);
            Assert.AreEqual(0, windows);
        }

        [Test]
        public void DownedAndDeadActorsCannotAttack()
        {
            _state.GoDown();
            _actions.Tick(1f, new CombatIntent(true, true, true, false, true));
            Assert.AreEqual(FighterState.Downed, _state.State);
            _state.Die();
            _actions.Tick(1f, s_punch);
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
