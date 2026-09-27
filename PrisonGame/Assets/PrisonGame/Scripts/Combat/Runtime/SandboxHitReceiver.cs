using PrisonGame.Core;
using UnityEngine;

namespace PrisonGame.Combat
{
    // Opt-in physical response for the neighboring-cell stand-in only; other characters receive no
    // reaction. Moves the capsule by each hit's knockback, stopping short of scenery. A shove also
    // staggers. Replace with the inmate's movement adapter when its AI and ragdoll are introduced.
    [RequireComponent(typeof(Fighter), typeof(CapsuleCollider))]
    public sealed class SandboxHitReceiver : MonoBehaviour
    {
        public const float ShoveStaggerSeconds = 0.45f;
        private Fighter _fighter;
        private CapsuleCollider _capsule;
        private RaycastHit[] _obstacles = new RaycastHit[16];

        public StableId LastAttacker { get; private set; }
        public float LastDisplacement { get; private set; }

        private void Awake()
        {
            _fighter = GetComponent<Fighter>();
            _capsule = GetComponent<CapsuleCollider>();
        }

        // Called by the attacker's hit resolution after damage, or instead of damage for a shove.
        public void Apply(DamageInfo hit)
        {
            LastDisplacement = 0f;
            if (!_fighter.Health.IsAlive && hit.Kind == AttackKind.Shove)
            {
                return;
            }
            LastAttacker = hit.Attacker;
            if (hit.Kind == AttackKind.Shove)
            {
                _fighter.State.Stagger(ShoveStaggerSeconds);
            }
            // A punch that downs him still pushes him back; the downed pose comes in step 6.
            Vector3 direction = Vector3.ProjectOnPlane(hit.Direction, Vector3.up).normalized;
            if (hit.Knockback <= 0f || direction.sqrMagnitude < 0.5f)
            {
                return;
            }
            float distance = ClearDistance(direction, hit.Knockback);
            transform.position += direction * distance;
            LastDisplacement = distance;
            Physics.SyncTransforms();
        }

        private float ClearDistance(Vector3 direction, float wanted)
        {
            Physics.SyncTransforms();
            Bounds bounds = _capsule.bounds;
            float radius = Mathf.Min(bounds.extents.x, bounds.extents.z);
            float halfSegment = Mathf.Max(0f, bounds.extents.y - radius);
            Vector3 top = bounds.center + Vector3.up * halfSegment;
            Vector3 bottom = bounds.center - Vector3.up * halfSegment;
            float distance = wanted;
            // A small skin avoids the floor's touching surface counting as a horizontal obstacle.
            int count;
            while ((count = Physics.CapsuleCastNonAlloc(top, bottom, radius - 0.01f, direction, _obstacles,
                wanted, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) == _obstacles.Length)
            {
                System.Array.Resize(ref _obstacles, _obstacles.Length * 2);
            }
            for (int i = 0; i < count; i++)
            {
                RaycastHit obstacle = _obstacles[i];
                if (obstacle.collider.GetComponentInParent<Fighter>() == _fighter)
                {
                    continue;
                }
                distance = Mathf.Min(distance, Mathf.Max(0f, obstacle.distance - 0.02f));
            }
            return distance;
        }
    }
}
