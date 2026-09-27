using System;

namespace PrisonGame.Combat
{
    // A brief freeze of one fighter's combat timeline when a hit lands, applied to the attacker and
    // the target only. Never Time.timeScale: a global freeze would hand any third fighter, or a
    // second co-op player, free time. Plain C# with time passed in.
    public sealed class HitStop
    {
        public float Remaining { get; private set; }
        public bool IsActive => Remaining > 0f;

        // Overlapping hits extend the freeze to the longest one, never the sum.
        public void Begin(float seconds)
        {
            if (!(seconds >= 0f) || float.IsInfinity(seconds))
            {
                throw new ArgumentOutOfRangeException(nameof(seconds), "Hitstop must be zero or positive and finite.");
            }
            Remaining = Math.Max(Remaining, seconds);
        }

        public void Clear()
        {
            Remaining = 0f;
        }

        // Returns the part of deltaSeconds left for the fighter's timeline after the freeze.
        public float Consume(float deltaSeconds)
        {
            if (!(deltaSeconds >= 0f) || float.IsInfinity(deltaSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds), "Tick time must be zero or positive and finite.");
            }
            if (Remaining >= deltaSeconds)
            {
                Remaining -= deltaSeconds;
                return 0f;
            }
            float left = deltaSeconds - Remaining;
            Remaining = 0f;
            return left;
        }
    }
}
