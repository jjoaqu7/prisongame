using System;
using UnityEngine;

namespace PrisonGame.Prototype
{
    // Shared room/observer state. All authored markers move with this module.
    public sealed class LaundryRoom : MonoBehaviour
    {
        public Transform guard, eyes, workLook, deskLook;
        public BoxCollider workArea, supplyArea;
        public GameObject toolVisual;
        [Serializable] public sealed class State { public int segment; public float elapsed, yaw; }
        readonly float[] durations = {8,11,9,13};
        int segment; float elapsed;
        public bool WatchingWork => segment % 2 == 0;
        void Update() => Advance(Time.deltaTime);
        public void Advance(float seconds)
        {
            if (!float.IsFinite(seconds) || seconds <= 0) return;
            elapsed += seconds;
            while (elapsed >= durations[segment]) { elapsed -= durations[segment]; segment = (segment + 1) % durations.Length; }
            Vector3 direction = (WatchingWork ? workLook : deskLook).position - guard.position; direction.y = 0;
            guard.rotation = Quaternion.RotateTowards(guard.rotation, Quaternion.LookRotation(direction), 65 * seconds);
        }
        public static bool Contains(BoxCollider box, Vector3 point)
        {
            Vector3 p = box.transform.InverseTransformPoint(point) - box.center;
            Vector3 h = box.size * .5f;
            return Mathf.Abs(p.x) <= h.x && Mathf.Abs(p.y) <= h.y && Mathf.Abs(p.z) <= h.z;
        }
        public bool AtWork(PlayerInteraction actor) => actor != null && Contains(workArea, actor.transform.position + Vector3.up);
        public bool InSupply(PlayerInteraction actor) => actor != null && Contains(supplyArea, actor.transform.position + Vector3.up);
        public bool CanSee(PlayerInteraction actor)
        {
            if (actor == null) return false;
            Vector3 delta = actor.transform.position + Vector3.up * 1.1f - eyes.position;
            if (delta.magnitude > 11 || Vector3.Angle(guard.forward, new Vector3(delta.x,0,delta.z)) > 50) return false;
            foreach (var hit in Physics.RaycastAll(eyes.position, delta.normalized, delta.magnitude, ~0, QueryTriggerInteraction.Ignore))
                if (!hit.transform.IsChildOf(guard) && !hit.transform.IsChildOf(actor.transform)) return false;
            return true;
        }
        public State Capture() => new State {segment=segment, elapsed=elapsed, yaw=guard.localEulerAngles.y};
        public bool Valid(State s) => s != null && s.segment >= 0 && s.segment < 4 && float.IsFinite(s.elapsed) && s.elapsed >= 0 && s.elapsed < durations[s.segment] && float.IsFinite(s.yaw);
        public void Restore(State s) { segment=s.segment; elapsed=s.elapsed; guard.localRotation=Quaternion.Euler(0,s.yaw,0); }
    }
}
