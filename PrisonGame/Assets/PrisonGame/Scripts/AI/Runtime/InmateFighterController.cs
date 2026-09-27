using PrisonGame.Combat;
using UnityEngine;

namespace PrisonGame.AI
{
    // Scene adapter for the inmate's brain: tells it what he perceives, turns and moves him along the
    // floor without passing through walls or the player, and submits its intent to his Fighter the same
    // way the player's input adapter does. Runs before Fighter so the intent is used in the same frame.
    // The capsule stand-in slides; step 6 hands movement to the M5 inmate's own motion.
    [RequireComponent(typeof(Fighter), typeof(CapsuleCollider))]
    [DefaultExecutionOrder(40)]
    public sealed class InmateFighterController : MonoBehaviour
    {
        [SerializeField] private FightController _fight;
        [SerializeField] private int _seed = 1;
        private Fighter _fighter;
        private CapsuleCollider _capsule;
        private RaycastHit[] _obstacles = new RaycastHit[16];
        private bool _subscribed;

        public InmateBrain Brain { get; private set; }

        public void Configure(FightController fight, int seed)
        {
            _fight = fight;
            _seed = seed;
        }

        private void Awake()
        {
            _fighter = GetComponent<Fighter>();
            _capsule = GetComponent<CapsuleCollider>();
        }

        private void Start()
        {
            Brain = new InmateBrain(_fighter.State, _fighter.Defense, _fighter.Health, _seed);
            _fighter.Actions.Moveset = UnarmedMoveset.Inmate;
            Subscribe();
        }

        private void OnEnable() => Subscribe();

        private void Subscribe()
        {
            if (_subscribed || Brain == null || _fight == null)
            {
                return;
            }
            _fight.CheckpointRestored += Brain.Reset;
            _subscribed = true;
        }

        private void OnDisable()
        {
            if (_subscribed)
            {
                _fight.CheckpointRestored -= Brain.Reset;
                _subscribed = false;
            }
        }

        private void Update()
        {
            if (Brain == null || _fight == null || _fight.Session == null)
            {
                return;
            }
            Fighter target = _fight.Challenger;
            Vector3 toTarget = Vector3.ProjectOnPlane(target.transform.position - transform.position, Vector3.up);
            float distance = toTarget.magnitude;
            bool available = target.Health.IsAlive && _fight.IsInArea(target.transform.position);
            InmateDecision decision = Brain.Tick(Time.deltaTime,
                new InmatePerception(_fight.Session.IsActive, distance, available));
            _fighter.Actions.WindUpScale = decision.WindUpScale;
            // Hitstop freezes his body as well as his combat timeline.
            if (!_fighter.HitStop.IsActive && distance > 0.001f)
            {
                Vector3 direction = toTarget / distance;
                Turn(direction, decision.TurnDegreesPerSecond * Time.deltaTime);
                Move(direction, Mathf.Min(decision.MoveSpeed * Time.deltaTime, distance - InmateBrain.PreferredDistance));
            }
            _fighter.SubmitIntent(decision.Intent);
        }

        private void Turn(Vector3 direction, float maxDegrees)
        {
            if (maxDegrees <= 0f)
            {
                return;
            }
            Quaternion facing = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, facing, maxDegrees);
        }

        private void Move(Vector3 direction, float wanted)
        {
            if (wanted <= 0f)
            {
                return;
            }
            float distance = CapsuleClearance.Distance(_capsule, direction, wanted, _fighter, ref _obstacles);
            Vector3 next = transform.position + direction * distance;
            // He stays in his cell (agreed), even if an opening leads out of it.
            if (distance <= 0f || !_fight.IsInArea(next))
            {
                return;
            }
            transform.position = next;
            Physics.SyncTransforms();
        }
    }
}
