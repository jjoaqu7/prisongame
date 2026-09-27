using System;
using UnityEngine;

namespace PrisonGame.Combat
{
    // Physics adapter. The supplied aim belongs to this fighter; no camera or local input lookup.
    [RequireComponent(typeof(Fighter))]
    public sealed class MeleeSweep : MonoBehaviour
    {
        public const float Radius = 0.12f;
        [SerializeField] private Transform _aim;
        private Fighter _fighter;
        private Vector3 _previousOrigin;
        private Vector3 _previousDirection;
        private bool _hasPrevious;
        private bool _subscribed;
        private Collider[] _overlaps = new Collider[16];
        private RaycastHit[] _hits = new RaycastHit[16];

        public Transform Aim { get => _aim; set => _aim = value; }
        public event Action<Fighter, DamageInfo> HitResolved;

        private void Awake() => _fighter = GetComponent<Fighter>();
        private void OnEnable() => Subscribe();
        private void Start() => Subscribe();

        private void Subscribe()
        {
            if (_subscribed || _fighter == null || _fighter.Actions == null)
            {
                return;
            }
            _fighter.Actions.ActiveWindowOpened += OpenWindow;
            _fighter.State.Changed += OnStateChanged;
            _subscribed = true;
        }

        private void OnDisable()
        {
            if (_subscribed)
            {
                _fighter.Actions.ActiveWindowOpened -= OpenWindow;
                _fighter.State.Changed -= OnStateChanged;
                _subscribed = false;
            }
            _hasPrevious = false;
        }

        private void OpenWindow()
        {
            _hasPrevious = false;
            Sample();
        }

        private void OnStateChanged(FighterState from, FighterState to)
        {
            if (to != FighterState.Active)
            {
                _hasPrevious = false;
            }
        }

        private void LateUpdate()
        {
            if (_fighter.Actions.CanHit)
            {
                Sample();
            }
        }

        public void Sample()
        {
            if (_aim == null || !_fighter.Actions.CanHit)
            {
                return;
            }
            Physics.SyncTransforms();
            Vector3 origin = _aim.position;
            Vector3 direction = _aim.forward;
            if (_hasPrevious)
            {
                // Sample the volume between frames as well as the current view. Each ray is
                // independently occluded by scenery, so turning cannot sweep through a wall.
                int steps = Mathf.CeilToInt(Mathf.Max(
                    Vector3.Distance(_previousOrigin, origin) / 0.06f,
                    Vector3.Angle(_previousDirection, direction) / 3f));
                steps = Mathf.Clamp(steps, 1, 120);
                for (int i = 1; i < steps; i++)
                {
                    float fraction = (float)i / steps;
                    Cast(Vector3.Lerp(_previousOrigin, origin, fraction),
                        Vector3.Slerp(_previousDirection, direction, fraction).normalized);
                }
            }
            Cast(origin, direction);
            _previousOrigin = origin;
            _previousDirection = direction;
            _hasPrevious = true;
        }

        private void Cast(Vector3 origin, Vector3 direction)
        {
            // SphereCast does not report initial overlaps. Check these first; a wall overlapping
            // the origin blocks the swing, even if a target also overlaps it.
            int overlapCount;
            while ((overlapCount = Physics.OverlapSphereNonAlloc(origin, Radius, _overlaps,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) == _overlaps.Length)
            {
                Array.Resize(ref _overlaps, _overlaps.Length * 2);
            }
            Fighter overlapTarget = null;
            for (int i = 0; i < overlapCount; i++)
            {
                Fighter target = _overlaps[i].GetComponentInParent<Fighter>();
                if (target == _fighter)
                {
                    continue;
                }
                if (target == null)
                {
                    return;
                }
                overlapTarget = target;
            }
            if (overlapTarget != null)
            {
                Resolve(overlapTarget, origin, direction);
                return;
            }

            int hitCount;
            while ((hitCount = Physics.SphereCastNonAlloc(origin, Radius, direction, _hits,
                _fighter.Actions.CurrentAttack.Reach - Radius,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) == _hits.Length)
            {
                Array.Resize(ref _hits, _hits.Length * 2);
            }
            float closest = float.PositiveInfinity;
            Fighter nearestTarget = null;
            Vector3 nearestPoint = default;
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = _hits[i];
                Fighter target = hit.collider.GetComponentInParent<Fighter>();
                if (target == _fighter || hit.distance > closest)
                {
                    continue;
                }
                closest = hit.distance;
                nearestTarget = target;
                nearestPoint = hit.point;
            }
            if (nearestTarget != null)
            {
                Resolve(nearestTarget, nearestPoint, direction);
            }
        }

        private void Resolve(Fighter target, Vector3 point, Vector3 direction)
        {
            if (!target.Health.IsAlive || !_fighter.Actions.TryRegisterHit(target.Id))
            {
                return;
            }
            FistAttack attack = _fighter.Actions.CurrentAttack;
            var hit = new DamageInfo(_fighter.Id, "fists", attack.Kind, attack.Damage,
                point, direction, Time.timeAsDouble, attack.Knockback, attack.HitStopSeconds);
            if (attack.Kind != AttackKind.Shove)
            {
                target.ApplyHit(hit);
            }
            // Only opted-in targets move; everyone else keeps the no-reaction default.
            if (target.TryGetComponent(out SandboxHitReceiver receiver))
            {
                receiver.Apply(hit);
            }
            // Hitstop freezes exactly the two fighters involved (see HitStop).
            _fighter.HitStop.Begin(hit.HitStopSeconds);
            target.HitStop.Begin(hit.HitStopSeconds);
            HitResolved?.Invoke(target, hit);
        }

        private void OnDrawGizmosSelected()
        {
            if (_aim == null)
            {
                return;
            }
            Gizmos.color = Color.yellow;
            Vector3 end = _aim.position + _aim.forward * (1.1f - Radius);
            Gizmos.DrawLine(_aim.position, end);
            Gizmos.DrawWireSphere(_aim.position, Radius);
            Gizmos.DrawWireSphere(end, Radius);
            if (_hasPrevious)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(_previousOrigin, _previousOrigin + _previousDirection * (1.1f - Radius));
                Gizmos.DrawLine(_previousOrigin + _previousDirection * (1.1f - Radius), end);
            }
        }
    }
}
