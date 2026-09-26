using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;

namespace PrisonGame.EditorTools
{
    // Keeps asset names free of spaces, which Unity's command-line tools handle badly
    // (docs/engineering-standards.md). Renames go through the AssetDatabase, so each asset
    // keeps its GUID and every scene or prefab reference to it survives.
    public static class AssetNaming
    {
        // Renames every asset under rootFolder whose file name contains spaces, joining the
        // words in PascalCase: "Dex burgundy.mat" becomes "DexBurgundy.mat". Returns one line
        // per rename or failure. Refuses to run while the Editor is busy or a scene is unsaved.
        public static string RemoveSpaces(string rootFolder)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            {
                return "Refused: the Editor is playing or compiling.";
            }
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
                {
                    return "Refused: save or discard scene changes first.";
                }
            }
            if (!AssetDatabase.IsValidFolder(rootFolder))
            {
                return "Refused: " + rootFolder + " is not a folder in the project.";
            }

            var planned = new List<(string path, string newName)>();
            foreach (string guid in AssetDatabase.FindAssets(string.Empty, new[] { rootFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string name = Path.GetFileNameWithoutExtension(path);
                if (name.Contains(" ") && !AssetDatabase.IsValidFolder(path))
                {
                    planned.Add((path, ToPascalCase(name)));
                }
            }

            var report = new List<string>();
            foreach ((string path, string newName) in planned)
            {
                string error = AssetDatabase.RenameAsset(path, newName);
                report.Add(string.IsNullOrEmpty(error)
                    ? path + " -> " + newName + Path.GetExtension(path)
                    : "FAILED " + path + ": " + error);
            }
            AssetDatabase.SaveAssets();
            return report.Count == 0 ? "No asset names with spaces under " + rootFolder + "." : string.Join("\n", report);
        }

        public static string ToPascalCase(string words)
        {
            var parts = words.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = char.ToUpper(parts[i][0], CultureInfo.InvariantCulture) + parts[i].Substring(1);
            }
            return string.Concat(parts);
        }
    }
}
