using System;
using System.IO;
using PrisonGame.Combat;
using PrisonGame.PlayerCombat;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PrisonGame.EditorTools
{
    // Builds a new step-2 scene from the verified greybox, preserving the original scene and its checks.
    public static class CombatSandboxBuilder
    {
        public const string ScenePath = "Assets/PrisonGame/Scenes/Sandbox/CombatAttacksSandbox.unity";

        [MenuItem("PrisonGame/Build Combat Attacks Sandbox")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            {
                throw new InvalidOperationException("Stop Play mode and compilation before building.");
            }
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).isDirty)
                {
                    throw new InvalidOperationException("Save or discard scene changes first.");
                }
            }
            if (File.Exists(ScenePath))
            {
                throw new InvalidOperationException("The sandbox already exists; refusing to overwrite it.");
            }
            SceneSetup[] previous = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                Scene scene = EditorSceneManager.OpenScene(
                    "Assets/PrisonGame/Scenes/Sandbox/CombatSandbox.unity", OpenSceneMode.Single);
                GameObject player = GameObject.Find("Player");
                Camera view = player.GetComponentInChildren<Camera>();
                Fighter dummy = UnityEngine.Object.FindFirstObjectByType<Fighter>();
                if (view == null || dummy == null)
                {
                    throw new InvalidOperationException("The source needs a player camera and dummy.");
                }
                Fighter actor = player.AddComponent<Fighter>();
                var serialized = new SerializedObject(actor);
                serialized.FindProperty("_stableId").stringValue = "sandbox-player";
                serialized.ApplyModifiedPropertiesWithoutUndo();
                player.AddComponent<LocalCombatInput>();
                player.AddComponent<MeleeSweep>().Aim = view.transform;
                dummy.gameObject.AddComponent<SandboxHitReceiver>();

                // Reuse a source material as a placeholder. No generated binary art or external assets.
                Material material = dummy.GetComponent<Renderer>().sharedMaterial;
                Transform left = MakeFist("LeftFist", view.transform, material, -0.24f);
                Transform right = MakeFist("RightFist", view.transform, material, 0.24f);
                view.gameObject.AddComponent<CombatArmsView>().Configure(actor, left, right);
                if (!EditorSceneManager.SaveScene(scene, ScenePath, true))
                {
                    throw new IOException("Could not save the attack sandbox.");
                }
                Directory.CreateDirectory("../docs/evidence");
                File.WriteAllText("../docs/evidence/encounter01-step2-scene-build.txt",
                    "Created " + ScenePath + " from CombatSandbox.\n" +
                    "Player: stable ID sandbox-player; local input, melee sweep, placeholder fists and phase readout.\n" +
                    "Dummy: opt-in shove response. Original scene preserved.\n");
            }
            finally
            {
                EditorSceneManager.RestoreSceneManagerSetup(previous);
            }
        }

        public const string CombatAudioFolder = "Assets/PrisonGame/Audio/Combat/";

        // ENCOUNTER-01 step 3: adds hit feedback to the existing attack sandbox. Additive only:
        // refuses to run twice, and saves only after every step has succeeded.
        [MenuItem("PrisonGame/Add Hit Feedback To Combat Attacks Sandbox")]
        public static void AddHitFeedback()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            {
                throw new InvalidOperationException("Stop Play mode and compilation before building.");
            }
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).isDirty)
                {
                    throw new InvalidOperationException("Save or discard scene changes first.");
                }
            }
            AudioClip[] punch = LoadClips("ImpactPunch_Medium_00", 5);
            AudioClip[] kick = LoadClips("ImpactPunch_Heavy_00", 5);
            AudioClip[] kickLayer = LoadClips("ImpactSoft_Heavy_00", 4);
            AudioClip[] shove = LoadClips("ImpactSoft_Medium_00", 4);
            SceneSetup[] previous = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                GameObject player = GameObject.Find("Player");
                Fighter actor = player == null ? null : player.GetComponent<Fighter>();
                MeleeSweep sweep = player == null ? null : player.GetComponent<MeleeSweep>();
                Camera view = player == null ? null : player.GetComponentInChildren<Camera>();
                Fighter dummy = null;
                foreach (Fighter fighter in UnityEngine.Object.FindObjectsByType<Fighter>(FindObjectsSortMode.None))
                {
                    if (fighter != actor)
                    {
                        dummy = fighter;
                    }
                }
                if (actor == null || sweep == null || view == null || dummy == null)
                {
                    throw new InvalidOperationException("The attack sandbox needs its player fighter, sweep, camera and dummy.");
                }
                if (dummy.GetComponent<HitFlinchView>() != null)
                {
                    throw new InvalidOperationException("Hit feedback is already in the sandbox; refusing to add it twice.");
                }

                // Move the dummy's look onto a child, so the flinch tilts only what is seen and the
                // collider that hit checks use stays upright.
                var visual = new GameObject("Visual");
                visual.transform.SetParent(dummy.transform, false);
                visual.AddComponent<MeshFilter>().sharedMesh = dummy.GetComponent<MeshFilter>().sharedMesh;
                MeshRenderer bodyRenderer = dummy.GetComponent<MeshRenderer>();
                visual.AddComponent<MeshRenderer>().sharedMaterials = bodyRenderer.sharedMaterials;
                bodyRenderer.enabled = false;
                Transform marker = dummy.transform.Find("Facing marker");
                if (marker != null)
                {
                    marker.SetParent(visual.transform, false);
                }
                dummy.gameObject.AddComponent<HitFlinchView>().Configure(visual.transform);

                player.AddComponent<FighterHitSounds>().Configure(punch, kick, kickLayer, shove);
                view.gameObject.AddComponent<CameraKick>().Configure(sweep);
                if (!EditorSceneManager.SaveScene(scene))
                {
                    throw new IOException("Could not save the attack sandbox.");
                }
                Directory.CreateDirectory("../docs/evidence");
                File.WriteAllText("../docs/evidence/encounter01-step3-scene-build.txt",
                    "Added hit feedback to " + ScenePath + ".\n" +
                    "Dummy: body visual moved to child 'Visual' (collider unchanged), HitFlinchView.\n" +
                    "Player: FighterHitSounds with " + punch.Length + " punch, " + kick.Length + " kick, " +
                    kickLayer.Length + " kick layer and " + shove.Length + " shove clips; CameraKick on the camera.\n");
            }
            finally
            {
                EditorSceneManager.RestoreSceneManagerSetup(previous);
            }
        }

        // Agreed September 27: the charged punch is replaced by a kick on V. Adds the placeholder leg
        // the arms view animates. Additive only: refuses to run twice.
        [MenuItem("PrisonGame/Add Kick Leg To Combat Attacks Sandbox")]
        public static void AddKickLeg()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            {
                throw new InvalidOperationException("Stop Play mode and compilation before building.");
            }
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).isDirty)
                {
                    throw new InvalidOperationException("Save or discard scene changes first.");
                }
            }
            SceneSetup[] previous = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                GameObject player = GameObject.Find("Player");
                Camera view = player == null ? null : player.GetComponentInChildren<Camera>();
                CombatArmsView arms = view == null ? null : view.GetComponent<CombatArmsView>();
                Transform left = view == null ? null : view.transform.Find("LeftFist");
                Transform right = view == null ? null : view.transform.Find("RightFist");
                if (arms == null || left == null || right == null)
                {
                    throw new InvalidOperationException("The attack sandbox needs its camera, arms view and both fists.");
                }
                if (view.transform.Find("KickLeg") != null)
                {
                    throw new InvalidOperationException("The kick leg is already in the sandbox; refusing to add it twice.");
                }
                GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                leg.name = "KickLeg";
                leg.transform.SetParent(view.transform, false);
                leg.transform.SetLocalPositionAndRotation(new Vector3(0.08f, -0.95f, 0.3f), Quaternion.Euler(70f, 0f, 0f));
                leg.transform.localScale = new Vector3(0.2f, 0.34f, 0.2f);
                UnityEngine.Object.DestroyImmediate(leg.GetComponent<Collider>());
                leg.GetComponent<Renderer>().sharedMaterial = right.GetComponent<Renderer>().sharedMaterial;
                arms.Configure(player.GetComponent<Fighter>(), left, right, leg.transform);
                if (!EditorSceneManager.SaveScene(scene))
                {
                    throw new IOException("Could not save the attack sandbox.");
                }
                File.AppendAllText("../docs/evidence/encounter01-step3-scene-build.txt",
                    "Kick (September 27): placeholder leg 'KickLeg' added under the camera and wired to the arms view.\n");
            }
            finally
            {
                EditorSceneManager.RestoreSceneManagerSetup(previous);
            }
        }

        private static AudioClip[] LoadClips(string prefix, int count)
        {
            var clips = new AudioClip[count];
            for (int i = 0; i < count; i++)
            {
                string path = CombatAudioFolder + prefix + i + ".ogg";
                clips[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clips[i] == null)
                {
                    throw new FileNotFoundException("Missing combat sound " + path);
                }
            }
            return clips;
        }

        private static Transform MakeFist(string name, Transform parent, Material material, float x)
        {
            GameObject fist = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            fist.name = name;
            fist.transform.SetParent(parent, false);
            fist.transform.SetLocalPositionAndRotation(new Vector3(x, -0.29f, 0.47f),
                Quaternion.Euler(90f, 0f, 0f));
            fist.transform.localScale = new Vector3(0.14f, 0.2f, 0.14f);
            UnityEngine.Object.DestroyImmediate(fist.GetComponent<Collider>());
            fist.GetComponent<Renderer>().sharedMaterial = material;
            return fist.transform;
        }
    }
}
