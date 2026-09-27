using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using PrisonGame.Combat;
using UnityEngine;
using UnityEngine.TestTools;

namespace PrisonGame.AI.Tests.PlayMode
{
    // ENCOUNTER-01 step 4 done criteria: he damages an idle player, a timed block prevents it, and a loss
    // restores the pre-fight state. The player is driven through the same intents its input adapter sends.
    [Category("AI")]
    public sealed class InmateFightTests
    {
        private const int IgnoreRaycastLayer = 2;
        private static readonly CombatIntent s_idle = new CombatIntent(true);
        private static readonly CombatIntent s_punch = new CombatIntent(true, punchPressed: true);
        private static readonly CombatIntent s_block = new CombatIntent(true, blockHeld: true);
        private static readonly Vector3 s_playerStart = Vector3.zero;
        private static readonly Vector3 s_inmateStart = new Vector3(0f, 0.9f, 1.2f);
        private readonly List<GameObject> _objects = new List<GameObject>();
        private Fighter _player;
        private Fighter _inmate;
        private InmateFighterController _controller;
        private FightController _fight;
        private bool _previousBackground;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _previousBackground = Application.runInBackground;
            Application.runInBackground = true;
            _player = MakeFighter("fight-player", s_playerStart, Quaternion.identity, 0.3f, 1.7f, true);
            _inmate = MakeFighter("fight-inmate", s_inmateStart, Quaternion.Euler(0f, 180f, 0f), 0.4f, 0.6f, false);
            var fightObject = new GameObject("TestFight");
            _objects.Add(fightObject);
            fightObject.SetActive(false);
            _fight = fightObject.AddComponent<FightController>();
            _fight.Configure(_player, _inmate, new Bounds(new Vector3(0f, 1.4f, 0f), new Vector3(4f, 2.8f, 5f)));
            fightObject.SetActive(true);
            _inmate.gameObject.SetActive(false);
            _controller = _inmate.gameObject.AddComponent<InmateFighterController>();
            _controller.Configure(_fight, 20260927);
            _inmate.gameObject.SetActive(true);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (GameObject item in _objects)
            {
                if (item != null)
                {
                    Object.Destroy(item);
                }
            }
            _objects.Clear();
            Application.runInBackground = _previousBackground;
            yield return null;
        }

        [UnityTest]
        public IEnumerator HeDamagesAnIdlePlayer()
        {
            yield return PunchToStartTheFight();
            Assert.IsTrue(_fight.Session.IsActive, "The first hit starts the fight.");
            yield return WaitUntil(() => _player.Health.Current < 100f, 5f, s_idle);
            Assert.AreEqual(100f - UnarmedAttack.InmatePunch.Damage, _player.Health.Current, 1e-4f);
        }

        [UnityTest]
        public IEnumerator TimedBlockPreventsHisDamageAndStaggersHim()
        {
            bool staggered = false;
            var outcomes = new List<BlockOutcome>();
            _inmate.State.Changed += (from, to) => staggered |= to == FighterState.Staggered;
            _player.Defense.HitTaken += (hit, result) => outcomes.Add(result.Block);
            yield return PunchToStartTheFight();
            yield return WaitUntil(() => _inmate.State.State == FighterState.WindUp &&
                _inmate.State.CurrentAttack.WindUp - _inmate.State.TimeInState <= 0.2f, 5f, s_idle);
            Assert.AreEqual(FighterState.WindUp, _inmate.State.State, "He must be winding up for this test.");
            yield return WaitUntil(() => outcomes.Count > 0, 1f, s_block);
            Assert.AreEqual(new[] { BlockOutcome.TimedBlock }, outcomes.ToArray());
            Assert.AreEqual(100f, _player.Health.Current, "A timed block takes no damage.");
            Assert.IsTrue(staggered, "He staggers after his punch meets a timed block.");
        }

        [UnityTest]
        public IEnumerator HeldBlockTakesAQuarterOfHisDamage()
        {
            yield return PunchToStartTheFight();
            // Block well before his punch, so it is an ordinary block rather than a timed one.
            yield return WaitUntil(() => _player.Health.Current < 100f, 5f, s_block);
            Assert.AreEqual(100f - UnarmedAttack.InmatePunch.Damage * FighterDefense.BlockedDamageFactor,
                _player.Health.Current, 1e-4f);
        }

        [UnityTest]
        public IEnumerator LosingRestoresThePreFightState()
        {
            yield return PunchToStartTheFight();
            Assert.IsTrue(_fight.HasCheckpoint);
            Assert.AreEqual(90f, _inmate.Health.Current, "The punch landed after the checkpoint.");
            _player.transform.position = new Vector3(0.5f, 0f, -1f);
            _player.ApplyHit(new DamageInfo(_inmate.Id, "fists", AttackKind.Punch, 500f, Vector3.zero, Vector3.back, 0d));
            Assert.IsFalse(_player.Health.IsAlive);
            yield return null;
            Assert.AreEqual(FightPhase.Lost, _fight.Session.Phase);
            bool restored = false;
            _fight.CheckpointRestored += () => restored = true;
            yield return WaitUntil(() => restored, FightSession.LossFadeSeconds + 1f, s_idle);
            Assert.IsTrue(restored);
            Assert.AreEqual(FightPhase.NotStarted, _fight.Session.Phase);
            Assert.AreEqual(100f, _player.Health.Current);
            Assert.AreEqual(LifeState.Alive, _player.Health.State);
            Assert.AreEqual(FighterState.Idle, _player.State.State);
            Assert.That(Vector3.Distance(_player.transform.position, s_playerStart), Is.LessThan(0.001f));
            Assert.AreEqual(100f, _inmate.Health.Current, "His health is back to before the first punch.");
            Assert.That(Vector3.Distance(_inmate.transform.position, s_inmateStart), Is.LessThan(0.001f));
            Assert.AreEqual(InmateState.Idle, _controller.Brain.State);
        }

