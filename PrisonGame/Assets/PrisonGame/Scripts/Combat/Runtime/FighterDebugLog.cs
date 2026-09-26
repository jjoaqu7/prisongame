using System.Collections.Generic;
using PrisonGame.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace PrisonGame.Combat
{
    // Development readout: writes a fighter's hits and state changes to the console
    // and keeps the latest lines for on-screen display and automated checks.
    // Presentation only; it never changes rules state.
    [RequireComponent(typeof(Fighter))]
    public sealed class FighterDebugLog : MonoBehaviour
    {
        private const int Capacity = 8;

        [SerializeField]
        [FormerlySerializedAs("logToConsole")]
        private bool _logToConsole = true;

        [SerializeField]
        [FormerlySerializedAs("showOnScreen")]
        private bool _showOnScreen = true;

        private readonly List<string> _lines = new List<string>();
        private Fighter _fighter;
        private GUIStyle _style;

        public IReadOnlyList<string> Lines => _lines;

        private void Start()
        {
            _fighter = GetComponent<Fighter>();
            _fighter.Health.Damaged += OnDamaged;
            _fighter.Health.WentDown += OnWentDown;
            _fighter.Health.Killed += OnKilled;
            _fighter.State.Changed += OnStateChanged;
        }

        private void OnDestroy()
        {
            if (_fighter == null || _fighter.Health == null)
            {
                return;
            }
            _fighter.Health.Damaged -= OnDamaged;
            _fighter.Health.WentDown -= OnWentDown;
            _fighter.Health.Killed -= OnKilled;
            _fighter.State.Changed -= OnStateChanged;
        }

        private void OnDamaged(DamageInfo hit, DamageResult result) =>
            Add($"-{result.Applied:0.#} {hit.Kind} from {hit.Attacker} ({_fighter.Health.Current:0.#}/{_fighter.Health.Max:0})");

        private void OnWentDown(DamageInfo hit) => Add($"down (last hit by {hit.Attacker})");

        private void OnKilled(StableId killer) => Add($"killed by {killer}");

        private void OnStateChanged(FighterState from, FighterState to) => Add($"{from} -> {to}");

        private void Add(string text)
        {
            string line = $"{name}: {text}";
            if (_lines.Count == Capacity)
            {
                _lines.RemoveAt(0);
            }
            _lines.Add(line);
            if (_logToConsole)
            {
                Debug.Log("[Combat] " + line, this);
            }
        }

        private void OnGUI()
        {
            if (!_showOnScreen || _lines.Count == 0)
            {
                return;
            }
            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label) { fontSize = 14 };
            }
            GUI.Box(new Rect(8, 8, 420, 12 + _lines.Count * 20), GUIContent.none);
            for (int i = 0; i < _lines.Count; i++)
            {
                GUI.Label(new Rect(16, 12 + i * 20, 410, 20), _lines[i], _style);
            }
        }
    }
}
