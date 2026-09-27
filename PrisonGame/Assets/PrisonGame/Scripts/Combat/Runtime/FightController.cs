using System;
using UnityEngine;

namespace PrisonGame.Combat
{
    // Scene owner of one fight. It watches the two fighters' hits, takes the fight-start checkpoint
    // just before the first hit, reports what happened to the FightSession rules, and applies their
    // outcomes: health refills and the checkpoint restore. The checkpoint lives in memory only; the
    // manual save slot is untouched (agreed). Step 7 extends it to the laundry scene's world state.
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(60)]
    public sealed class FightController : MonoBehaviour
    {
        [SerializeField] private Fighter _challenger;
        [SerializeField] private Fighter _opponent;

        // The opponent's cell. He stays inside it, and leaving it for too long ends the fight.
        [SerializeField] private Vector3 _areaCenter;
        [SerializeField] private Vector3 _areaSize = new Vector3(4f, 3f, 5f);
        private MeleeSweep _challengerSweep;
        private FighterSnapshot _challengerCheckpoint;
        private FighterSnapshot _opponentCheckpoint;

        public FightSession Session { get; private set; }
        public Fighter Challenger => _challenger;
        public Fighter Opponent => _opponent;
        public Bounds Area => new Bounds(_areaCenter, _areaSize);
        public bool HasCheckpoint { get; private set; }
        public event Action CheckpointRestored;

        public void Configure(Fighter challenger, Fighter opponent, Bounds area)
        {
            _challenger = challenger;
            _opponent = opponent;
            _areaCenter = area.center;
            _areaSize = area.size;
        }

        private void Start()
        {
            if (_challenger == null || _opponent == null)
            {
                Debug.LogError("FightController needs a challenger and an opponent.", this);
                enabled = false;
                return;
            }
            Session = new FightSession(_opponent.Id);
            Session.Ended += OnEnded;
            Session.RestoreRequested += RestoreCheckpoint;
            _challengerSweep = _challenger.GetComponent<MeleeSweep>();
            if (_challengerSweep != null)
            {
                _challengerSweep.HitLanding += OnHitLanding;
            }
        }

        private void OnDestroy()
        {
            if (_challengerSweep != null)
            {
                _challengerSweep.HitLanding -= OnHitLanding;
            }
            if (Session != null)
            {
                Session.Ended -= OnEnded;
                Session.RestoreRequested -= RestoreCheckpoint;
            }
        }

        // Only floor position counts, so jumping or crouching never takes the player "outside".
        public bool IsInArea(Vector3 position)
        {
            Bounds area = Area;
            return position.x >= area.min.x && position.x <= area.max.x
                && position.z >= area.min.z && position.z <= area.max.z;
        }

        private void Update()
        {
            if (Session == null)
            {
                return;
            }
            Session.Tick(Time.deltaTime, new FightStatus(
                !_challenger.Health.IsAlive, !_opponent.Health.IsAlive, IsInArea(_challenger.transform.position)));
        }

        private void OnHitLanding(Fighter target, DamageInfo hit)
        {
            if (Session.Phase != FightPhase.NotStarted || target != _opponent || !_opponent.Health.IsAlive)
            {
                return;
            }
            TakeCheckpoint();
            Session.TryStart(hit.Attacker);
        }

        public void TakeCheckpoint()
        {
            _challengerCheckpoint = FighterSnapshot.Capture(_challenger);
            _opponentCheckpoint = FighterSnapshot.Capture(_opponent);
            HasCheckpoint = true;
        }

        public void RestoreCheckpoint()
        {
            if (!HasCheckpoint)
            {
                return;
            }
            _challengerCheckpoint.Apply();
            _opponentCheckpoint.Apply();
            Physics.SyncTransforms();
            CheckpointRestored?.Invoke();
        }

        private void OnEnded(FightEnd reason)
        {
            switch (reason)
            {
                case FightEnd.Won:
                    _challenger.RecoverFully();
                    break;
                case FightEnd.LeftArea:
                    _challenger.RecoverFully();
                    _opponent.RecoverFully();
                    break;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.4f, 0.1f);
            Gizmos.DrawWireCube(_areaCenter, _areaSize);
        }

        private readonly struct FighterSnapshot
        {
            private readonly Fighter _fighter;
            private readonly Vector3 _position;
            private readonly Quaternion _rotation;
            private readonly float _health;
            private readonly LifeState _life;

            private FighterSnapshot(Fighter fighter)
            {
                _fighter = fighter;
                _position = fighter.transform.position;
                _rotation = fighter.transform.rotation;
                _health = fighter.Health.Current;
                _life = fighter.Health.State;
            }

            public static FighterSnapshot Capture(Fighter fighter) => new FighterSnapshot(fighter);

            public void Apply()
            {
                // A CharacterController overrides a direct move while enabled.
                var controller = _fighter.GetComponent<CharacterController>();
                bool wasEnabled = controller != null && controller.enabled;
                if (wasEnabled)
                {
                    controller.enabled = false;
                }
                _fighter.transform.SetPositionAndRotation(_position, _rotation);
                if (wasEnabled)
                {
                    controller.enabled = true;
                }
                _fighter.RestoreCheckpoint(_health, _life);
            }
        }
    }
}
