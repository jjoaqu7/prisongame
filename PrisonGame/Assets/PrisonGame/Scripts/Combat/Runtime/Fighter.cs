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

        public StableId Id { get; private set; }
        public Health Health { get; private set; }
        public FighterStateMachine State { get; private set; }
        public FighterActions Actions { get; private set; }
        public HitStop HitStop { get; } = new HitStop();
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
            Health.WentDown += _ => State.GoDown();
            Health.Killed += _ => State.Die();
        }

        private void Update()
        {
            // During hitstop the timeline receives no time, but intent is still read, so a press
            // made during the freeze is not lost.
            Actions.Tick(HitStop.Consume(Time.deltaTime), _intent);
            _intent = default;
        }

        public void SubmitIntent(CombatIntent intent) => _intent = intent;

        // Called by the combat rules after the attacker's hit check; a fighter never
        // decides for itself that it was hit.
        public DamageResult ApplyHit(in DamageInfo hit) => Health.Apply(hit);

        public bool Kill(StableId killer) => Health.Kill(killer);

        // A fight ended without a result: both sides recover.
        public void RecoverFully()
        {
            if (Health.RestoreFull())
            {
                State.Recover();
            }
        }
    }
}
