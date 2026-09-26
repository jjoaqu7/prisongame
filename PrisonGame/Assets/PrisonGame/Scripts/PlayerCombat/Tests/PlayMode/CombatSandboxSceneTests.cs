#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using PrisonGame.Combat;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PrisonGame.PlayerCombat.Tests
{
    [Category("PlayerCombat")]
    public sealed class CombatSandboxSceneTests
    {
        private static readonly WaitForSeconds s_settle = new WaitForSeconds(0.1f);
        private static readonly WaitForSeconds s_attack = new WaitForSeconds(0.55f);

        [UnityTest]
        public IEnumerator BuiltSandboxSupportsInputAndKeepsPrototypeMovementAndInteraction()
        {
            bool background = Application.runInBackground;
            var behavior = InputSystem.settings.editorInputBehaviorInPlayMode;
            Mouse mouse = null;
            Keyboard keyboard = null;
            try
            {
                Application.runInBackground = true;
                InputSystem.settings.editorInputBehaviorInPlayMode =
                    InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                EditorSceneManager.LoadSceneInPlayMode(
                    "Assets/PrisonGame/Scenes/Sandbox/CombatAttacksSandbox.unity",
                    new LoadSceneParameters(LoadSceneMode.Single));
                yield return null;
                GameObject player = GameObject.Find("Player");
                Fighter actor = player.GetComponent<Fighter>();
                Fighter dummy = GameObject.Find("Sandbox dummy").GetComponent<Fighter>();
                Assert.AreEqual("sandbox-player", actor.Id.Value);
                Assert.AreEqual("sandbox-dummy", dummy.Id.Value);
                Assert.AreEqual(70f, player.GetComponentInChildren<Camera>().fieldOfView);
                Assert.IsNotNull(player.GetComponent("PlayerInteraction"));
                Component movement = player.GetComponent("FirstPersonController");
                Assert.IsNotNull(movement);
                var controller = player.GetComponent<CharacterController>();
                controller.enabled = false;
                player.transform.SetPositionAndRotation(new Vector3(0.4f, 0.05f, 0.25f), Quaternion.identity);
                controller.enabled = true;
                mouse = InputSystem.AddDevice<Mouse>();
                keyboard = InputSystem.AddDevice<Keyboard>();
                movement.GetType().GetMethod("SetCursorCaptured", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(movement, new object[] { true });
                InputSystem.QueueStateEvent(mouse, new MouseState());
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                yield return s_settle;

                var arms = player.GetComponentInChildren<CombatArmsView>();
                Assert.IsNotNull(arms.RightFist.GetComponent<Renderer>());
                double pressedAt = Time.realtimeSinceStartupAsDouble;
                InputSystem.QueueStateEvent(mouse, new MouseState().WithButton(MouseButton.Left));
                yield return null;
                yield return null;
                Assert.IsTrue(arms.IsPulledBack);
                double latency = (arms.PullbackShownAt - pressedAt) * 1000d;
                Debug.Log($"[Step2 scene latency] queued press to visible fist pose: {latency:F2} ms");
                Assert.That(latency, Is.InRange(0d, 99.999d));
                InputSystem.QueueStateEvent(mouse, new MouseState());
                yield return s_attack;
                Assert.AreEqual(90f, dummy.Health.Current);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
                yield return s_settle;
                Assert.AreEqual(1, actor.Actions.SwingNumber, "E still does not attack in the real scene.");

                InputSystem.QueueStateEvent(mouse, new MouseState().WithButton(MouseButton.Right));
                yield return s_settle;
                Assert.AreEqual(FighterState.Blocking, actor.State.State);
                InputSystem.QueueStateEvent(mouse, new MouseState());
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                yield return s_settle;
                float beforeShove = dummy.transform.position.z;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.F));
                yield return s_settle;
                Assert.Greater(dummy.transform.position.z, beforeShove);
                Assert.AreEqual(actor.Id, dummy.GetComponent<SandboxShoveReceiver>().LastAttacker);
                Assert.AreEqual(90f, dummy.Health.Current);
                Assert.Less(dummy.GetComponent<Collider>().bounds.max.z, 2.51f,
                    "Shove must not put the dummy through the cell wall.");
            }
            finally
            {
                if (mouse != null)
                {
                    InputSystem.RemoveDevice(mouse);
                }
                if (keyboard != null)
                {
                    InputSystem.RemoveDevice(keyboard);
                }
                Application.runInBackground = background;
                InputSystem.settings.editorInputBehaviorInPlayMode = behavior;
                Cursor.lockState = CursorLockMode.None;
            }
            // Remove the populated scene before the isolated physics tests share this test run.
            Scene empty = SceneManager.CreateScene("AfterCombatSandboxTest");
            Scene populated = SceneManager.GetSceneByName("CombatAttacksSandbox");
            SceneManager.SetActiveScene(empty);
            yield return SceneManager.UnloadSceneAsync(populated);
        }
    }
}
#endif
