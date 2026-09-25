using UnityEditor;
using UnityEngine;

namespace PrisonGame.EditorTools
{
    public sealed class M5RigPreviewWindow : EditorWindow
    {
        private PrisonGame.Prototype.InmateMotion lastMotion;
        private Vector3 walkStart;
        [MenuItem("Prison Game/Art/M5 Rig Preview")]
        private static void Open() => GetWindow<M5RigPreviewWindow>("M5 Rig Preview");

        private void OnInspectorUpdate() => Repaint();

        private void OnGUI()
        {
            EditorGUILayout.LabelField("M5 movement study", EditorStyles.boldLabel);
            var motion = FindFirstObjectByType<PrisonGame.Prototype.InmateMotion>();
            if (motion != null)
            {
                if (motion != lastMotion) { lastMotion = motion; walkStart = motion.transform.position; }
                EditorGUILayout.HelpBox("Behavior study: press E near M5 to talk. These buttons test actual walking; obstacles stop movement. The inmate pauses while talking and then resumes the requested walk.", MessageType.Info);
                using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
                {
                    if (GUILayout.Button("Walk 1.2 m into common area")) motion.SetDestination(walkStart + Vector3.back * 1.2f);
                    if (GUILayout.Button("Walk back to start")) motion.SetDestination(walkStart);
                    if (GUILayout.Button("Stop walking")) motion.Stop();
                }
                EditorGUILayout.LabelField("Walk pace", PrisonGame.Prototype.InmateMotion.WalkSpeed.ToString("F2") + " m/s");
                EditorGUILayout.LabelField("State", motion.IsTalking ? "Talking" : motion.IsBlocked ? "Blocked" : motion.HasDestination ? "Walking / turning" : "Idle");
                return;
            }
            EditorGUILayout.HelpBox("Open Art03_M5_Rig and enter Play mode for isolated clips, or Art03_M5_Behavior for walking and talking together.", MessageType.Info);
            EditorGUILayout.LabelField("Original walk clip: 0.43 m/s at normal playback speed.", EditorStyles.wordWrappedLabel);
            Animator animator = null;
            foreach (var candidate in FindObjectsByType<Animator>(FindObjectsSortMode.None))
            {
                if (candidate.runtimeAnimatorController != null &&
                    AssetDatabase.GetAssetPath(candidate.runtimeAnimatorController) ==
                    "Assets/Prototype/Art03Rig/M5_Study.controller")
                {
                    animator = candidate;
                    break;
                }
            }
            using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying || animator == null))
            {
                if (GUILayout.Button("Idle")) animator.SetInteger("Preview", 0);
                if (GUILayout.Button("Walk preview")) animator.SetInteger("Preview", 1);
                if (GUILayout.Button("Gesture preview")) animator.SetInteger("Preview", 2);
            }
            if (animator != null && EditorApplication.isPlaying)
                EditorGUILayout.LabelField("Preview selection", animator.GetInteger("Preview").ToString());
        }
    }
}
