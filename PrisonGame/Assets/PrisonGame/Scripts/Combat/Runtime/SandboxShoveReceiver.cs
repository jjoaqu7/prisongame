using PrisonGame.Core;
using UnityEngine;

namespace PrisonGame.Combat
{
    // Opt-in response for the neighboring-cell stand-in only. Other characters receive no reaction.
    // Replace with the inmate's movement adapter when its AI and ragdoll are introduced.
    [RequireComponent(typeof(Fighter), typeof(CapsuleCollider))]
    public sealed class SandboxShoveReceiver : MonoBehaviour
    {
        private Fighter _fighter;
        private CapsuleCollider _capsule;
        private RaycastHit[] _obstacles = new RaycastHit[16];
        public StableId LastAttacker { get; private set; }

        private void Awake()
        {
            _fighter = GetComponent<Fighter>();
            _capsule = GetComponent<CapsuleCollider>();
        }

        public void Apply(DamageInfo hit)
        {
            if (!_fighter.Health.IsAlive || hit.Kind != AttackKind.Shove)
            {
                return;
            }
            LastAttacker = hit.Attacker;
            _fighter.State.Stagger(0.45f);
            Vector3 direction = Vector3.ProjectOnPlane(hit.Direction, Vector3.up).normalized;
            if (direction.sqrMagnitude < 0.5f)
            {
                return;
            }
            Physics.SyncTransforms();
            Bounds bounds = _capsule.bounds;
            float radius = Mathf.Min(bounds.extents.x, bounds.extents.z);
            float halfSegment = Mathf.Max(0f, bounds.extents.y - radius);
            Vector3 top = bounds.center + Vector3.up * halfSegment;
            Vector3 bottom = bounds.center - Vector3.up * halfSegment;
            float distance = 1f;
            // A small skin avoids the floor's touching surface counting as a horizontal obstacle.
            int count;
            while ((count = Physics.CapsuleCastNonAlloc(top, bottom, radius - 0.01f,
                direction, _obstacles, distance, Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore)) == _obstacles.Length)
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
            transform.position += direction * distance;
            Physics.SyncTransforms();
        }
    }
}
