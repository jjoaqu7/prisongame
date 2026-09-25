using PrisonGame.Core;
using UnityEngine;

namespace PrisonGame.Combat
{
    // Scene owner of one fighter's rules state. It does not know whether it is the
    // local player, an NPC or, later, a remote co-op player; input adapters and NPC
    // decisions talk to it the same way.
    [DisallowMultipleComponent]
    public sealed class Fighter : MonoBehaviour
    {
        [SerializeField] private string stableId;
        [SerializeField, Min(1f)] private float maxHealth = 100f;

        public StableId Id { get; private set; }
        public Health Health { get; private set; }
        public FighterStateMachine State { get; private set; }

        private void Reset() => stableId = StableId.New().Value;

        private void Awake()
        {
            if (string.IsNullOrWhiteSpace(stableId))
            {
                Debug.LogError("Fighter needs a stable entity ID; generating a temporary one.", this);
                stableId = StableId.New().Value;
            }
            Id = new StableId(stableId);
            Health = new Health(maxHealth);
            State = new FighterStateMachine();
            Health.WentDown += _ => State.GoDown();
            Health.Killed += _ => State.Die();
        }

        private void Update() => State.Tick(Time.deltaTime);

        // Called by the combat rules after the attacker's hit check; a fighter never
        // decides for itself that it was hit.
        public DamageResult ApplyHit(in DamageInfo hit) => Health.Apply(hit);

        public bool Kill(StableId killer) => Health.Kill(killer);

        // A fight ended without a result: both sides recover.
        public void RecoverFully()
        {
            if (Health.RestoreFull()) State.Recover();
        }
    }
}
