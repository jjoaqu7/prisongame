using System;

namespace PrisonGame.Combat
{
    // Resolves a hit that has landed on one defender: block, timed block, damage and stagger build-up.
    // The attacker's hit check decides that a hit landed; this decides what it does. Plain C#.
    // Agreed rules: a blocked hit does a quarter of its damage; a block raised within about 0.25 s
    // before the hit is a timed block (no damage, attacker staggers); below about a third of his
    // health a fighter is hurt and easier to stagger. Raising the block again soon after lowering
    // it gives a normal block, so spamming the button cannot make every block a timed one.
    public sealed class FighterDefense
    {
        public const float BlockedDamageFactor = 0.25f;
        public const float TimedBlockWindowSeconds = 0.25f;
        public const float BlockSpamSeconds = 0.4f;
        public const float StaggerSeconds = 0.8f;
        public const float TimedBlockStaggerSeconds = 1f;
        public const float HurtFraction = 1f / 3f;
        public const float HurtStaggerMultiplier = 1.5f;
        private readonly Health _health;
        private readonly FighterStateMachine _state;
        private float _sinceBlockLowered = float.PositiveInfinity;
        private bool _blockMayBeTimed;

        public FighterDefense(Health health, FighterStateMachine state, bool canTimedBlock)
        {
            _health = health ?? throw new ArgumentNullException(nameof(health));
            _state = state ?? throw new ArgumentNullException(nameof(state));
            CanTimedBlock = canTimedBlock;
            _state.Changed += OnStateChanged;
        }

        public bool CanTimedBlock { get; }
        public StaggerMeter Stagger { get; } = new StaggerMeter();
        public bool IsHurt => _health.IsAlive && _health.Current <= _health.Max * HurtFraction;

        // Raised for every hit that reaches a living defender, including timed blocks, which do no damage.
        public event Action<DamageInfo, DamageResult> HitTaken;

        public void Tick(float deltaSeconds)
        {
            Stagger.Tick(deltaSeconds);
            if (_state.State != FighterState.Blocking)
            {
                _sinceBlockLowered += deltaSeconds;
            }
        }

        public BlockOutcome Evaluate()
        {
            if (_state.State != FighterState.Blocking)
            {
                return BlockOutcome.None;
            }
            return _blockMayBeTimed && _state.TimeInState <= TimedBlockWindowSeconds
                ? BlockOutcome.TimedBlock
                : BlockOutcome.Blocked;
        }

        public DamageResult Receive(in DamageInfo hit)
        {
            if (!(hit.Amount >= 0f) || float.IsInfinity(hit.Amount))
            {
                throw new ArgumentOutOfRangeException(nameof(hit), "Damage must be zero or positive and finite.");
            }
            if (!_health.IsAlive)
            {
                return DamageResult.IgnoredHit;
            }
            BlockOutcome block = Evaluate();
            DamageResult result;
            if (block == BlockOutcome.TimedBlock)
            {
                result = new DamageResult(0f, false, false, BlockOutcome.TimedBlock);
            }
            else
            {
                float amount = block == BlockOutcome.Blocked ? hit.Amount * BlockedDamageFactor : hit.Amount;
                result = _health.Apply(hit.WithAmount(amount), block);
            }
            // Blocked hits still build stagger, so a guard can be broken; a kick fills the meter at once.
            if (block != BlockOutcome.TimedBlock && !result.WentDown && hit.StaggerPower > 0f)
            {
                float power = hit.StaggerPower * (IsHurt ? HurtStaggerMultiplier : 1f);
                if (Stagger.Add(power))
                {
                    _state.Stagger(StaggerSeconds);
                }
            }
            HitTaken?.Invoke(hit, result);
            return result;
        }

        public void Reset()
        {
            Stagger.Reset();
            _sinceBlockLowered = float.PositiveInfinity;
            _blockMayBeTimed = false;
        }

        private void OnStateChanged(FighterState from, FighterState to)
        {
            if (to == FighterState.Blocking)
            {
                _blockMayBeTimed = CanTimedBlock && _sinceBlockLowered >= BlockSpamSeconds;
            }
            else if (from == FighterState.Blocking)
            {
                _sinceBlockLowered = 0f;
            }
        }
    }
}
