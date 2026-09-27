using System;
using PrisonGame.Core;

namespace PrisonGame.Combat
{
    public enum LifeState
    {
        Alive,
        Downed,
        Dead,
    }

    // Agreed rule: zero health leaves a fighter downed; killing is a separate action.
    // Plain C# with no clock, input or scene access, so tests can drive it directly.
    public sealed class Health
    {
        public Health(float max)
        {
            if (!(max > 0f) || float.IsInfinity(max))
            {
                throw new ArgumentOutOfRangeException(nameof(max), "Maximum health must be positive and finite.");
            }
            Max = max;
            Current = max;
        }

        public float Max { get; }
        public float Current { get; private set; }
        public LifeState State { get; private set; }
        public bool IsAlive => State == LifeState.Alive;

        public event Action<DamageInfo, DamageResult> Damaged;
        public event Action<DamageInfo> WentDown;
        public event Action<StableId> Killed;

        public DamageResult Apply(in DamageInfo hit, BlockOutcome block = BlockOutcome.None)
        {
            if (!(hit.Amount >= 0f) || float.IsInfinity(hit.Amount))
            {
                throw new ArgumentOutOfRangeException(nameof(hit), "Damage must be zero or positive and finite.");
            }
            // Attacks do not hurt a downed fighter; only the kill action ends him.
            if (State != LifeState.Alive)
            {
                return DamageResult.IgnoredHit;
            }
            float applied = Math.Min(hit.Amount, Current);
            Current -= applied;
            bool wentDown = Current <= 0f;
            if (wentDown)
            {
                Current = 0f;
                State = LifeState.Downed;
            }
            var result = new DamageResult(applied, wentDown, false, block);
            Damaged?.Invoke(hit, result);
            if (wentDown)
            {
                WentDown?.Invoke(hit);
            }
            return result;
        }

        public bool Kill(StableId killer)
        {
            if (State != LifeState.Downed)
            {
                return false;
            }
            State = LifeState.Dead;
            Killed?.Invoke(killer);
            return true;
        }

        // Returns health to a value saved earlier, such as the fight-start checkpoint. Unlike
        // RestoreFull this can bring back a downed or dead fighter, because a checkpoint undoes the fight.
        public void Restore(float current, LifeState state)
        {
            if (!(current >= 0f) || current > Max)
            {
                throw new ArgumentOutOfRangeException(nameof(current),
                    "Restored health must be between zero and the maximum.");
            }
            if ((state == LifeState.Alive) != (current > 0f))
            {
                throw new ArgumentException(
                    "A living fighter has health above zero; a downed or dead one has none.", nameof(state));
            }
            Current = current;
            State = state;
        }

        // Used when a fight ends without a result and both fighters recover.
        public bool RestoreFull()
        {
            if (State == LifeState.Dead)
            {
                return false;
            }
            Current = Max;
            State = LifeState.Alive;
            return true;
        }
    }
}
