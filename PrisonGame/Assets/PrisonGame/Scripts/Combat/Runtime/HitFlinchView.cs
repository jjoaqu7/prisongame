using UnityEngine;

namespace PrisonGame.Combat
{
    // Placeholder flinch: tilts a visual child away from each landed punch, holds the pose during
    // hitstop, then springs back. Presentation only: it never touches the fighter's collider,
    // position or rules state. Replaced by an animated hit reaction when the inmate rig arrives.
    [RequireComponent(typeof(Fighter))]
    public sealed class HitFlinchView : MonoBehaviour
    {
        public const float RecoverSeconds = 0.25f;
        public const float PunchDegrees = 6f;
        public const float KickDegrees = 12f;
        [SerializeField] private Transform _visual;
        private Fighter _fighter;
        private bool _subscribed;
        private Vector3 _axis = Vector3.right;
        private float _peakDegrees;
        private float _recovered = 1f;

        public float CurrentDegrees { get; private set; }

        public void Configure(Transform visual) => _visual = visual;

        private void Awake() => _fighter = GetComponent<Fighter>();
        private void OnEnable() => Subscribe();
        private void Start() => Subscribe();

        private void Subscribe()
        {
            if (_subscribed || _fighter == null || _fighter.Health == null)
            {
                return;
            }
            _fighter.Health.Damaged += OnDamaged;
            _subscribed = true;
        }

        private void OnDisable()
        {
            if (_subscribed)
            {
                _fighter.Health.Damaged -= OnDamaged;
                _subscribed = false;
            }
        }

        private void OnDamaged(DamageInfo hit, DamageResult result)
        {
            Vector3 away = Vector3.ProjectOnPlane(hit.Direction, Vector3.up);
            if (result.Ignored || away.sqrMagnitude < 0.0001f)
            {
                return;
            }
            // Lean away and to a random side. A purely backward tilt points along the attacker's view
            // and is nearly invisible in first person; the side component makes it read from the front.
            // Local axis, so the tilt stays correct however the fighter is turned.
            Vector3 backward = Vector3.Cross(Vector3.up, away.normalized);
            float side = Random.value < 0.5f ? -0.6f : 0.6f;
            _axis = transform.InverseTransformDirection((backward + away.normalized * side).normalized);
            _peakDegrees = hit.Kind == AttackKind.Kick ? KickDegrees : PunchDegrees;
            _recovered = 0f;
        }

        private void LateUpdate()
        {
            if (_visual == null)
            {
                return;
            }
            if (!_fighter.HitStop.IsActive)
            {
                _recovered = Mathf.Min(1f, _recovered + Time.deltaTime / RecoverSeconds);
            }
            // Ease out: fast recoil, gentle settle.
            float remaining = 1f - _recovered;
            CurrentDegrees = _peakDegrees * remaining * remaining;
            _visual.localRotation = Quaternion.AngleAxis(CurrentDegrees, _axis);
        }
    }
}
