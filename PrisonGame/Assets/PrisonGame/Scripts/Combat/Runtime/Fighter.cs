using PrisonGame.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace PrisonGame.Combat
{
    // Scene owner of one fighter's rules state. It does not know whether it is the
    // local player, an NPC or, later, a remote co-op player; input adapters and NPC
    // decisions talk to it the same way.
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(50)]
    public sealed class Fighter : MonoBehaviour
    {
        [SerializeField]
        [FormerlySerializedAs("stableId")]
        private string _stableId;

        [SerializeField]
        [FormerlySerializedAs("maxHealth")]
        [Min(1f)]
        private float _maxHealth = 100f;

        // The player's timed block staggers the attacker. The inmate's blocks are never timed, so a
        // player punch that happens to land just after he raises his guard does not stagger the player.
        [SerializeField]
        private bool _canTimedBlock = true;

        public StableId Id { get; private set; }
        public Health Health { get; private set; }
        public FighterStateMachine State { get; private set; }
        public FighterActions Actions { get; private set; }
        public FighterDefense Defense { get; private set; }
        public HitStop HitStop { get; } = new HitStop();
        public bool IsHurt => Defense.IsHurt;
        private CombatIntent _intent;

        private void Reset()
        {
            _stableId = StableId.New().Value;
        }

        private void Awake()
        {
            if (string.IsNullOrWhiteSpace(_stableId))
            {
                Debug.LogError("Fighter needs a stable entity ID; generating a temporary one.", this);
                _stableId = StableId.New().Value;
            }
            Id = new StableId(_stableId);
            Health = new Health(_maxHealth);
            State = new FighterStateMachine();
            Actions = new FighterActions(Id, State);
            Defense = new FighterDefense(Health, State, _canTimedBlock);
            Health.WentDown += _ => State.GoDown();
            Health.Killed += _ => State.Die();
        }

        private void Update()
        {
            // During hitstop the timeline receives no time, but intent is still read, so a press
            // made during the freeze is not lost.
            float combatTime = HitStop.Consume(Time.deltaTime);
            Actions.Tick(combatTime, _intent);
            Defense.Tick(combatTime);
            _intent = default;
        }

        public void SubmitIntent(CombatIntent intent) => _intent = intent;

        // Called by the combat rules after the attacker's hit check; a fighter never
        // decides for itself that it was hit.
        public DamageResult ApplyHit(in DamageInfo hit) => Defense.Receive(hit);

        // This fighter's attack met a timed block.
        public void Parried() => State.Stagger(FighterDefense.TimedBlockStaggerSeconds);

        public bool Kill(StableId killer) => Health.Kill(killer);

        // A fight ended without a result, or the player won: recover to full health.
        public void RecoverFully()
        {
            if (Health.RestoreFull())
            {
                Actions.Cancel();
                State.Recover();
                Defense.Reset();
                HitStop.Clear();
            }
        }

        // Undoes a fight: health and life state as saved at the fight-start checkpoint, no action under way.
        public void RestoreCheckpoint(float health, LifeState life)
        {
            Health.Restore(health, life);
            Actions.Cancel();
            State.ResetTo(life);
            Defense.Reset();
            HitStop.Clear();
            _intent = default;
        }
    }
}
