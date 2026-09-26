using PrisonGame.Combat;
using UnityEngine;

namespace PrisonGame.PlayerCombat
{
    // Placeholder poses read the rules; moving the arms never deals damage.
    public sealed class CombatArmsView : MonoBehaviour
    {
        [SerializeField] private Fighter _fighter;
        [SerializeField] private Transform _leftFist;
        [SerializeField] private Transform _rightFist;
        private GUIStyle _style;
        private bool _wasPulledBack;

        public double PullbackShownAt { get; private set; }
        public bool IsPulledBack { get; private set; }
        public Transform RightFist => _rightFist;

        public void Configure(Fighter fighter, Transform leftFist, Transform rightFist)
        {
            _fighter = fighter;
            _leftFist = leftFist;
            _rightFist = rightFist;
        }

        private void LateUpdate()
        {
            if (_fighter == null || _fighter.State == null || _leftFist == null || _rightFist == null)
            {
                return;
            }
            FighterState state = _fighter.State.State;
            IsPulledBack = state == FighterState.Charging || state == FighterState.WindUp;
            if (IsPulledBack && !_wasPulledBack)
            {
                PullbackShownAt = Time.realtimeSinceStartupAsDouble;
            }
            _wasPulledBack = IsPulledBack;
            Vector3 left = new Vector3(-0.24f, -0.29f, 0.47f);
            Vector3 right = new Vector3(0.24f, -0.29f, 0.47f);
            if (IsPulledBack)
            {
                right += new Vector3(0.035f, 0.03f, -0.13f);
            }
            else if (state == FighterState.Blocking)
            {
                left = new Vector3(-0.14f, -0.12f, 0.4f);
                right = new Vector3(0.14f, -0.12f, 0.4f);
            }
            else if (state == FighterState.Active || state == FighterState.Recovery)
            {
                float extension = state == FighterState.Active ? 1f :
                    1f - Mathf.Clamp01(_fighter.State.TimeInState / _fighter.State.CurrentAttack.Recovery);
                right = Vector3.Lerp(right, new Vector3(0.035f, -0.13f, 0.86f), extension);
                if (_fighter.Actions.CurrentAttack.Kind == AttackKind.Shove)
                {
                    left = Vector3.Lerp(left, new Vector3(-0.12f, -0.12f, 0.82f), extension);
                }
            }
            _leftFist.localPosition = left;
            _rightFist.localPosition = right;
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
            GUI.Label(new Rect(22f, top + 5f, 600f, 24f),
                "LMB: tap / hold then release punch    RMB: hold block    F: shove", _style);
            GUI.Label(new Rect(22f, top + 31f, 600f, 24f),
                $"Phase: {_fighter.State.State}    Charge: {_fighter.Actions.ChargeSeconds / 0.8f:P0}", _style);
            GUI.Label(new Rect(22f, top + 57f, 600f, 24f),
                "Sandbox: dummy does not fight back yet. E remains interaction.", _style);
        }
    }
}
