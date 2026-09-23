#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace PrisonGame.Prototype
{
    // Opt-in checks inside the actual executable. No object is created during ordinary play.
    // Excluded from non-development Players. Results and capture stay beside the test build.
    public sealed class StandaloneSmokeCheck : MonoBehaviour
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private readonly List<string> results = new List<string>();
        private readonly List<string> errors = new List<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BeginIfRequested()
        {
            if (Application.isEditor || Array.IndexOf(Environment.GetCommandLineArgs(), "-prototype-smoke-test") < 0) return;
            new GameObject("Standalone smoke check").AddComponent<StandaloneSmokeCheck>();
        }

        private void Awake()
        {
            Application.runInBackground = true;
            Application.logMessageReceived += RecordError;
        }

        private void OnDestroy() { Application.logMessageReceived -= RecordError; }

        private void RecordError(string message, string stack, LogType kind)
        {
            if (kind == LogType.Error || kind == LogType.Exception || kind == LogType.Assert)
                errors.Add(message + "\n" + stack);
        }

        private IEnumerator Start()
        {
            string directory = Path.Combine(Path.GetDirectoryName(Application.dataPath), "SmokeCheck");
            Directory.CreateDirectory(directory);
            yield return new WaitForSecondsRealtime(1);
            yield return new WaitForEndOfFrame();
            try
            {
                // A hidden automated Player has no composited window backbuffer.
                // Render the real scene camera explicitly; this verifies 3D output, not UI.
                CaptureCamera(Path.Combine(directory, "startup.png"));
                RunChecks();
            }
            catch (Exception exception) { errors.Add(exception.ToString()); }

            // Allow deferred Unity errors to arrive before reporting success.
            yield return null;
            bool passed = errors.Count == 0;
            string report = (passed ? "PASS" : "FAIL") + " - standalone Windows smoke check\n"
                + "Unity: " + Application.unityVersion + "\nScene: " + SceneManager.GetActiveScene().name
                + "\nGraphics: " + SystemInfo.graphicsDeviceType + "\n"
                + string.Join("\n", results) + "\n" + string.Join("\n", errors);
            File.WriteAllText(Path.Combine(directory, "results.txt"), report);
            Debug.Log(report);
            Application.Quit(passed ? 0 : 1);
        }

        private void Check(bool condition, string label)
        {
            if (!condition) throw new InvalidOperationException("FAIL: " + label);
            results.Add("PASS: " + label);
        }

        private static void CaptureCamera(string path)
        {
            var camera = Camera.main;
            var target = RenderTexture.GetTemporary(1280, 720, 24, RenderTextureFormat.ARGB32);
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var texture = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                RenderTexture.ReleaseTemporary(target);
                Destroy(texture);
            }
        }

        private static object Invoke(object target, string method, params object[] arguments)
        {
            return target.GetType().GetMethod(method, Private).Invoke(target, arguments);
        }

        private void RunChecks()
        {
            Check(SceneManager.GetActiveScene().name == "Room01_Blockout", "Correct startup scene");
            var player = GameObject.Find("Player");
            var movement = player.GetComponent<FirstPersonController>();
            var interaction = player.GetComponent<PlayerInteraction>();
            var cc = player.GetComponent<CharacterController>();
            var camera = player.GetComponentInChildren<Camera>();
            var door = FindFirstObjectByType<PrototypeDoor>();
            var parcel = FindFirstObjectByType<PrototypePickup>();
            var inmate = FindFirstObjectByType<PrototypeInmate>();
            Check(movement != null && interaction != null && door != null && parcel != null && inmate != null,
                "Player and interactive objects included in build");
            Check(!movement.ControlsActive, "Startup releases cursor and presents settings");
            int materialCount = 0;
            foreach (Renderer renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material == null || material.shader == null || !material.shader.isSupported)
                        throw new InvalidOperationException("Unsupported material: " + renderer.name);
                    materialCount++;
                }
            Check(materialCount > 0, "All " + materialCount + " rendered materials supported");

            movement.enabled = false;
            interaction.enabled = false;
            door.enabled = false;
            var settings = InputSystem.settings;
            var previousBackground = settings.backgroundBehavior;
            Keyboard keyboard = null;
            void Place(Vector3 position, float yaw)
            {
                cc.enabled = false;
                player.transform.SetPositionAndRotation(position, Quaternion.Euler(0, yaw, 0));
                cc.enabled = true;
                camera.transform.localRotation = Quaternion.identity;
                Physics.SyncTransforms();
            }
            void Aim(Vector3 position)
            {
                camera.transform.rotation = Quaternion.LookRotation(position - camera.transform.position);
                Physics.SyncTransforms();
                Invoke(interaction, "FindTarget");
            }
            void Walk(int frames)
            {
                for (int frame = 0; frame < frames; frame++) Invoke(movement, "StepMovement", Vector2.up, 1f / 60);
            }
            void Press(Key key)
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                InputSystem.Update();
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
                InputSystem.Update();
                Invoke(interaction, "Update");
            }
            try
            {
                settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                keyboard = InputSystem.AddDevice<Keyboard>();
                Place(new Vector3(-3.4f, .05f, -.5f), 90);
                Aim(door.transform.position + Vector3.up * .4f);
                Check(interaction.Target == door, "Door targeting");
                Walk(90);
                Check(player.transform.position.x < -2.3f, "Closed door blocks walking");
                Place(new Vector3(-3.4f, .05f, -.5f), 90);
                Aim(door.transform.position + Vector3.up * .4f);
                Invoke(movement, "SetCursorCaptured", true);
                Press(Key.E);
                // Hidden Players can render extremely fast. Allow animation time, not a fixed tick count.
                Check(Time.deltaTime > 0, "Animation clock advances");
                int doorFrames = Mathf.CeilToInt(2f / Time.deltaTime);
                for (int frame = 0; frame < doorFrames && !door.IsOpen; frame++) Invoke(door, "Update");
                Check(door.IsOpen, "E opens door");
                Physics.SyncTransforms(); // Manual ticks run before Unity's next normal physics sync.
                Walk(58);
                Check(player.transform.position.x > -.7f, "Open door allows walking into corridor");

                Place(new Vector3(-3.1f, .05f, .1f), 0);
                Aim(parcel.transform.position);
                Check(interaction.Target == parcel, "Parcel targeting");
                Invoke(movement, "SetCursorCaptured", false);
                Press(Key.E);
                Check(interaction.HeldItem == null, "Settings block E interaction");
                Invoke(movement, "SetCursorCaptured", true);
                Press(Key.E);
                Check(interaction.HeldItem == parcel && parcel.transform.IsChildOf(camera.transform), "E picks up parcel");
                Invoke(movement, "SetCursorCaptured", false);
                Press(Key.Q);
                Check(interaction.HeldItem == parcel, "Settings block Q placement");
                Invoke(movement, "SetCursorCaptured", true);
                Aim(new Vector3(-3.1f, .83f, 1.5f));
                Press(Key.Q);
                Check(interaction.HeldItem == null && !parcel.GetComponent<Rigidbody>().isKinematic,
                    "Q places parcel and restores physics");

                Place(new Vector3(.4f, .05f, 5.4f), 90);
                Aim(inmate.transform.position + Vector3.up * 1.2f);
                Check(interaction.Target == null, "Wall blocks interaction");
                Place(new Vector3(2.4f, .05f, 3.5f), 0);
                Aim(inmate.transform.position + Vector3.up * 1.2f);
                Check(interaction.Target == inmate, "Inmate targeting");
                Press(Key.E);
                var message = (string)typeof(PlayerInteraction).GetField("message", Private).GetValue(interaction);
                Check(message.StartsWith("Inmate:"), "E produces inmate response");
                Press(Key.Escape);
                Invoke(movement, "Update");
                Check(!movement.ControlsActive && Cursor.visible, "Escape releases controls");
            }
            finally
            {
                if (keyboard != null) InputSystem.RemoveDevice(keyboard);
                settings.backgroundBehavior = previousBackground;
                Invoke(movement, "SetCursorCaptured", false);
            }
        }
    }
}
#endif
