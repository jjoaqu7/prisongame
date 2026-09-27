using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using PrisonGame.Combat;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace PrisonGame.PlayerCombat.Tests
{
    [Category("PlayerCombat")]
    public sealed class PlayerCombatTests
    {
        private readonly List<GameObject> _objects = new List<GameObject>();
        private Fighter _actor;
        private Fighter _target;
        private MeleeSweep _sweep;
        private Transform _aim;
        private Mouse _mouse;
        private Keyboard _keyboard;
        private CursorLockMode _previousLock;
        private bool _previousBackground;
        private static readonly WaitForSeconds s_warmup = new WaitForSeconds(0.1f);
        private static readonly WaitForSeconds s_swingWait = new WaitForSeconds(0.25f);
        private InputSettings.BackgroundBehavior _previousBackgroundBehavior;
        private List<InputDevice> _realDevices;
#if UNITY_EDITOR
        private InputSettings.EditorInputBehaviorInPlayMode _previousInputBehavior;
#endif

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _previousBackground = Application.runInBackground;
            Application.runInBackground = true;
            _previousLock = Cursor.lockState;
            // Keep the simulated mouse and keyboard enabled when Unity is not the focused app.
            _previousBackgroundBehavior = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            _realDevices = InputIsolation.DisableRealPointerAndKeyboard();
#if UNITY_EDITOR
            _previousInputBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode =
                InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            _actor = MakeFighter("test-player", Vector3.zero);
            _target = MakeFighter("test-target", new Vector3(0f, 0f, 1.2f));
            _aim = MakeObject("TestAim").transform;
            _aim.SetParent(_actor.transform, false);
            _sweep = _actor.gameObject.AddComponent<MeleeSweep>();
            _sweep.Aim = _aim;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_mouse != null)
            {
                InputSystem.RemoveDevice(_mouse);
            }
            if (_keyboard != null)
            {
                InputSystem.RemoveDevice(_keyboard);
            }
            foreach (GameObject item in _objects)
            {
                if (item != null)
                {
                    Object.Destroy(item);
                }
            }
            _objects.Clear();
            Cursor.lockState = _previousLock;
            Application.runInBackground = _previousBackground;
            InputSystem.settings.backgroundBehavior = _previousBackgroundBehavior;
            InputIsolation.Restore(_realDevices);
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode = _previousInputBehavior;
#endif
            yield return null;
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

        [UnityTest]
        public IEnumerator MultipleCollidersAndRepeatedSamplesOnlyDamageOnce()
        {
            var extra = MakeObject("ExtraHitCollider");
            extra.transform.SetParent(_target.transform, false);
            extra.AddComponent<SphereCollider>().radius = 0.3f;
            Punch();
            _sweep.Sample();
            _sweep.Sample();
            yield return null;
            Assert.AreEqual(90f, _target.Health.Current);
            Assert.AreEqual(100f, _actor.Health.Current, "The player's own capsule is excluded.");
        }

        [UnityTest]
        public IEnumerator AKickReachesPastAPunchAndDealsTwelveDamage()
        {
            _target.transform.position = new Vector3(0f, 0f, 1.5f);
            Physics.SyncTransforms();
            Punch();
            Assert.AreEqual(100f, _target.Health.Current, "1.5 m is beyond a punch.");
            _actor.Actions.Tick(1f, new CombatIntent(true));
            Kick();
            yield return null;
            Assert.AreEqual(88f, _target.Health.Current);
        }

        [UnityTest]
        public IEnumerator BeyondReachMissesAndASecondSwingCanHit()
        {
            _target.transform.position = new Vector3(0f, 0f, 1.5f);
            Punch();
            Assert.AreEqual(100f, _target.Health.Current);
            _actor.Actions.Tick(1f, new CombatIntent(true));
            _target.transform.position = new Vector3(0f, 0f, 1.2f);
            Punch();
            yield return null;
            Assert.AreEqual(90f, _target.Health.Current);
        }

        [UnityTest]
        public IEnumerator WallBlocksTheTarget()
        {
            GameObject wall = MakeObject("Wall");
            wall.transform.position = new Vector3(0f, 0f, 0.55f);
            wall.AddComponent<BoxCollider>().size = new Vector3(2f, 2f, 0.08f);
            Punch();
            yield return null;
            Assert.AreEqual(100f, _target.Health.Current);
        }

        [UnityTest]
        public IEnumerator WallOverlappingTheSweepOriginAlsoBlocksTheTarget()
        {
            MakeObject("OverlappingWall").AddComponent<BoxCollider>().size = Vector3.one * 0.1f;
            Punch();
            yield return null;
            Assert.AreEqual(100f, _target.Health.Current);
        }

        [UnityTest]
        public IEnumerator TurningSweepsAcrossATargetBetweenFrames()
        {
            _target.GetComponent<CapsuleCollider>().radius = 0.05f;
            _target.transform.position = new Vector3(0f, 0f, 1f);
            _aim.rotation = Quaternion.Euler(0f, -25f, 0f);
            Punch();
            Assert.AreEqual(100f, _target.Health.Current);
            _aim.rotation = Quaternion.Euler(0f, 25f, 0f);
            _sweep.Sample();
            yield return null;
            Assert.AreEqual(90f, _target.Health.Current);
        }

        [UnityTest]
        public IEnumerator LongFrameCannotSkipTheHitWindow()
        {
            Punch(1f);
            yield return null;
            Assert.AreEqual(90f, _target.Health.Current);
            Assert.AreEqual(FighterState.Idle, _actor.State.State);
        }

        [UnityTest]
        public IEnumerator ShoveReportsActorAndStopsBeforeAWallWithoutDamage()
        {
            var receiver = _target.gameObject.AddComponent<SandboxHitReceiver>();
            GameObject wall = MakeObject("ShoveWall");
            wall.transform.position = new Vector3(0f, 0f, 1.95f);
            wall.AddComponent<BoxCollider>().size = new Vector3(2f, 3f, 0.1f);
            _actor.Actions.Tick(0f, new CombatIntent(true, shovePressed: true));
            _actor.Actions.Tick(0.06f, new CombatIntent(true));
            yield return null;
            Assert.AreEqual(_actor.Id, receiver.LastAttacker);
            Assert.AreEqual(FighterState.Staggered, _target.State.State);
            Assert.That(_target.transform.position.z, Is.InRange(1.21f, 1.66f));
            Assert.AreEqual(100f, _target.Health.Current);
        }

        [UnityTest]
        public IEnumerator ShoveMovesTheDummyOneMetreWhenThereIsSpace()
        {
            _target.gameObject.AddComponent<SandboxHitReceiver>();
            _actor.Actions.Tick(0f, new CombatIntent(true, shovePressed: true));
            _actor.Actions.Tick(0.06f, new CombatIntent(true));
            yield return null;
            Assert.AreEqual(2.2f, _target.transform.position.z, 0.001f);
        }

        [UnityTest]
        public IEnumerator OtherCharactersHaveNoShoveReaction()
        {
            bool reported = false;
            _sweep.HitResolved += (target, hit, result) => reported = hit.Kind == AttackKind.Shove;
            _actor.Actions.Tick(0f, new CombatIntent(true, shovePressed: true));
            _actor.Actions.Tick(0.06f, new CombatIntent(true));
            yield return null;
            Assert.IsTrue(reported);
            Assert.AreEqual(1.2f, _target.transform.position.z);
            Assert.AreEqual(FighterState.Idle, _target.State.State);
            Assert.AreEqual(100f, _target.Health.Current);
        }

        private IEnumerator EnableInput()
        {
            _mouse = InputSystem.AddDevice<Mouse>();
            _keyboard = InputSystem.AddDevice<Keyboard>();
            _actor.gameObject.AddComponent<LocalCombatInput>();
            _actor.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            InputSystem.QueueStateEvent(_mouse, new MouseState());
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState());
            yield return null;
            yield return null;
            Assert.AreEqual(CursorLockMode.Locked, Cursor.lockState, "Input check requires captured controls.");
        }

        [UnityTest]
        public IEnumerator MousePressMovesPlaceholderFistWithinOneHundredMilliseconds()
        {
            Transform left = MakeObject("LeftFist").transform;
            Transform right = MakeObject("RightFist").transform;
            left.SetParent(_aim, false);
            right.SetParent(_aim, false);
            var view = _aim.gameObject.AddComponent<CombatArmsView>();
            view.Configure(_actor, left, right);
            yield return EnableInput();
            yield return s_warmup;
            double queuedAt = Time.realtimeSinceStartupAsDouble;
            InputSystem.QueueStateEvent(_mouse, new MouseState().WithButton(MouseButton.Left));
            yield return null;
            yield return null;
            // The punch fires on press, so its 0.06 s pull-back may already be over; check when it was shown.
            Assert.That(view.PullbackShownAt, Is.GreaterThanOrEqualTo(queuedAt), "The press showed the pull-back.");
            double milliseconds = (view.PullbackShownAt - queuedAt) * 1000d;
            Debug.Log($"[Step2 latency] queued mouse press to LateUpdate fist pose: {milliseconds:F2} ms");
            Assert.That(milliseconds, Is.InRange(0d, 99.999d));
            InputSystem.QueueStateEvent(_mouse, new MouseState());
            yield return s_swingWait;
            Assert.AreEqual(90f, _target.Health.Current);
        }

        [UnityTest]
        public IEnumerator EDoesNotAttackRmbBlocksAndFShoves()
        {
            yield return EnableInput();
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(Key.E));
            yield return null;
            yield return null;
            Assert.AreEqual(0, _actor.Actions.SwingNumber);
            InputSystem.QueueStateEvent(_mouse, new MouseState().WithButton(MouseButton.Right));
            yield return null;
            yield return null;
            Assert.AreEqual(FighterState.Blocking, _actor.State.State);
            InputSystem.QueueStateEvent(_mouse, new MouseState());
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState());
            yield return null;
            yield return null;
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(Key.F));
            yield return null;
            yield return null;
            Assert.AreEqual(AttackKind.Shove, _actor.Actions.CurrentAttack.Kind);
            Assert.AreEqual(1, _actor.Actions.SwingNumber);
        }

        [UnityTest]
        public IEnumerator VKicksThroughTheLocalInputAdapter()
        {
            yield return EnableInput();
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(Key.V));
            yield return null;
            yield return null;
            Assert.AreEqual(1, _actor.Actions.SwingNumber);
            Assert.AreEqual(AttackKind.Kick, _actor.Actions.CurrentAttack.Kind);
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState());
            yield return s_swingWait;
            yield return s_swingWait;
            Assert.AreEqual(88f, _target.Health.Current);
        }

        [UnityTest]
        public IEnumerator MenuGateCancelsTheSwingAndRequiresAFreshPressAfterResume()
        {
            yield return EnableInput();
            var input = _actor.GetComponent<LocalCombatInput>();
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(Key.V));
            yield return null;
            yield return null;
            Assert.AreEqual(1, _actor.Actions.SwingNumber);
            input.WorldActionsEnabled = false;
            yield return null;
            yield return null;
            Assert.AreEqual(FighterState.Idle, _actor.State.State, "Opening a menu cancels the kick.");
            Assert.AreEqual(100f, _target.Health.Current);
            input.WorldActionsEnabled = true;
            yield return null;
            yield return null;
            Assert.AreEqual(1, _actor.Actions.SwingNumber, "A key still held from before the menu does not attack.");
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState());
            yield return null;
            yield return null;
            InputSystem.QueueStateEvent(_mouse, new MouseState().WithButton(MouseButton.Left));
            yield return null;
            yield return null;
            Assert.AreEqual(2, _actor.Actions.SwingNumber);
            Assert.AreEqual(AttackKind.Punch, _actor.Actions.CurrentAttack.Kind);
        }
    }
}
