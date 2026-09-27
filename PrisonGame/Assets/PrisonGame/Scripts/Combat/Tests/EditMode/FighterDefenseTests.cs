using NUnit.Framework;
using PrisonGame.Core;
using UnityEngine;

namespace PrisonGame.Combat.Tests
{
    [Category("Combat")]
    public sealed class FighterDefenseTests
    {
        private static readonly StableId s_attacker = new StableId("inmate");
        private Health _health;
        private FighterStateMachine _state;
        private FighterDefense _defense;

        [SetUp]
        public void SetUp()
        {
            _health = new Health(100f);
            _state = new FighterStateMachine();
            _defense = new FighterDefense(_health, _state, canTimedBlock: true);
            // As Fighter wires them.
            _health.WentDown += _ => _state.GoDown();
        }

        private static DamageInfo Hit(float amount, float staggerPower = 0f, AttackKind kind = AttackKind.Punch) =>
            new DamageInfo(s_attacker, "fists", kind, amount, Vector3.zero, Vector3.forward, 0d, staggerPower: staggerPower);

        // Raises the block and lets it be held for the given time before the hit.
        private void Block(float heldSeconds)
        {
            _state.StartBlock();
            _state.Tick(heldSeconds);
            _defense.Tick(heldSeconds);
        }

        [Test]
        public void UnblockedHitDoesFullDamage()
        {
            DamageResult result = _defense.Receive(Hit(12f));
            Assert.AreEqual(88f, _health.Current);
            Assert.AreEqual(BlockOutcome.None, result.Block);
        }

        [Test]
        public void BlockedHitDoesAQuarterOfItsDamage()
        {
            Block(0.5f);
            DamageResult result = _defense.Receive(Hit(12f));
            Assert.AreEqual(BlockOutcome.Blocked, result.Block);
            Assert.AreEqual(3f, result.Applied, 1e-4f);
            Assert.AreEqual(97f, _health.Current, 1e-4f);
        }

        [Test]
        public void BlockRaisedJustBeforeTheHitIsATimedBlockWithNoDamage()
        {
            Block(0.2f);
            DamageResult result = _defense.Receive(Hit(12f));
            Assert.AreEqual(BlockOutcome.TimedBlock, result.Block);
            Assert.AreEqual(0f, result.Applied);
            Assert.AreEqual(100f, _health.Current);
        }

        [Test]
        public void TimedBlockWindowEndsAfterAQuarterSecond()
        {
            Block(FighterDefense.TimedBlockWindowSeconds);
            Assert.AreEqual(BlockOutcome.TimedBlock, _defense.Evaluate(), "The window includes its last moment.");
            _state.Tick(0.01f);
            Assert.AreEqual(BlockOutcome.Blocked, _defense.Evaluate());
        }

        [Test]
        public void RaisingTheBlockAgainTooSoonGivesANormalBlock()
        {
            Block(0.1f);
            _state.EndBlock();
            _defense.Tick(FighterDefense.BlockSpamSeconds - 0.1f);
            _state.StartBlock();
            Assert.AreEqual(BlockOutcome.Blocked, _defense.Evaluate(), "Spamming the block cannot make every block timed.");

            _state.EndBlock();
            _defense.Tick(FighterDefense.BlockSpamSeconds);
            _state.StartBlock();
            Assert.AreEqual(BlockOutcome.TimedBlock, _defense.Evaluate());
        }

        [Test]
        public void AFighterWithoutTimedBlocksOnlyEverBlocks()
        {
            var defense = new FighterDefense(new Health(100f), _state, canTimedBlock: false);
            _state.StartBlock();
            Assert.AreEqual(BlockOutcome.Blocked, defense.Evaluate());
        }

        [Test]
        public void PunchesBuildStaggerAndTheFourthStaggers()
        {
            float punch = UnarmedAttack.Punch.StaggerPower;
            for (int i = 0; i < 3; i++)
            {
                _defense.Receive(Hit(10f, punch));
                Assert.AreEqual(FighterState.Idle, _state.State, "One punch never interrupts him.");
            }
            _defense.Receive(Hit(10f, punch));
            Assert.AreEqual(FighterState.Staggered, _state.State);
        }

        [Test]
        public void KickStaggersEvenThroughABlock()
        {
            Block(0.5f);
            DamageResult result = _defense.Receive(Hit(12f, UnarmedAttack.Kick.StaggerPower, AttackKind.Kick));
            Assert.AreEqual(BlockOutcome.Blocked, result.Block);
            Assert.AreEqual(FighterState.Staggered, _state.State, "The kick breaks his guard.");
        }

        [Test]
        public void KickInterruptsAWindUp()
        {
            _state.TryStartAttack(UnarmedAttack.InmatePunch.Timing);
            _defense.Receive(Hit(12f, UnarmedAttack.Kick.StaggerPower, AttackKind.Kick));
            Assert.AreEqual(FighterState.Staggered, _state.State);
        }

        [Test]
        public void HurtFighterIsEasierToStagger()
        {
            _defense.Receive(Hit(70f));
            Assert.IsTrue(_defense.IsHurt, "30 of 100 is below a third.");
            float punch = UnarmedAttack.Punch.StaggerPower;
            _defense.Receive(Hit(1f, punch));
            _defense.Receive(Hit(1f, punch));
            Assert.AreEqual(FighterState.Idle, _state.State);
            _defense.Receive(Hit(1f, punch));
            Assert.AreEqual(FighterState.Staggered, _state.State, "Three punches instead of four.");
        }

        [Test]
        public void NotHurtAboveAThirdOfHealth()
        {
            _defense.Receive(Hit(60f));
            Assert.IsFalse(_defense.IsHurt);
        }

        [Test]
        public void TheHitThatDownsHimDoesNotStagger()
        {
            _defense.Receive(Hit(100f, UnarmedAttack.Kick.StaggerPower));
            Assert.AreEqual(FighterState.Downed, _state.State);
            Assert.AreEqual(0f, _defense.Stagger.Value);
        }

        [Test]
        public void HitsOnADownedFighterAreIgnoredAndNotReported()
        {
            _health.Apply(Hit(100f));
            int reports = 0;
            _defense.HitTaken += (hit, result) => reports++;
            DamageResult result = _defense.Receive(Hit(10f, 25f));
            Assert.IsTrue(result.Ignored);
            Assert.AreEqual(0, reports);
        }

        [Test]
        public void EveryHitOnALivingFighterIsReportedIncludingTimedBlocks()
        {
            var outcomes = new System.Collections.Generic.List<BlockOutcome>();
            _defense.HitTaken += (hit, result) => outcomes.Add(result.Block);
            _defense.Receive(Hit(1f));
            Block(0.1f);
            _defense.Receive(Hit(1f));
            CollectionAssert.AreEqual(new[] { BlockOutcome.None, BlockOutcome.TimedBlock }, outcomes);
        }

        [Test]
        public void ResetEmptiesTheStaggerMeter()
        {
            _defense.Receive(Hit(1f, 75f));
            _defense.Reset();
            Assert.AreEqual(0f, _defense.Stagger.Value);
        }
    }
}
