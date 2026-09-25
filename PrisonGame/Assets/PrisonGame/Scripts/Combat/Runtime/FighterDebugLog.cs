using System.Collections.Generic;
using UnityEngine;

namespace PrisonGame.Combat
{
    // Development readout: writes a fighter's hits and state changes to the console
    // and keeps the latest lines for on-screen display and automated checks.
    // Presentation only; it never changes rules state.
    [RequireComponent(typeof(Fighter))]
    public sealed class FighterDebugLog : MonoBehaviour
    {
        private const int Capacity = 8;
        [SerializeField] private bool logToConsole = true;
        [SerializeField] private bool showOnScreen = true;
        private readonly List<string> lines = new List<string>();
        private Fighter fighter;
        private GUIStyle style;

        public IReadOnlyList<string> Lines => lines;

        private void Start()
        {
            fighter = GetComponent<Fighter>();
            fighter.Health.Damaged += OnDamaged;
            fighter.Health.WentDown += OnWentDown;
            fighter.Health.Killed += OnKilled;
            fighter.State.Changed += OnStateChanged;
        }

        private void OnDestroy()
        {
            if (fighter == null || fighter.Health == null) return;
            fighter.Health.Damaged -= OnDamaged;
            fighter.Health.WentDown -= OnWentDown;
            fighter.Health.Killed -= OnKilled;
            fighter.State.Changed -= OnStateChanged;
        }

        private void OnDamaged(DamageInfo hit, DamageResult result) =>
            Add($"-{result.Applied:0.#} {hit.Kind} from {hit.Attacker} ({fighter.Health.Current:0.#}/{fighter.Health.Max:0})");
        private void OnWentDown(DamageInfo hit) => Add($"down (last hit by {hit.Attacker})");
        private void OnKilled(PrisonGame.Core.StableId killer) => Add($"killed by {killer}");
        private void OnStateChanged(FighterState from, FighterState to) => Add($"{from} -> {to}");

        private void Add(string text)
        {
            string line = $"{name}: {text}";
            if (lines.Count == Capacity) lines.RemoveAt(0);
            lines.Add(line);
            if (logToConsole) Debug.Log("[Combat] " + line, this);
        }

        private void OnGUI()
        {
            if (!showOnScreen || lines.Count == 0) return;
            if (style == null) style = new GUIStyle(GUI.skin.label) { fontSize = 14 };
            GUI.Box(new Rect(8, 8, 420, 12 + lines.Count * 20), GUIContent.none);
            for (int i = 0; i < lines.Count; i++) GUI.Label(new Rect(16, 12 + i * 20, 410, 20), lines[i], style);
        }
    }
}
