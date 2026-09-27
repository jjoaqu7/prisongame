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

        private float ClearDistance(Vector3 direction, float wanted) =>
            CapsuleClearance.Distance(_capsule, direction, wanted, _fighter, ref _obstacles);
    }
}
