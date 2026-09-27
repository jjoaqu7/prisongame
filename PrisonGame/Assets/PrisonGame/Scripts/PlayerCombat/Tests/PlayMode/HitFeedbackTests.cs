using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using PrisonGame.Combat;
using UnityEngine;
using UnityEngine.TestTools;

namespace PrisonGame.PlayerCombat.Tests
{
    // ENCOUNTER-01 step 3: hitstop, knockback, flinch, camera kick and hit sounds.
    [Category("PlayerCombat")]
    public sealed class HitFeedbackTests
    {
        private readonly List<GameObject> _objects = new List<GameObject>();
        private Fighter _actor;
        private Fighter _target;
        private MeleeSweep _sweep;
        private Transform _aim;
        private bool _previousBackground;
        private bool _hadShakeSetting;
        private int _previousShakeSetting;
        private static readonly WaitForSeconds s_recoverWait = new WaitForSeconds(0.6f);
        private static readonly WaitForSeconds s_longWait = new WaitForSeconds(1.5f);

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _previousBackground = Application.runInBackground;
            Application.runInBackground = true;
            _hadShakeSetting = PlayerPrefs.HasKey(CameraKick.EnabledKey);
            _previousShakeSetting = PlayerPrefs.GetInt(CameraKick.EnabledKey, 1);
            _actor = MakeFighter("feedback-player", Vector3.zero);
            _target = MakeFighter("feedback-target", new Vector3(0f, 0f, 1.2f));
            _aim = MakeObject("FeedbackAim").transform;
            _aim.SetParent(_actor.transform, false);
            _sweep = _actor.gameObject.AddComponent<MeleeSweep>();
            _sweep.Aim = _aim;
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
            if (_hadShakeSetting)
            {
                PlayerPrefs.SetInt(CameraKick.EnabledKey, _previousShakeSetting);
            }
            else
            {
                PlayerPrefs.DeleteKey(CameraKick.EnabledKey);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator LandedHitFreezesBothFightersForTheAttacksHitStop()
        {
            Punch();
            yield return null;
            Assert.AreEqual(90f, _target.Health.Current, "The punch must land for this test.");
            Assert.AreEqual(0.06f, _actor.HitStop.Remaining, 1e-4f);
            Assert.AreEqual(0.06f, _target.HitStop.Remaining, 1e-4f);
        }

        [UnityTest]
        public IEnumerator MissedSwingCausesNoHitStop()
        {
            _target.transform.position = new Vector3(0f, 0f, 2.5f);
            Physics.SyncTransforms();
            Punch();
            yield return null;
            Assert.AreEqual(100f, _target.Health.Current);
            Assert.IsFalse(_actor.HitStop.IsActive);
            Assert.IsFalse(_target.HitStop.IsActive);
        }

        [UnityTest]
        public IEnumerator HitStopHoldsAFightersTimelineThenReleasesIt()
        {
            _actor.enabled = true;
            _actor.State.TryStartAttack(new AttackTiming(5f, 0.1f, 0.1f));
            _actor.HitStop.Begin(1f);
            yield return null;
            yield return null;
            Assert.AreEqual(0f, _actor.State.TimeInState, "Frozen: no time reaches the timeline.");
            Assert.That(_actor.HitStop.Remaining, Is.LessThan(1f));
            yield return s_longWait;
            Assert.IsFalse(_actor.HitStop.IsActive);
            Assert.That(_actor.State.TimeInState, Is.GreaterThan(0f), "Time flows again after the freeze.");
        }

        [UnityTest]
        public IEnumerator KickKnocksTheTargetFurtherThanAPunch()
        {
            var receiver = _target.gameObject.AddComponent<SandboxHitReceiver>();
            Punch();
            yield return null;
            Assert.AreEqual(1.28f, _target.transform.position.z, 0.005f);
            Assert.AreEqual(0.08f, receiver.LastDisplacement, 0.005f);

            _actor.Actions.Tick(1f, new CombatIntent(true));
            _target.transform.position = new Vector3(0f, 0f, 1.2f);
            Physics.SyncTransforms();
            Kick();
            yield return null;
            Assert.AreEqual(78f, _target.Health.Current, 0.01f, "Punch 10, then kick 12.");
            Assert.AreEqual(1.8f, _target.transform.position.z, 0.005f);
        }

        [UnityTest]
        public IEnumerator KnockbackStopsBeforeAWall()
        {
            var receiver = _target.gameObject.AddComponent<SandboxHitReceiver>();
            GameObject wall = MakeObject("KnockbackWall");
            wall.transform.position = new Vector3(0f, 0f, 1.6f);
            wall.AddComponent<BoxCollider>().size = new Vector3(2f, 3f, 0.1f);
            Physics.SyncTransforms();
            Kick();
            yield return null;
            Assert.That(_target.transform.position.z, Is.InRange(1.2f, 1.31f));
            Assert.That(receiver.LastDisplacement, Is.LessThan(0.11f));
        }

        [UnityTest]
        public IEnumerator FlinchTiltsOnlyTheVisualAndRecovers()
        {
            Transform visual = MakeObject("FlinchVisual").transform;
            visual.SetParent(_target.transform, false);
            var flinch = _target.gameObject.AddComponent<HitFlinchView>();
            flinch.Configure(visual);
            Kick();
            // Hold the pose long enough to observe it whatever the frame rate.
            _target.HitStop.Begin(0.5f);
            _target.enabled = true;
            yield return null;
            Assert.AreEqual(HitFlinchView.KickDegrees, flinch.CurrentDegrees, 0.01f, "Kick: peak tilt, held during hitstop.");
            Assert.AreEqual(HitFlinchView.KickDegrees, Quaternion.Angle(Quaternion.identity, visual.localRotation), 0.05f);
            Assert.AreEqual(0f, Quaternion.Angle(Quaternion.identity, _target.transform.rotation), 0.001f,
                "The fighter's collider stays upright.");
            yield return s_longWait;
            Assert.That(flinch.CurrentDegrees, Is.LessThan(0.01f));
            Assert.That(Quaternion.Angle(Quaternion.identity, visual.localRotation), Is.LessThan(0.01f));
        }

        [UnityTest]
        public IEnumerator CameraKickNeverMovesTheAimAndCanBeTurnedOff()
        {
            CameraKick.IsOn = true;
            Camera view = MakeObject("FeedbackCamera").AddComponent<Camera>();
            view.transform.SetParent(_actor.transform, false);
            _sweep.Aim = view.transform;
            var kick = view.gameObject.AddComponent<CameraKick>();
            kick.Configure(_sweep);
            yield return null;
            Kick();
            yield return null;
            Assert.AreEqual(88f, _target.Health.Current, 0.01f, "The kick must land.");
            Assert.AreEqual(1.2f, kick.PeakDegrees, 0.001f, "A landed kick moves the view 1.2 degrees.");
            Assert.AreEqual(0f, Quaternion.Angle(Quaternion.identity, view.transform.localRotation), 0.001f,
                "Outside rendering the aim transform is untouched.");

            CameraKick.IsOn = false;
            yield return null;
            Assert.AreEqual(0f, kick.CurrentDegrees, "Turning the setting off removes the kick.");
        }

        [UnityTest]
        public IEnumerator HitSoundsPickHeavierLayeredSoundsForKicks()
        {
            var sounds = _actor.gameObject.AddComponent<FighterHitSounds>();
            sounds.Configure(Clips("punch"), Clips("kick"), Clips("layer"), Clips("shove"));
            yield return null;
            Punch();
            yield return null;
            Assert.AreEqual("punch", sounds.LastImpact.name);
            Assert.IsNull(sounds.LastLayer);

            _actor.Actions.Tick(1f, new CombatIntent(true));
            Kick();
            yield return null;
            Assert.AreEqual("kick", sounds.LastImpact.name);
            Assert.AreEqual("layer", sounds.LastLayer.name);

            _actor.Actions.Tick(1f, new CombatIntent(true));
            _actor.Actions.Tick(0f, new CombatIntent(true, shovePressed: true));
            _actor.Actions.Tick(0.06f, new CombatIntent(true));
            yield return null;
            Assert.AreEqual("shove", sounds.LastImpact.name);
        }

        private AudioClip[] Clips(string name)
        {
            return new[] { AudioClip.Create(name, 441, 1, 44100, false) };
        }

        private void Punch(float advance = 0.07f)
        {
            _actor.Actions.Tick(0f, new CombatIntent(true, punchPressed: true));
            _actor.Actions.Tick(advance, new CombatIntent(true));
        }

        private void Kick(float advance = 0.31f)
        {
            _actor.Actions.Tick(0f, new CombatIntent(true, kickPressed: true));
            _actor.Actions.Tick(advance, new CombatIntent(true));
        }

        private GameObject MakeObject(string name)
        {
            var item = new GameObject(name);
            _objects.Add(item);
            return item;
        }

        private Fighter MakeFighter(string id, Vector3 position)
        {
            GameObject item = MakeObject(id);
            item.SetActive(false);
            item.transform.position = position;
            Fighter fighter = item.AddComponent<Fighter>();
            typeof(Fighter).GetField("_stableId", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(fighter, id);
            item.AddComponent<CapsuleCollider>().radius = 0.25f;
            item.SetActive(true);
            fighter.enabled = false;
            return fighter;
        }
    }
}
