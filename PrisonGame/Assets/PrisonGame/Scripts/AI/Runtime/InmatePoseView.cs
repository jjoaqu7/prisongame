using PrisonGame.Combat;
using UnityEngine;

namespace PrisonGame.AI
{
    // Placeholder body language for the capsule stand-in until the M5 inmate and his fight clips arrive
    // (steps 6 and 7). The fist pulls back and up through the wind-up so the punch can be seen coming,
    // then drives out; he raises a guard, wobbles when staggered, hunches when hurt and lies on his side
    // when downed. Presentation only: it reads the rules and never changes them or the hit collider.
    [RequireComponent(typeof(Fighter))]
    public sealed class InmatePoseView : MonoBehaviour
    {
        public const float WindUpLeanDegrees = -10f;
        public const float PunchLeanDegrees = 8f;
        public const float HurtLeanDegrees = 14f;
        public const float StaggerLeanDegrees = -14f;

        // Local positions under the scaled capsule (0.8 x 0.9 x 0.8), chosen to read from the player's view.
        private static readonly Vector3 s_fistRest = new Vector3(0.31f, 0.5f, 0.56f);
        private static readonly Vector3 s_fistPulled = new Vector3(0.44f, 0.83f, -0.12f);
        private static readonly Vector3 s_fistOut = new Vector3(0.12f, 0.67f, 1.19f);
        private static readonly Vector3 s_fistGuard = new Vector3(0.12f, 0.72f, 0.44f);
        private static readonly Vector3 s_hurtDrop = new Vector3(0f, -0.06f, 0f);
        private static readonly Vector3 s_lyingDrop = new Vector3(0f, -0.5f, 0f);
        [SerializeField] private Transform _pose;
        [SerializeField] private Transform _fist;
        private Fighter _fighter;

        public float LeanDegrees { get; private set; }
        public bool IsLyingDown { get; private set; }
        public Transform Fist => _fist;

        public void Configure(Transform pose, Transform fist)
        {
            _pose = pose;
            _fist = fist;
        }

        private void Awake() => _fighter = GetComponent<Fighter>();

        private void LateUpdate()
        {
            if (_pose == null || _fighter.State == null)
            {
                return;
            }
            FighterStateMachine state = _fighter.State;
            float time = state.TimeInState;
            AttackTiming attack = state.CurrentAttack;
            bool hurt = _fighter.IsHurt;
            float lean = hurt ? HurtLeanDegrees : 0f;
            float wobble = 0f;
            Vector3 fist = s_fistRest;
            switch (state.State)
            {
                case FighterState.WindUp:
                    float pulled = Mathf.SmoothStep(0f, 1f, Fraction(time, attack.WindUp));
                    fist = Vector3.Lerp(s_fistRest, s_fistPulled, pulled);
                    lean += WindUpLeanDegrees * pulled;
                    break;
                case FighterState.Active:
                    fist = s_fistOut;
                    lean += PunchLeanDegrees;
                    break;
                case FighterState.Recovery:
                    float back = Fraction(time, attack.Recovery);
                    fist = Vector3.Lerp(s_fistOut, s_fistRest, back);
                    lean += PunchLeanDegrees * (1f - back);
                    break;
                case FighterState.Blocking:
                    fist = s_fistGuard;
                    break;
                case FighterState.Staggered:
                    lean += StaggerLeanDegrees;
                    wobble = Mathf.Sin(time * 25f) * 6f;
                    break;
            }
            IsLyingDown = state.State == FighterState.Downed || state.State == FighterState.Dead;
            if (IsLyingDown)
            {
                LeanDegrees = 0f;
                _pose.SetLocalPositionAndRotation(s_lyingDrop, Quaternion.Euler(0f, 0f, 90f));
            }
            else
            {
                LeanDegrees = lean;
                _pose.SetLocalPositionAndRotation(hurt ? s_hurtDrop : Vector3.zero, Quaternion.Euler(lean, 0f, wobble));
            }
            if (_fist != null)
            {
                _fist.localPosition = fist;
            }
        }

        private static float Fraction(float time, float duration) => duration > 0f ? Mathf.Clamp01(time / duration) : 1f;
    }
}
