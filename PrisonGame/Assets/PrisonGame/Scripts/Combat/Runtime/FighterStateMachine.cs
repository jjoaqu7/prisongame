using System;
using UnityEngine;

namespace PrisonGame.Combat
{
    public enum FighterState
    {
        Idle,
        WindUp,
        Active,
        Recovery,
        Blocking,
        Staggered,
        Downed,
        Dead,
    }

    // Attack phases in seconds. Only the active window can hit. Stored as data so the
    // rules run the same timeline with or without an Animator.
    [Serializable]
    public struct AttackTiming
    {
        [SerializeField]
        private float _windUp;

        [SerializeField]
        private float _active;

        [SerializeField]
        private float _recovery;

        public AttackTiming(float windUp, float active, float recovery)
        {
            _windUp = windUp;
            _active = active;
            _recovery = recovery;
        }

        public float WindUp => _windUp;
        public float Active => _active;
        public float Recovery => _recovery;

        public bool IsValid =>
            _windUp >= 0f
            && _active > 0f
            && _recovery >= 0f
            && !float.IsInfinity(_windUp)
            && !float.IsInfinity(_active)
            && !float.IsInfinity(_recovery);
    }

    // One fighter's current action. Time is passed in; nothing here reads a clock,
    // input or the Animator. A long tick walks through every phase it spans, so a
    // frame spike cannot skip an attack's active window without reporting it.
    public sealed class FighterStateMachine
    {
        private float _staggerDuration;

        public FighterState State { get; private set; } = FighterState.Idle;
        public float TimeInState { get; private set; }
        public AttackTiming CurrentAttack { get; private set; }
        public bool HitWindowOpen => State == FighterState.Active;

        // Raised for every transition, in order, including phases passed within one tick.
        public event Action<FighterState, FighterState> Changed;

        public bool TryStartAttack(AttackTiming timing)
        {
            if (!timing.IsValid)
            {
                throw new ArgumentException(
                    "Attack timing must have a positive active window and no negative or infinite phases.",
                    nameof(timing)
                );
            }
            if (State != FighterState.Idle)
            {
                return false;
            }
            CurrentAttack = timing;
            Enter(FighterState.WindUp);
            return true;
        }

        public bool StartBlock()
        {
            if (State != FighterState.Idle)
            {
                return false;
            }
            Enter(FighterState.Blocking);
            return true;
        }

        public void EndBlock()
        {
            if (State == FighterState.Blocking)
            {
                Enter(FighterState.Idle);
            }
        }

        // Interrupts any wind-up, swing, recovery or block.
        public bool Stagger(float seconds)
        {
            if (!(seconds > 0f) || float.IsInfinity(seconds))
            {
                throw new ArgumentOutOfRangeException(nameof(seconds), "Stagger must last a positive, finite time.");
            }
            if (State == FighterState.Downed || State == FighterState.Dead)
            {
                return false;
            }
            _staggerDuration = seconds;
            Enter(FighterState.Staggered);
            return true;
        }

        public void GoDown()
        {
            if (State != FighterState.Dead && State != FighterState.Downed)
            {
                Enter(FighterState.Downed);
            }
        }

        public void Die()
        {
            if (State != FighterState.Dead)
            {
                Enter(FighterState.Dead);
            }
        }

        // Used when a fight ends without a result; a dead fighter stays dead.
        public bool Recover()
        {
            if (State == FighterState.Dead)
            {
                return false;
            }
            if (State != FighterState.Idle)
            {
                Enter(FighterState.Idle);
            }
            return true;
        }

        public void Tick(float deltaSeconds)
        {
            if (!(deltaSeconds >= 0f) || float.IsInfinity(deltaSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds), "Tick time must be zero or positive and finite.");
            }
            float remaining = deltaSeconds;
            while (true)
            {
                float duration = PhaseDuration();
                if (float.IsPositiveInfinity(duration))
                {
                    TimeInState += remaining;
                    return;
                }
                float left = Mathf.Max(0f, duration - TimeInState);
                if (remaining < left)
                {
                    TimeInState += remaining;
                    return;
                }
                remaining -= left;
                Enter(NextPhase());
            }
        }

        private float PhaseDuration()
        {
            switch (State)
            {
                case FighterState.WindUp:
                    return CurrentAttack.WindUp;
                case FighterState.Active:
                    return CurrentAttack.Active;
                case FighterState.Recovery:
                    return CurrentAttack.Recovery;
                case FighterState.Staggered:
                    return _staggerDuration;
                default:
                    return float.PositiveInfinity;
            }
        }

        private FighterState NextPhase()
        {
            switch (State)
            {
                case FighterState.WindUp:
                    return FighterState.Active;
                case FighterState.Active:
                    return FighterState.Recovery;
                default:
                    return FighterState.Idle;
            }
        }

        private void Enter(FighterState next)
        {
            FighterState previous = State;
            State = next;
            TimeInState = 0f;
            Changed?.Invoke(previous, next);
        }
    }
}
