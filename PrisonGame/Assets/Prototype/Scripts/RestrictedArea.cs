using UnityEngine;

namespace PrisonGame.Prototype
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class RestrictedArea : MonoBehaviour
    {
        public bool Contains(PlayerInteraction actor)
        {
            if (actor == null || !isActiveAndEnabled) return false;
            var box = GetComponent<BoxCollider>();
            if (!box.enabled) return false;
            Vector3 point = transform.InverseTransformPoint(actor.transform.position + Vector3.up * .9f) - box.center;
            Vector3 half = box.size * .5f;
            return Mathf.Abs(point.x) <= half.x && Mathf.Abs(point.y) <= half.y && Mathf.Abs(point.z) <= half.z;
        }
    }
}
