using UnityEngine;

namespace PrisonGame.Prototype
{
    // Presentation-only correction for level-ground animation crossfades.
    // Attach to the visual rig below the gameplay object/collider.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public sealed class RigFloorClearance : MonoBehaviour
    {
        [SerializeField] private Transform leftFoot;
        [SerializeField] private Transform rightFoot;
        [SerializeField] private Bounds leftSoleBounds;
        [SerializeField] private Bounds rightSoleBounds;

        private Vector3 restPosition;
        private bool initialized;

        private void Awake()
        {
            restPosition = transform.localPosition;
            initialized = true;
        }

        private void LateUpdate() => ApplyFloorClearance();

        // Public so an Animator evaluated manually in verification can use the same correction.
        public void ApplyFloorClearance()
        {
            if (!initialized || leftFoot == null || rightFoot == null) return;
            transform.localPosition = restPosition;
            Vector3 origin = transform.position;
            Vector3 up = transform.parent != null ? transform.parent.up : Vector3.up;
            float lowest = Mathf.Min(Lowest(leftFoot, leftSoleBounds, origin, up),
                                     Lowest(rightFoot, rightSoleBounds, origin, up));
            if (lowest < 0f) transform.position -= up * lowest;
        }

        private static float Lowest(Transform foot, Bounds sole, Vector3 origin, Vector3 up)
        {
            float lowest = float.PositiveInfinity;
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = sole.center + Vector3.Scale(sole.extents,
                    new Vector3((i & 1) == 0 ? -1 : 1,
                                (i & 2) == 0 ? -1 : 1,
                                (i & 4) == 0 ? -1 : 1));
                lowest = Mathf.Min(lowest, Vector3.Dot(foot.TransformPoint(corner) - origin, up));
            }
            return lowest;
        }

        private void OnDisable()
        {
            if (initialized) transform.localPosition = restPosition;
        }
    }
}
