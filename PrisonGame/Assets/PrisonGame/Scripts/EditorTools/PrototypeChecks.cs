using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PrisonGame.EditorTools
{
    // Runs the prototype's runtime checks, which were written for Development builds, inside Editor
    // Play mode. In the Editor, keyboards reach the game only while the Game view has focus, and these
    // checks simulate key presses, so Start routes all input to the game until RestoreInput is called.
    // Results are written by each check, for example PrisonGame/LaundryCheck/results.txt.
    public static class PrototypeChecks
    {
        private const string SavedBehaviorKey = "PrisonGame.EditorTools.InputBehavior";

        // checkTypeName is a class in PrisonGame.Prototype, for example "LaundryStandaloneCheck"
        // (in Duty01_Laundry) or "StandaloneSmokeCheck" (in Room01_Blockout).
        public static string Start(string checkTypeName)
        {
            if (!EditorApplication.isPlaying)
            {
                return "Refused: enter Play mode in the check's scene first.";
            }
            // The prototype lives in Assembly-CSharp, which Editor assemblies cannot reference directly.
            Type checkType = Type.GetType("PrisonGame.Prototype." + checkTypeName + ", Assembly-CSharp");
            if (checkType == null || !typeof(MonoBehaviour).IsAssignableFrom(checkType))
            {
                return "Refused: no prototype check named " + checkTypeName + ".";
            }
            InputSettings settings = InputSystem.settings;
            if (!SessionState.GetBool(SavedBehaviorKey + ".Set", false))
            {
                SessionState.SetString(SavedBehaviorKey, settings.editorInputBehaviorInPlayMode.ToString());
                SessionState.SetBool(SavedBehaviorKey + ".Set", true);
            }
            settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            Application.runInBackground = true;
            new GameObject(checkTypeName).AddComponent(checkType);
            return "Started " + checkTypeName + "; input routed to the game. Call RestoreInput when it finishes.";
        }

        public static string RestoreInput()
        {
            if (!SessionState.GetBool(SavedBehaviorKey + ".Set", false))
            {
                return "Nothing to restore.";
            }
            string saved = SessionState.GetString(SavedBehaviorKey, string.Empty);
            InputSystem.settings.editorInputBehaviorInPlayMode =
                (InputSettings.EditorInputBehaviorInPlayMode)Enum.Parse(typeof(InputSettings.EditorInputBehaviorInPlayMode), saved);
            Application.runInBackground = PlayerSettings.runInBackground;
            SessionState.EraseString(SavedBehaviorKey);
            SessionState.EraseBool(SavedBehaviorKey + ".Set");
            return "Restored input behavior " + saved + ".";
        }
    }
}
