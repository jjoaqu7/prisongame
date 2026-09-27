using System;
using System.Collections.Generic;
using NUnit.Framework;
using PrisonGame.Core;

namespace PrisonGame.Combat.Tests
{
    [Category("Combat")]
    public sealed class FightSessionTests
    {
        private static readonly StableId s_player = new StableId("player");
        private static readonly StableId s_inmate = new StableId("inmate");
        private static readonly FightStatus s_inside = new FightStatus(false, false, true);
        private static readonly FightStatus s_outside = new FightStatus(false, false, false);
        private FightSession _session;
        private List<FightEnd> _ends;
        private int _restores;

        [SetUp]
        public void SetUp()
        {
            _session = new FightSession(s_inmate);
            _ends = new List<FightEnd>();
            _restores = 0;
            _session.Ended += _ends.Add;
            _session.RestoreRequested += () => _restores++;
        }

        [Test]
        public void NothingHappensBeforeTheFirstHit()
        {
            _session.Tick(60f, s_outside);
            Assert.AreEqual(FightPhase.NotStarted, _session.Phase);
            Assert.IsFalse(_session.ChallengerHealthVisible);
        }

        [Test]
        public void FirstHitStartsTheFightOnceAndNamesWhoStartedIt()
        {
            StableId started = default;
            _session.Started += id => started = id;
            Assert.IsTrue(_session.TryStart(s_player));
            Assert.IsFalse(_session.TryStart(s_player), "Later hits do not restart it.");
            Assert.AreEqual(FightPhase.Active, _session.Phase);
            Assert.AreEqual(s_player, started);
            Assert.AreEqual(s_player, _session.Challenger);
            Assert.IsTrue(_session.ChallengerHealthVisible);
        }

        [Test]
        public void TheOpponentCannotBeTheChallenger()
        {
            Assert.Throws<ArgumentException>(() => _session.TryStart(s_inmate));
        }

        [Test]
        public void StayingOutsideForTenSecondsEndsTheFight()
        {
            _session.TryStart(s_player);
            _session.Tick(FightSession.LeaveSeconds - 0.1f, s_outside);
            Assert.AreEqual(FightPhase.Active, _session.Phase);
            _session.Tick(0.2f, s_outside);
            Assert.AreEqual(FightPhase.NotStarted, _session.Phase);
            CollectionAssert.AreEqual(new[] { FightEnd.LeftArea }, _ends);
        }

        [Test]
        public void ComingBackInsideRestartsTheTenSeconds()
        {
            _session.TryStart(s_player);
            _session.Tick(9f, s_outside);
            _session.Tick(0.1f, s_inside);
            _session.Tick(9f, s_outside);
            Assert.AreEqual(FightPhase.Active, _session.Phase);
        }

        [Test]
        public void AFightEndedByLeavingCanStartAgain()
        {
            _session.TryStart(s_player);
            _session.Tick(FightSession.LeaveSeconds, s_outside);
            Assert.IsTrue(_session.TryStart(s_player));
        }

        [Test]
        public void DowningTheOpponentWinsAndTheHealthBarLingersBriefly()
        {
            _session.TryStart(s_player);
            _session.Tick(0.1f, new FightStatus(false, true, true));
            Assert.AreEqual(FightPhase.Won, _session.Phase);
            CollectionAssert.AreEqual(new[] { FightEnd.Won }, _ends);
            _session.Tick(FightSession.HealthBarLingerSeconds - 0.1f, s_inside);
            Assert.IsTrue(_session.ChallengerHealthVisible);
            _session.Tick(0.2f, s_inside);
            Assert.IsFalse(_session.ChallengerHealthVisible);
            Assert.IsFalse(_session.TryStart(s_player), "A won fight does not start again.");
        }

        [Test]
        public void LosingFadesThenAsksForTheCheckpointOnce()
        {
            _session.TryStart(s_player);
            _session.Tick(0.1f, new FightStatus(true, false, true));
            Assert.AreEqual(FightPhase.Lost, _session.Phase);
            CollectionAssert.AreEqual(new[] { FightEnd.Lost }, _ends);
            _session.Tick(FightSession.LossFadeSeconds - 0.1f, s_inside);
            Assert.AreEqual(0, _restores, "The restore waits for the fade.");
            _session.Tick(0.2f, s_inside);
            Assert.AreEqual(1, _restores);
            Assert.AreEqual(FightPhase.NotStarted, _session.Phase, "After the checkpoint the fight has not started.");
            _session.Tick(5f, s_inside);
            Assert.AreEqual(1, _restores);
            Assert.IsTrue(_session.TryStart(s_player));
        }

        [Test]
        public void LosingWinsOverTheOpponentGoingDownInTheSameTick()
        {
            _session.TryStart(s_player);
            _session.Tick(0.1f, new FightStatus(true, true, true));
            Assert.AreEqual(FightPhase.Lost, _session.Phase);
        }

        [Test]
        public void RejectsNegativeTime()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _session.Tick(-1f, s_inside));
        }
    }
}
