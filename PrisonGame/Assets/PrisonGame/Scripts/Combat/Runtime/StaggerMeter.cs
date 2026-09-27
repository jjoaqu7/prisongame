using System;

namespace PrisonGame.Combat
{
    // Builds up as a fighter takes hits and staggers him when full, so one punch cannot interrupt a
    // wind-up but a flurry or a kick can. It drains once he has not been hit for a moment. Plain C#
    // with time passed in.
    public sealed class StaggerMeter
    {
        public const float Capacity = 100f;
        public const float DecayDelaySeconds = 1f;
        public const float DecayPerSecond = 25f;
        private float _sinceLastAdd = float.PositiveInfinity;

        public float Value { get; private set; }

        // Returns true when this addition fills the meter; the meter then empties for the next build-up.
        public bool Add(float amount)
        {
            if (!(amount >= 0f) || float.IsInfinity(amount))
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Stagger build-up must be zero or positive and finite.");
            }
            if (amount == 0f)
            {
                return false;
            }
            _sinceLastAdd = 0f;
            Value = Math.Min(Capacity, Value + amount);
            if (Value < Capacity)
            {
                return false;
            }
            Value = 0f;
            return true;
        }

        public void Tick(float deltaSeconds)
        {
            if (!(deltaSeconds >= 0f) || float.IsInfinity(deltaSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds), "Tick time must be zero or positive and finite.");
            }
            _sinceLastAdd += deltaSeconds;
            if (Value <= 0f)
            {
                return;
            }
            // Only the part of this tick after the delay drains the meter.
            float draining = Math.Min(deltaSeconds, _sinceLastAdd - DecayDelaySeconds);
            if (draining > 0f)
            {
                Value = Math.Max(0f, Value - DecayPerSecond * draining);
            }
        }

        public void Reset()
        {
            Value = 0f;
            _sinceLastAdd = float.PositiveInfinity;
        }
    }
}
