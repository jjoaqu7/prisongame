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
                dummy.gameObject.AddComponent<SandboxShoveReceiver>();

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
