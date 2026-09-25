using UnityEngine;

namespace PrisonGame.Prototype
{
    // Shared character state and straight-line movement commands; no local input or UI.
    // This study supports upright, unit-scale capsules on a level floor.
    [DefaultExecutionOrder(-20)]
    [RequireComponent(typeof(PrototypeInmate), typeof(CapsuleCollider))]
    public sealed class InmateMotion : MonoBehaviour
    {
        public const float WalkSpeed = .52f;
        public const float TalkDuration = 6f;
        public const float GestureDuration = 2.4f;
        private const float Clearance = .01f;
        private CapsuleCollider body;
        private PrototypeInmate inmate;
        private Vector3 destination;
        private Transform speaker;
        private float talkRemaining;

        public Vector3 Velocity { get; private set; }
        public bool HasDestination { get; private set; }
        public bool IsBlocked { get; private set; }
        public bool IsTalking => talkRemaining > 0f;
        public bool IsGesturing => talkRemaining > TalkDuration - GestureDuration;
        public int TalkSequence { get; private set; }

        private void Awake()
        {
            body = GetComponent<CapsuleCollider>();
            inmate = GetComponent<PrototypeInmate>();
        }

        private void OnEnable() => inmate.Talked += BeginTalk;
        private void OnDisable()
        {
            inmate.Talked -= BeginTalk;
            Stop();
            talkRemaining = 0f;
            speaker = null;
        }

        private void BeginTalk(PlayerInteraction actor)
        {
            speaker = actor.transform;
            talkRemaining = TalkDuration;
            TalkSequence++;
            Velocity = Vector3.zero;
        }

        public bool SetDestination(Vector3 point)
        {
            if (!isActiveAndEnabled || !float.IsFinite(point.x) || !float.IsFinite(point.y) ||
                !float.IsFinite(point.z) || Mathf.Abs(point.y - transform.position.y) > .05f)
                return false;
            destination = new Vector3(point.x, transform.position.y, point.z);
            HasDestination = true;
            IsBlocked = false;
            return true;
        }

        public void Stop()
        {
            HasDestination = false;
            IsBlocked = false;
            Velocity = Vector3.zero;
        }

        private void Update() => Step(Time.deltaTime);

        private void Step(float dt)
        {
            Velocity = Vector3.zero;
            if (dt <= 0f || !float.IsFinite(dt)) return;
            if (IsTalking)
            {
                if (speaker != null) Face(speaker.position - transform.position, dt);
                talkRemaining = Mathf.Max(0f, talkRemaining - dt);
                return;
            }
            if (!HasDestination) return;
            Vector3 delta = destination - transform.position;
            delta.y = 0f;
            if (delta.magnitude < .001f) { Stop(); return; }
            Vector3 direction = delta.normalized;
            if (!Face(direction, dt)) return;

            Physics.SyncTransforms();
            float distance = Mathf.Min(WalkSpeed * dt, delta.magnitude);
            Vector3 center = transform.TransformPoint(body.center);
            float radius = body.radius - Clearance;
            float half = body.height * .5f - body.radius;
            foreach (RaycastHit hit in Physics.CapsuleCastAll(center + Vector3.up * half,
                center - Vector3.up * half, radius, direction, distance + Clearance,
                ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.transform.IsChildOf(transform)) continue;
                distance = Mathf.Min(distance, Mathf.Max(0f, hit.distance - Clearance));
            }

            Vector3 next = transform.position + direction * distance;
            // Reject ledges and height changes; stairs/slopes require later navigation work.
            bool supported = Supported(next) && Supported(next + direction * body.radius);
            if (!supported || distance < .00001f)
            {
                IsBlocked = true;
                return;
            }
            IsBlocked = false;
            Vector3 before = transform.position;
            transform.position = next;
            Velocity = (next - before) / dt;
            if ((destination - next).sqrMagnitude < .000001f) HasDestination = false;
        }

        private bool Supported(Vector3 point)
        {
            foreach (RaycastHit hit in Physics.RaycastAll(point + Vector3.up * .12f, Vector3.down,
                .16f, ~0, QueryTriggerInteraction.Ignore))
                if (!hit.collider.transform.IsChildOf(transform) && hit.normal.y > .99f &&
                    Mathf.Abs(hit.point.y - transform.position.y) < .015f) return true;
            return false;
        }

        private bool Face(Vector3 direction, float dt)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < .000001f) return true;
            // M5 faces local -Z.
            Quaternion rotation = Quaternion.LookRotation(-direction.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, 180f * dt);
            return Quaternion.Angle(transform.rotation, rotation) < .1f;
        }
    }
}
