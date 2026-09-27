using System.Collections.Generic;
using NUnit.Framework;
using PrisonGame.Combat;
using PrisonGame.Core;
using UnityEngine;

namespace PrisonGame.AI.Tests
{
    [Category("AI")]
    public sealed class InmateBrainTests
    {
        private const float Step = 0.05f;
        private static readonly InmatePerception s_notFighting = new InmatePerception(false, 1f, true);
        private static readonly InmatePerception s_far = new InmatePerception(true, 3f, true);
        private static readonly InmatePerception s_close = new InmatePerception(true, 1.2f, true);
        private static readonly InmatePerception s_unavailable = new InmatePerception(true, 1.2f, false);
        private FighterStateMachine _state;
        private Health _health;
        private FighterDefense _defense;
        private FighterActions _actions;
        private InmateBrain _brain;
        private InmateDecision _last;

        [SetUp]
        public void SetUp()
        {
            _brain = MakeBrain(7, out _state, out _health, out _defense, out _actions);
        }

        private static InmateBrain MakeBrain(int seed, out FighterStateMachine state, out Health health,
            out FighterDefense defense, out FighterActions actions)
        {
            state = new FighterStateMachine();
            health = new Health(100f);
            defense = new FighterDefense(health, state, canTimedBlock: false);
            FighterStateMachine wired = state;
            health.WentDown += _ => wired.GoDown();
            health.Killed += _ => wired.Die();
            actions = new FighterActions(new StableId("inmate"), state) { Moveset = UnarmedMoveset.Inmate };
            return new InmateBrain(state, defense, health, seed);
        }

        // One frame as the scene runs it: the brain decides, then his fighter acts on the intent.
        private InmateDecision Run(float seconds, InmatePerception perception)
        {
            for (float t = 0f; t < seconds - 1e-4f; t += Step)
            {
                _last = _brain.Tick(Step, perception);
                _actions.Tick(Step, _last.Intent);
                _defense.Tick(Step);
            }
            return _last;
        }

        private static DamageInfo PlayerHit(float amount, float staggerPower = 0f) =>
            new DamageInfo(new StableId("player"), "fists", AttackKind.Punch, amount, Vector3.zero, Vector3.forward, 0d,
                staggerPower: staggerPower);

        [Test]
        public void StaysIdleUntilTheFightStarts()
        {
            InmateDecision decision = Run(5f, s_notFighting);
            Assert.AreEqual(InmateState.Idle, _brain.State);
            Assert.IsFalse(decision.Intent.Enabled);
            Assert.AreEqual(0f, decision.MoveSpeed);
            Assert.AreEqual(0, _brain.AttacksStarted);
        }

        [Test]
        public void ApproachesAPlayerOutOfReach()
        {
            InmateDecision decision = Run(Step, s_far);
            Assert.AreEqual(InmateState.Approach, _brain.State);
            Assert.AreEqual(InmateBrain.ApproachSpeed, decision.MoveSpeed);
            Assert.Greater(decision.TurnDegreesPerSecond, 0f);
        }

        [Test]
        public void FirstPunchComesShortlyAfterTheFightStarts()
        {
            Run(InmateBrain.MinFirstAttackDelay - Step, s_close);
            Assert.AreEqual(0, _brain.AttacksStarted, "A short pause before he retaliates.");
            Run(InmateBrain.MaxFirstAttackDelay - InmateBrain.MinFirstAttackDelay + 2f * Step, s_close);
            Assert.AreEqual(1, _brain.AttacksStarted);
            Assert.AreEqual(FighterState.WindUp, _state.State);
        }

        [Test]
        public void PunchGoesThroughWindUpAttackAndRecover()
        {
            var seen = new List<InmateState>();
            _brain.Changed += (from, to) => seen.Add(to);
            Run(3f, s_close);
            CollectionAssert.IsSubsetOf(
                new[] { InmateState.Ready, InmateState.WindUp, InmateState.Attack, InmateState.Recover }, seen);
            Assert.Less(seen.IndexOf(InmateState.WindUp), seen.IndexOf(InmateState.Attack));
            Assert.Less(seen.IndexOf(InmateState.Attack), seen.IndexOf(InmateState.Recover));
        }

        [Test]
        public void WaitsTwoToThreeSecondsBetweenAttacks()
        {
            var starts = new List<float>();
            float time = 0f;
            _state.Changed += (from, to) =>
            {
                if (to == FighterState.WindUp)
                {
                    starts.Add(time);
                }
            };
            for (int i = 0; i < 600; i++)
            {
                time += Step;
                Run(Step, s_close);
            }
            Assert.GreaterOrEqual(starts.Count, 8);
            for (int i = 1; i < starts.Count; i++)
            {
                float interval = starts[i] - starts[i - 1];
                Assert.That(interval, Is.InRange(InmateBrain.MinAttackInterval - 1e-3f, InmateBrain.MaxAttackInterval + Step));
            }
        }

        [Test]
        public void HoldsHisGroundWhenThePlayerIsDownOrOutsideHisCell()
        {
            InmateDecision decision = Run(10f, s_unavailable);
            Assert.AreEqual(0, _brain.AttacksStarted);
            Assert.AreEqual(InmateState.Ready, _brain.State);
            Assert.AreEqual(0f, decision.MoveSpeed);
        }

