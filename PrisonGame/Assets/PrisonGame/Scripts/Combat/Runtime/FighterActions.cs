using System;
using System.Collections.Generic;
using PrisonGame.Core;

namespace PrisonGame.Combat
{
    // One frame of intent from a local adapter or an NPC decision. No input-device references.
    public readonly struct CombatIntent
    {
        public bool Enabled { get; }
        public bool PunchPressed { get; }
        public bool KickPressed { get; }
        public bool BlockHeld { get; }
        public bool ShovePressed { get; }

        public CombatIntent(bool enabled, bool punchPressed = false, bool kickPressed = false,
            bool blockHeld = false, bool shovePressed = false)
        {
            Enabled = enabled;
            PunchPressed = punchPressed;
            KickPressed = kickPressed;
            BlockHeld = blockHeld;
            ShovePressed = shovePressed;
        }
    }

    // Provisional unarmed tuning, separated from device input and presentation. Agreed
    // September 27: no charged punch; the kick does slightly more damage than the punch and is
    // slower, reaches further and pushes harder, and it staggers the inmate (full stagger power).
    public readonly struct UnarmedAttack
    {
        public const float ShoveDistance = 1f;

        public AttackKind Kind { get; }
        public string WeaponId { get; }
        public float Damage { get; }
        public float Reach { get; }
        public AttackTiming Timing { get; }
        public float HitStopSeconds { get; }
        public float Knockback { get; }
        public float StaggerPower { get; }

        public UnarmedAttack(AttackKind kind, string weaponId, float damage, float reach, AttackTiming timing,
            float hitStopSeconds, float knockback, float staggerPower)
        {
            Kind = kind;
            WeaponId = weaponId;
            Damage = damage;
            Reach = reach;
            Timing = timing;
            HitStopSeconds = hitStopSeconds;
            Knockback = knockback;
            StaggerPower = staggerPower;
        }

        public static UnarmedAttack Punch =>
            new UnarmedAttack(AttackKind.Punch, "fists", 10f, 1.1f, new AttackTiming(0.06f, 0.12f, 0.25f), 0.06f, 0.08f,
                25f);

        public static UnarmedAttack Kick =>
            new UnarmedAttack(AttackKind.Kick, "foot", 12f, 1.4f, new AttackTiming(0.3f, 0.14f, 0.45f), 0.09f, 0.6f,
                StaggerMeter.Capacity);

        public static UnarmedAttack Shove =>
            new UnarmedAttack(AttackKind.Shove, "hands", 0f, 1.1f, new AttackTiming(0.05f, 0.12f, 0.35f), 0.05f,
                ShoveDistance, 0f);

        // The neighboring inmate's telegraphed punch: 12 damage after a 0.7 s wind-up (agreed starting
        // values). It never staggers the player; the player's defence is the block and timed block.
        public static UnarmedAttack InmatePunch =>
            new UnarmedAttack(AttackKind.Punch, "fists", 12f, 1.2f, new AttackTiming(0.7f, 0.15f, 0.6f), 0.08f, 0f, 0f);

        public static UnarmedAttack For(AttackKind kind) => UnarmedMoveset.Player.For(kind);
    }

    // The unarmed attacks one fighter can make. Weapons (step 5) will replace these per held item.
    public sealed class UnarmedMoveset
    {
        public UnarmedMoveset(UnarmedAttack punch, UnarmedAttack kick, UnarmedAttack shove)
        {
            Punch = punch;
            Kick = kick;
            Shove = shove;
        }

        public UnarmedAttack Punch { get; }
        public UnarmedAttack Kick { get; }
        public UnarmedAttack Shove { get; }

        public static UnarmedMoveset Player { get; } =
            new UnarmedMoveset(UnarmedAttack.Punch, UnarmedAttack.Kick, UnarmedAttack.Shove);

        // He fights with the one telegraphed punch; kick and shove are kept for completeness.
        public static UnarmedMoveset Inmate { get; } =
            new UnarmedMoveset(UnarmedAttack.InmatePunch, UnarmedAttack.Kick, UnarmedAttack.Shove);

