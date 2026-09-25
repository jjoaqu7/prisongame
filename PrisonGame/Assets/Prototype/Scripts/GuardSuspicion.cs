using UnityEngine;

namespace PrisonGame.Prototype
{
    // This player's current suspicion with one guard, separate from local input and HUD.
    [RequireComponent(typeof(PlayerInteraction))]
    public sealed class GuardSuspicion : MonoBehaviour
    {
        [SerializeField] private GuardObserver observer;
        [SerializeField] private PrisonClock clock;
        public GuardObserver Observer => observer;
        public float Level { get; private set; }
        public bool Inside { get; private set; }
        public bool Seen { get; private set; }
        public bool Rising => Inside && Seen;
        public bool OrderedOut => Level >= 100;

        internal void Restore(float level)
        {
            Level = level;
            var actor = GetComponent<PlayerInteraction>();
            Inside = observer != null && observer.Area != null && observer.Area.Contains(actor);
            Seen = observer != null && observer.CanSee(actor);
        }

        private void Update() => Step(Time.deltaTime);

        public void Step(float seconds)
        {
            if (seconds <= 0 || !float.IsFinite(seconds) || (clock != null && clock.Paused)) return;
            var actor = GetComponent<PlayerInteraction>();
            Inside = observer != null && observer.Area != null && observer.Area.Contains(actor);
            Seen = observer != null && observer.CanSee(actor);
            Level = Mathf.Clamp(Level + (Rising ? 12.5f : -20f) * seconds, 0, 100);
        }
    }
}