        [UnityTest]
        public IEnumerator HeApproachesAndStopsShortOfThePlayer()
        {
            _player.transform.position = new Vector3(0f, 0f, -1.5f);
            _inmate.transform.position = new Vector3(0f, 0.9f, 1.5f);
            Physics.SyncTransforms();
            _fight.TakeCheckpoint();
            _fight.Session.TryStart(_player.Id);
            yield return WaitUntil(() => PlanarDistance() <= InmateBrain.AttackRange, 3f, s_idle);
            Assert.That(PlanarDistance(), Is.LessThanOrEqualTo(InmateBrain.AttackRange));
            yield return Wait(1f, s_idle);
            Assert.That(PlanarDistance(), Is.GreaterThanOrEqualTo(InmateBrain.PreferredDistance - 0.01f));
        }

        [UnityTest]
        public IEnumerator LeavingHisCellEndsTheFightWithBothBackToFullHealth()
        {
            yield return PunchToStartTheFight();
            _player.ApplyHit(new DamageInfo(_inmate.Id, "fists", AttackKind.Punch, 30f, Vector3.zero, Vector3.back, 0d));
            _fight.Session.Tick(FightSession.LeaveSeconds, new FightStatus(false, false, false));
            Assert.AreEqual(FightPhase.NotStarted, _fight.Session.Phase);
            Assert.AreEqual(100f, _player.Health.Current);
            Assert.AreEqual(100f, _inmate.Health.Current);
        }

        [UnityTest]
        public IEnumerator DowningHimWinsAndRefillsThePlayersHealth()
        {
            yield return PunchToStartTheFight();
            _player.ApplyHit(new DamageInfo(_inmate.Id, "fists", AttackKind.Punch, 30f, Vector3.zero, Vector3.back, 0d));
            _inmate.ApplyHit(new DamageInfo(_player.Id, "fists", AttackKind.Punch, 500f, Vector3.zero, Vector3.forward, 0d));
            yield return null;
            Assert.AreEqual(FightPhase.Won, _fight.Session.Phase);
            Assert.AreEqual(100f, _player.Health.Current);
            Assert.AreEqual(LifeState.Downed, _inmate.Health.State);
            Assert.AreEqual(InmateState.Downed, _controller.Brain.State);
        }

        private IEnumerator PunchToStartTheFight()
        {
            _player.SubmitIntent(s_punch);
            // The intent is used in the next frame's update; submitting another first would replace it.
            yield return null;
            yield return WaitUntil(() => _fight.Session.IsActive, 1f, s_idle);
        }

        private IEnumerator WaitUntil(System.Func<bool> condition, float seconds, CombatIntent intent)
        {
            float waited = 0f;
            while (!condition() && waited < seconds)
            {
                _player.SubmitIntent(intent);
                yield return null;
                waited += Time.deltaTime;
            }
        }

        private IEnumerator Wait(float seconds, CombatIntent intent) => WaitUntil(() => false, seconds, intent);

        private float PlanarDistance()
        {
            Vector3 offset = _inmate.transform.position - _player.transform.position;
            offset.y = 0f;
            return offset.magnitude;
        }

        private Fighter MakeFighter(string id, Vector3 position, Quaternion rotation, float radius, float aimHeight,
            bool canTimedBlock)
        {
            var item = new GameObject(id);
            _objects.Add(item);
            item.SetActive(false);
            // The prototype player is on the Ignore Raycast layer; hit checks must still find it.
            item.layer = canTimedBlock ? IgnoreRaycastLayer : 0;
            item.transform.SetPositionAndRotation(position, rotation);
            Fighter fighter = item.AddComponent<Fighter>();
            typeof(Fighter).GetField("_stableId", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(fighter, id);
            typeof(Fighter).GetField("_canTimedBlock", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(fighter, canTimedBlock);
            var capsule = item.AddComponent<CapsuleCollider>();
            capsule.radius = radius;
            capsule.height = 1.8f;
            // The player's pivot is at its feet and the inmate's at his centre, as in the sandbox.
            capsule.center = canTimedBlock ? new Vector3(0f, 0.9f, 0f) : Vector3.zero;
            var aim = new GameObject(id + "-aim").transform;
            aim.SetParent(item.transform, false);
            aim.localPosition = new Vector3(0f, aimHeight, 0f);
            item.AddComponent<MeleeSweep>().Aim = aim;
            item.SetActive(true);
            return fighter;
        }
    }
}
