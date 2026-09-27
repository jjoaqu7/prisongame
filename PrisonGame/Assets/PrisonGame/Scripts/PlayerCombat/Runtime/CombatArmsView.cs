using PrisonGame.Combat;
using UnityEngine;

namespace PrisonGame.PlayerCombat
{
    // Placeholder poses read the rules; moving the arms or leg never deals damage.
    public sealed class CombatArmsView : MonoBehaviour
    {
        [SerializeField] private Fighter _fighter;
        [SerializeField] private Transform _leftFist;
        [SerializeField] private Transform _rightFist;
        [SerializeField] private Transform _leg;
        private GUIStyle _style;
        private bool _wasPulledBack;

        private static readonly Vector3 s_legHidden = new Vector3(0.08f, -0.95f, 0.3f);
        private static readonly Vector3 s_legRaised = new Vector3(0.08f, -0.62f, 0.42f);
        private static readonly Vector3 s_legExtended = new Vector3(0.05f, -0.42f, 1.05f);

        public double PullbackShownAt { get; private set; }
        public bool IsPulledBack { get; private set; }
        public Transform RightFist => _rightFist;
        public Transform Leg => _leg;

        public void Configure(Fighter fighter, Transform leftFist, Transform rightFist, Transform leg = null)
        {
            _fighter = fighter;
            _leftFist = leftFist;
            _rightFist = rightFist;
            _leg = leg;
        }

        private void LateUpdate()
        {
            if (_fighter == null || _fighter.State == null || _leftFist == null || _rightFist == null)
            {
                return;
            }
            FighterState state = _fighter.State.State;
            AttackKind kind = _fighter.Actions.CurrentAttack.Kind;
            bool attacking = state == FighterState.WindUp || state == FighterState.Active || state == FighterState.Recovery;
            // The punch's wind-up is the visible pull-back, shown the frame the button is pressed.
            IsPulledBack = state == FighterState.WindUp && kind == AttackKind.Punch;
            if (IsPulledBack && !_wasPulledBack)
            {
                PullbackShownAt = Time.realtimeSinceStartupAsDouble;
            }
            _wasPulledBack = IsPulledBack;

            Vector3 left = new Vector3(-0.24f, -0.29f, 0.47f);
            Vector3 right = new Vector3(0.24f, -0.29f, 0.47f);
            Vector3 leg = s_legHidden;
            if (state == FighterState.Blocking)
            {
                left = new Vector3(-0.14f, -0.12f, 0.4f);
                right = new Vector3(0.14f, -0.12f, 0.4f);
            }
            else if (attacking && kind == AttackKind.Kick)
            {
                // Fists come up to guard while the leg rises into view, then drives forward.
                left = new Vector3(-0.2f, -0.2f, 0.42f);
                right = new Vector3(0.2f, -0.2f, 0.42f);
                leg = state == FighterState.WindUp
                    ? Vector3.Lerp(s_legHidden, s_legRaised, Mathf.Clamp01(_fighter.State.TimeInState / _fighter.State.CurrentAttack.WindUp))
                    : state == FighterState.Active ? s_legExtended
                    : Vector3.Lerp(s_legExtended, s_legHidden, Mathf.Clamp01(_fighter.State.TimeInState / _fighter.State.CurrentAttack.Recovery));
            }
            else if (IsPulledBack)
            {
                right += new Vector3(0.035f, 0.03f, -0.13f);
            }
            else if (state == FighterState.Active || state == FighterState.Recovery)
            {
                float extension = state == FighterState.Active ? 1f :
                    1f - Mathf.Clamp01(_fighter.State.TimeInState / _fighter.State.CurrentAttack.Recovery);
                right = Vector3.Lerp(right, new Vector3(0.035f, -0.13f, 0.86f), extension);
                if (kind == AttackKind.Shove)
                {
                    left = Vector3.Lerp(left, new Vector3(-0.12f, -0.12f, 0.82f), extension);
                }
            }
            _leftFist.localPosition = left;
            _rightFist.localPosition = right;
            if (_leg != null)
            {
                _leg.localPosition = leg;
            }
        }

        private void OnGUI()
        {
            if (_fighter == null || _fighter.State == null)
            {
                return;
            }
            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label) { fontSize = 16 };
            }
            float top = Screen.height - 110f;
            GUI.Box(new Rect(12f, top, 620f, 90f), GUIContent.none);
            GUI.Label(new Rect(22f, top + 5f, 600f, 24f), "LMB: punch    V: kick    RMB: hold block    F: shove", _style);
            GUI.Label(new Rect(22f, top + 31f, 600f, 24f), $"Phase: {_fighter.State.State}", _style);
            GUI.Label(new Rect(22f, top + 57f, 600f, 24f),
                "He fights back. Raise the block just before his punch lands to stagger him.", _style);
        }
    }
}
