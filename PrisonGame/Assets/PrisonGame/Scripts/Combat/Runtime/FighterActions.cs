using System;
using System.Collections.Generic;
using PrisonGame.Core;

namespace PrisonGame.Combat
{
    // One frame of intent from a local adapter or an NPC decision. No input-device references.
    public readonly struct CombatIntent
    {
        public bool Enabled { get; }
        public bool AttackPressed { get; }
        public bool AttackHeld { get; }
        public bool BlockHeld { get; }
        public bool ShovePressed { get; }

        public CombatIntent(bool enabled, bool attackPressed = false, bool attackHeld = false,
            bool blockHeld = false, bool shovePressed = false)
        {
            Enabled = enabled;
            AttackPressed = attackPressed;
            AttackHeld = attackHeld;
            BlockHeld = blockHeld;
            ShovePressed = shovePressed;
        }
    }

    // Provisional fist tuning, separated from device input and presentation.
    public readonly struct FistAttack
    {
        public AttackKind Kind { get; }
        public float Damage { get; }
        public float Reach => 1.1f;
        public AttackTiming Timing { get; }

        private FistAttack(AttackKind kind, float damage, AttackTiming timing)
        {
            Kind = kind;
            Damage = damage;
            Timing = timing;
        }

        public static FistAttack Punch(float chargeSeconds)
        {
            float charge = Math.Clamp((chargeSeconds - 0.18f) / 0.62f, 0f, 1f);
            return new FistAttack(charge > 0f ? AttackKind.Charged : AttackKind.Quick,
                10f + 12f * charge, new AttackTiming(0.06f, 0.12f, 0.25f + 0.25f * charge));
        }

        public static FistAttack Shove =>
            new FistAttack(AttackKind.Shove, 0f, new AttackTiming(0.05f, 0.12f, 0.35f));
    }

    // Owns charge, one buffered press, and the per-swing hit set for one explicit actor.
    // State transitions expose even active windows crossed entirely by a long frame.
    public sealed class FighterActions
    {
        public const float BufferSeconds = 0.15f;
        private readonly FighterStateMachine _state;
        private readonly HashSet<StableId> _hitTargets = new HashSet<StableId>();
        private float _bufferRemaining;
        private bool _ownsAction;

        public FighterActions(StableId actor, FighterStateMachine state)
        {
            if (!actor.IsValid)
            {
                throw new ArgumentException("An acting fighter needs a stable ID.", nameof(actor));
            }
            Actor = actor;
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _state.Changed += OnStateChanged;
        }

        public StableId Actor { get; }
        public FistAttack CurrentAttack { get; private set; }
        public float ChargeSeconds { get; private set; }
        public int SwingNumber { get; private set; }
        public bool IsCharging => _ownsAction && _state.State == FighterState.Charging;
        public bool CanHit => _ownsAction && _state.HitWindowOpen;
        public event Action ActiveWindowOpened;

        public void Tick(float deltaSeconds, CombatIntent intent)
        {
            if (!(deltaSeconds >= 0f) || float.IsInfinity(deltaSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            }
            if (!intent.Enabled)
            {
                Cancel();
                _state.Tick(deltaSeconds);
                return;
            }

            _bufferRemaining = Math.Max(0f, _bufferRemaining - deltaSeconds);
            if (IsCharging)
            {
                ChargeSeconds = Math.Min(0.8f, ChargeSeconds + deltaSeconds);
            }
            _state.Tick(deltaSeconds);

            if (intent.BlockHeld)
            {
                _bufferRemaining = 0f;
                if (IsCharging)
                {
                    Cancel();
                }
                _state.StartBlock();
                return;
            }
            _state.EndBlock();

            if (intent.ShovePressed && (_state.State == FighterState.Idle || IsCharging))
            {
                Cancel();
                BeginAttack(FistAttack.Shove, false);
                return;
            }
            if (intent.AttackPressed)
            {
                if (_state.State == FighterState.Idle)
                {
                    BeginCharge();
                }
                else if (_state.State == FighterState.Recovery)
                {
                    _bufferRemaining = BufferSeconds;
                }
            }
            if (_state.State == FighterState.Idle && _bufferRemaining > 0f)
            {
                BeginCharge();
            }
            if (IsCharging && !intent.AttackHeld)
            {
                BeginAttack(FistAttack.Punch(ChargeSeconds), true);
            }
        }

        public bool TryRegisterHit(StableId target)
        {
            return CanHit && target.IsValid && target != Actor && _hitTargets.Add(target);
        }

        public void Cancel()
        {
            _bufferRemaining = 0f;
            ChargeSeconds = 0f;
            if (_ownsAction)
            {
                _ownsAction = false;
                if (_state.State == FighterState.Charging || _state.State == FighterState.WindUp ||
                    _state.State == FighterState.Active || _state.State == FighterState.Recovery)
                {
                    _state.Recover();
                }
            }
            _state.EndBlock();
        }

        private void BeginCharge()
        {
            _bufferRemaining = 0f;
            ChargeSeconds = 0f;
            _ownsAction = _state.BeginCharge();
        }

        private void BeginAttack(FistAttack attack, bool fromCharge)
        {
            CurrentAttack = attack;
            _hitTargets.Clear();
            _ownsAction = true;
            SwingNumber++;
            if (fromCharge)
            {
                _state.ReleaseCharge(attack.Timing);
            }
            else
            {
                _state.TryStartAttack(attack.Timing);
            }
        }

        private void OnStateChanged(FighterState from, FighterState to)
        {
            if (to == FighterState.Active && _ownsAction)
            {
                ActiveWindowOpened?.Invoke();
            }
            if (to == FighterState.Idle || to == FighterState.Staggered ||
                to == FighterState.Downed || to == FighterState.Dead)
            {
                _ownsAction = false;
                ChargeSeconds = 0f;
                if (to != FighterState.Idle)
                {
                    _bufferRemaining = 0f;
                }
            }
        }
    }
}
