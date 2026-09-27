using System;
using PrisonGame.Combat;

namespace PrisonGame.AI
{
    public enum InmateState
    {
        Idle,
        Approach,
        Ready,
        WindUp,
        Attack,
        Recover,
        Block,
        Stagger,
        Downed,
        Dead,
    }

    // What the scene tells the brain each tick. The brain never looks at the scene, the camera or input.
    public readonly struct InmatePerception
    {
        public InmatePerception(bool fightActive, float distanceToTarget, bool targetAvailable)
        {
            if (!(distanceToTarget >= 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(distanceToTarget), "Distance must be zero or positive.");
            }
            FightActive = fightActive;
            DistanceToTarget = distanceToTarget;
            TargetAvailable = targetAvailable;
        }

        public bool FightActive { get; }

        // Floor distance between his centre and the target's.
        public float DistanceToTarget { get; }

        // False when the target is down or outside his cell; he then holds his ground and waits.
        public bool TargetAvailable { get; }
    }

    // What the brain wants this tick. The scene adapter moves and turns him and submits the intent
    // exactly as the player's input adapter does.
    public readonly struct InmateDecision
    {
        public InmateDecision(CombatIntent intent, float moveSpeed, float turnDegreesPerSecond, float windUpScale)
        {
            Intent = intent;
            MoveSpeed = moveSpeed;
            TurnDegreesPerSecond = turnDegreesPerSecond;
            WindUpScale = windUpScale;
        }

        public CombatIntent Intent { get; }

        // Metres per second toward the target, stopping at PreferredDistance.
        public float MoveSpeed { get; }
        public float TurnDegreesPerSecond { get; }
        public float WindUpScale { get; }
    }

    // The neighboring inmate's fight decisions: a plain C# state machine (research decision 12). It reads
    // his fighter state and health, is told what he perceives, and returns intents. Agreed behaviour: one
    // telegraphed punch with 2 to 3 s between attacks, block, stagger, a slower wind-up when hurt, downed
    // and dead. He stays in his cell. Random choices come from his own seeded generator, so a fight
    // replays the same way in tests and sounds or effects can never change its outcome.
    public sealed class InmateBrain
    {
        public const float AttackRange = 1.3f;
        public const float ChaseRange = 1.6f;
        public const float PreferredDistance = 1f;
        public const float ApproachSpeed = 2f;
        public const float WindUpStepSpeed = 1f;
        public const float TurnSpeed = 540f;
        public const float WindUpTurnSpeed = 180f;
        public const float MinAttackInterval = 2f;
        public const float MaxAttackInterval = 3f;
        public const float MinFirstAttackDelay = 0.4f;
        public const float MaxFirstAttackDelay = 0.8f;
        public const float BlockChance = 0.35f;
        public const float BlockSeconds = 1f;
        public const float HurtWindUpScale = 1.3f;
        private static readonly CombatIntent s_ready = new CombatIntent(true);
        private static readonly CombatIntent s_punch = new CombatIntent(true, punchPressed: true);
        private static readonly CombatIntent s_block = new CombatIntent(true, blockHeld: true);
        private readonly FighterStateMachine _fighter;
        private readonly FighterDefense _defense;
        private readonly Health _health;
        private readonly int _seed;
        private Random _random;
        private bool _engaged;
        private bool _inRange;
        private bool _hitSinceLastTick;
        private float _nextAttackIn;
        private float _blockRemaining;

        public InmateBrain(FighterStateMachine fighter, FighterDefense defense, Health health, int seed)
        {
            _fighter = fighter ?? throw new ArgumentNullException(nameof(fighter));
            _defense = defense ?? throw new ArgumentNullException(nameof(defense));
            _health = health ?? throw new ArgumentNullException(nameof(health));
            _seed = seed;
            _random = new Random(seed);
            _defense.HitTaken += OnHitTaken;
        }

        public InmateState State { get; private set; }
        public int AttacksStarted { get; private set; }
        public float NextAttackIn => _nextAttackIn;

        // Every state change, in order, for tests and a debug readout.
        public event Action<InmateState, InmateState> Changed;