        public UnarmedAttack For(AttackKind kind)
        {
            switch (kind)
            {
                case AttackKind.Kick:
                    return Kick;
                case AttackKind.Shove:
                    return Shove;
                default:
                    return Punch;
            }
        }
    }

    // Owns one buffered press and the per-swing hit set for one explicit actor. A press starts
    // the attack at once; a press made during recovery is kept briefly and fires when recovery
    // ends. State transitions expose even active windows crossed entirely by a long frame.
    public sealed class FighterActions
    {
        public const float BufferSeconds = 0.15f;
        private readonly FighterStateMachine _state;
        private readonly HashSet<StableId> _hitTargets = new HashSet<StableId>();
        private float _bufferRemaining;
        private AttackKind _buffered;
        private bool _ownsAction;
        private UnarmedMoveset _moveset = UnarmedMoveset.Player;
        private float _windUpScale = 1f;

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
        public UnarmedAttack CurrentAttack { get; private set; } = UnarmedAttack.Punch;

        public UnarmedMoveset Moveset
        {
            get => _moveset;
            set => _moveset = value ?? throw new ArgumentNullException(nameof(value));
        }

        // Stretches the wind-up of attacks started from now on; the hurt inmate winds up slower.
        public float WindUpScale
        {
            get => _windUpScale;
            set
            {
                if (!(value > 0f) || float.IsInfinity(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Wind-up scale must be positive and finite.");
                }
                _windUpScale = value;
            }
        }

        public int SwingNumber { get; private set; }
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
            _state.Tick(deltaSeconds);

            if (intent.BlockHeld)
            {
                _bufferRemaining = 0f;
                _state.StartBlock();
                return;
            }
            _state.EndBlock();

            if (intent.ShovePressed && _state.State == FighterState.Idle)
            {
                _bufferRemaining = 0f;
                BeginAttack(_moveset.Shove);
                return;
            }
            // A kick wins if both are pressed in the same frame: it is the deliberate, slower move.
            bool requested = intent.KickPressed || intent.PunchPressed;
            AttackKind kind = intent.KickPressed ? AttackKind.Kick : AttackKind.Punch;
            if (requested && _state.State == FighterState.Idle)
            {
                BeginAttack(_moveset.For(kind));
                return;
            }
            if (requested && _state.State == FighterState.Recovery && _ownsAction)
            {
                _buffered = kind;
                _bufferRemaining = BufferSeconds;
            }
            if (_state.State == FighterState.Idle && _bufferRemaining > 0f)
            {
                _bufferRemaining = 0f;
                BeginAttack(_moveset.For(_buffered));
            }
        }

        public bool TryRegisterHit(StableId target)
        {
            return CanHit && target.IsValid && target != Actor && _hitTargets.Add(target);
        }

        public void Cancel()
        {
            _bufferRemaining = 0f;
            if (_ownsAction)
            {
                _ownsAction = false;
                if (_state.State == FighterState.WindUp || _state.State == FighterState.Active ||
                    _state.State == FighterState.Recovery)
                {
                    _state.Recover();
                }
            }
            _state.EndBlock();
        }

        private void BeginAttack(UnarmedAttack attack)
        {
            AttackTiming timing = attack.Timing;
            if (_windUpScale != 1f)
            {
                timing = new AttackTiming(timing.WindUp * _windUpScale, timing.Active, timing.Recovery);
                attack = new UnarmedAttack(attack.Kind, attack.WeaponId, attack.Damage, attack.Reach, timing,
                    attack.HitStopSeconds, attack.Knockback, attack.StaggerPower);
            }
            if (!_state.TryStartAttack(timing))
            {
                return;
            }
            CurrentAttack = attack;
            _hitTargets.Clear();
            _ownsAction = true;
            SwingNumber++;
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
                if (to != FighterState.Idle)
                {
                    _bufferRemaining = 0f;
                }
            }
        }
    }
}
