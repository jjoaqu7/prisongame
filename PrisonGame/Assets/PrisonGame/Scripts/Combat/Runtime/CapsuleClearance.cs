using UnityEngine;

namespace PrisonGame.Combat
{
    // How far a fighter's capsule can move along the floor before touching scenery or another fighter.
    // Shared by knockback and the inmate's movement, so neither can push him through a wall.
    public static class CapsuleClearance
    {
        public const float Skin = 0.02f;

        // Every layer, including Ignore Raycast: the prototype player sits on that layer so its own
        // interaction ray skips it, but fights must still find and collide with it. Each query filters
        // out the acting fighter by its Fighter component instead.
        public const int Layers = Physics.AllLayers;

        public static float Distance(CapsuleCollider capsule, Vector3 direction, float wanted, Fighter self,
            ref RaycastHit[] buffer)
        {
            if (wanted <= 0f)
            {
                return 0f;
            }
            Physics.SyncTransforms();
            Bounds bounds = capsule.bounds;
            float radius = Mathf.Min(bounds.extents.x, bounds.extents.z);
            float halfSegment = Mathf.Max(0f, bounds.extents.y - radius);
            Vector3 top = bounds.center + Vector3.up * halfSegment;
            Vector3 bottom = bounds.center - Vector3.up * halfSegment;
            float distance = wanted;
            // A small skin avoids the floor's touching surface counting as a horizontal obstacle.
            int count;
            while ((count = Physics.CapsuleCastNonAlloc(top, bottom, radius - 0.01f, direction, buffer,
                wanted, Layers, QueryTriggerInteraction.Ignore)) == buffer.Length)
            {
                System.Array.Resize(ref buffer, buffer.Length * 2);
            }
            for (int i = 0; i < count; i++)
            {
                RaycastHit obstacle = buffer[i];
                if (obstacle.collider.GetComponentInParent<Fighter>() == self)
                {
                    continue;
                }
                distance = Mathf.Min(distance, Mathf.Max(0f, obstacle.distance - Skin));
            }
            return distance;
        }
    }
}