        public InmateDecision Tick(float deltaSeconds, in InmatePerception perception)
        {
            if (!(deltaSeconds >= 0f) || float.IsInfinity(deltaSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds), "Tick time must be zero or positive and finite.");
            }
            bool wasHit = _hitSinceLastTick;
            _hitSinceLastTick = false;
            float windUpScale = _defense.IsHurt ? HurtWindUpScale : 1f;
            if (_health.State != LifeState.Alive)
            {
                Disengage();
                SetState(_health.State == LifeState.Dead ? InmateState.Dead : InmateState.Downed);
                return new InmateDecision(default, 0f, 0f, windUpScale);
            }
            if (!perception.FightActive)
            {
                Disengage();
                SetState(InmateState.Idle);
                return new InmateDecision(default, 0f, 0f, windUpScale);
            }
            if (!_engaged)
            {
                Engage();
            }
            _nextAttackIn = Math.Max(0f, _nextAttackIn - deltaSeconds);
            _blockRemaining = Math.Max(0f, _blockRemaining - deltaSeconds);

            switch (_fighter.State)
            {
                case FighterState.Staggered:
                    _blockRemaining = 0f;
                    SetState(InmateState.Stagger);
                    return new InmateDecision(s_ready, 0f, 0f, windUpScale);
                case FighterState.WindUp:
                    // He keeps turning and stepping in while winding up, then commits to the swing.
                    SetState(InmateState.WindUp);
                    bool stepIn = perception.TargetAvailable && perception.DistanceToTarget > PreferredDistance;
                    return new InmateDecision(s_ready, stepIn ? WindUpStepSpeed : 0f, WindUpTurnSpeed, windUpScale);
                case FighterState.Active:
                    SetState(InmateState.Attack);
                    return new InmateDecision(s_ready, 0f, 0f, windUpScale);
                case FighterState.Recovery:
                    SetState(InmateState.Recover);
                    return new InmateDecision(s_ready, 0f, 0f, windUpScale);
            }

            if (wasHit && _random.NextDouble() < BlockChance)
            {
                _blockRemaining = BlockSeconds;
            }
            if (!perception.TargetAvailable)
            {
                _blockRemaining = 0f;
                SetState(InmateState.Ready);
                return new InmateDecision(s_ready, 0f, TurnSpeed, windUpScale);
            }
            // Separate enter and leave distances stop him flipping between approaching and attacking.
            if (_inRange && perception.DistanceToTarget > ChaseRange)
            {
                _inRange = false;
            }
            else if (!_inRange && perception.DistanceToTarget <= AttackRange)
            {
                _inRange = true;
            }
            if (_blockRemaining > 0f)
            {
                SetState(InmateState.Block);
                return new InmateDecision(s_block, 0f, TurnSpeed, windUpScale);
            }
            if (!_inRange)
            {
                SetState(InmateState.Approach);
                return new InmateDecision(s_ready, ApproachSpeed, TurnSpeed, windUpScale);
            }
            if (_nextAttackIn <= 0f)
            {
                // The interval runs from the start of one attack to the start of the next.
                _nextAttackIn = Range(MinAttackInterval, MaxAttackInterval);
                AttacksStarted++;
                SetState(InmateState.WindUp);
                return new InmateDecision(s_punch, 0f, TurnSpeed, windUpScale);
            }
            SetState(InmateState.Ready);
            return new InmateDecision(s_ready, 0f, TurnSpeed, windUpScale);
        }

        // After a checkpoint restore: back to idle, and the same random sequence as at the start.
        public void Reset()
        {
            Disengage();
            _random = new Random(_seed);
            _hitSinceLastTick = false;
            SetState(InmateState.Idle);
        }

        private void Engage()
        {
            _engaged = true;
            _inRange = false;
            _blockRemaining = 0f;
            _nextAttackIn = Range(MinFirstAttackDelay, MaxFirstAttackDelay);
        }

        private void Disengage()
        {
            _engaged = false;
            _blockRemaining = 0f;
        }

        private float Range(float min, float max) => min + (float)_random.NextDouble() * (max - min);

        private void OnHitTaken(DamageInfo hit, DamageResult result)
        {
            if (!result.Ignored && result.Block != BlockOutcome.TimedBlock)
            {
                _hitSinceLastTick = true;
            }
        }

        private void SetState(InmateState next)
        {
            if (State == next)
            {
                return;
            }
            InmateState previous = State;
            State = next;
            Changed?.Invoke(previous, next);
        }
    }
}