        [Test]
        public void KeepsAttackingJustBeyondAttackRangeButChasesFurther()
        {
            Run(Step, s_close);
            Run(Step, new InmatePerception(true, 1.5f, true));
            Assert.AreNotEqual(InmateState.Approach, _brain.State, "Separate enter and leave distances.");
            Run(Step, new InmatePerception(true, InmateBrain.ChaseRange + 0.1f, true));
            Assert.AreEqual(InmateState.Approach, _brain.State);
        }

        [Test]
        public void StepsInAndTurnsDuringTheWindUpThenCommitsToTheSwing()
        {
            Run(1f, s_close);
            Assert.AreEqual(FighterState.WindUp, _state.State);
            Assert.AreEqual(InmateBrain.WindUpStepSpeed, _last.MoveSpeed);
            Assert.AreEqual(InmateBrain.WindUpTurnSpeed, _last.TurnDegreesPerSecond);
            while (_state.State == FighterState.WindUp)
            {
                Run(Step, s_close);
            }
            // The brain decides before his fighter acts, so it sees the swing on the next frame.
            Run(Step, s_close);
            Assert.AreEqual(InmateState.Attack, _brain.State);
            Assert.AreEqual(0f, _last.MoveSpeed);
            Assert.AreEqual(0f, _last.TurnDegreesPerSecond);
        }

        [Test]
        public void StaggerStopsHimAndCancelsHisPunch()
        {
            Run(1f, s_close);
            Assert.AreEqual(FighterState.WindUp, _state.State);
            _defense.Receive(PlayerHit(12f, UnarmedAttack.Kick.StaggerPower));
            bool swung = false;
            _state.Changed += (from, to) => swung |= to == FighterState.Active;
            Run(Step, s_close);
            Assert.AreEqual(InmateState.Stagger, _brain.State);
            Assert.AreEqual(0f, _last.MoveSpeed);
            Run(FighterDefense.StaggerSeconds + 2f * Step, s_close);
            Assert.AreNotEqual(InmateState.Stagger, _brain.State);
            Assert.IsFalse(swung, "The interrupted punch never landed.");
        }

        [Test]
        public void DownedAndDeadStopEverything()
        {
            Run(Step, s_close);
            _defense.Receive(PlayerHit(100f));
            InmateDecision decision = Run(1f, s_close);
            Assert.AreEqual(InmateState.Downed, _brain.State);
            Assert.IsFalse(decision.Intent.Enabled);
            _health.Kill(new StableId("player"));
            Run(Step, s_close);
            Assert.AreEqual(InmateState.Dead, _brain.State);
        }

        [Test]
        public void HurtInmateWindsUpSlower()
        {
            Assert.AreEqual(1f, Run(Step, s_close).WindUpScale);
            _defense.Receive(PlayerHit(70f));
            Assert.AreEqual(InmateBrain.HurtWindUpScale, Run(Step, s_close).WindUpScale);
        }

        [Test]
        public void BlocksAfterSomeHitsButNotAll()
        {
            int blocked = 0;
            for (int seed = 0; seed < 100; seed++)
            {
                InmateBrain brain = MakeBrain(seed, out _, out _, out FighterDefense defense, out _);
                brain.Tick(Step, s_close);
                defense.Receive(PlayerHit(1f));
                InmateDecision decision = brain.Tick(Step, s_close);
                if (decision.Intent.BlockHeld)
                {
                    blocked++;
                    Assert.AreEqual(InmateState.Block, brain.State);
                    Assert.IsFalse(decision.Intent.PunchPressed);
                }
            }
            Assert.That(blocked, Is.InRange(15, 55), "About a third of hits make him raise his guard.");
        }

        [Test]
        public void TheSameSeedMakesTheSameChoices()
        {
            InmateBrain other = MakeBrain(7, out FighterStateMachine otherState, out _, out FighterDefense otherDefense,
                out FighterActions otherActions);
            for (int i = 0; i < 400; i++)
            {
                if (i % 37 == 0)
                {
                    _defense.Receive(PlayerHit(1f));
                    otherDefense.Receive(PlayerHit(1f));
                }
                Run(Step, s_close);
                InmateDecision decision = other.Tick(Step, s_close);
                otherActions.Tick(Step, decision.Intent);
                otherDefense.Tick(Step);
                Assert.AreEqual(_brain.State, other.State, $"Diverged at step {i}.");
            }
        }

        [Test]
        public void ResetReturnsHimToIdleWithTheSameChoicesAsAtTheStart()
        {
            Run(1f, s_close);
            _brain.Reset();
            _actions.Cancel();
            Assert.AreEqual(InmateState.Idle, _brain.State);
            InmateBrain fresh = MakeBrain(7, out _, out _, out _, out _);
            fresh.Tick(0f, s_close);
            _brain.Tick(0f, s_close);
            Assert.AreEqual(fresh.NextAttackIn, _brain.NextAttackIn, 1e-6f);
        }
    }
}
