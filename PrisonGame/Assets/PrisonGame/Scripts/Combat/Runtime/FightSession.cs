using System;
using PrisonGame.Core;

namespace PrisonGame.Combat
{
    public enum FightPhase
    {
        NotStarted,
        Active,
        Lost,
        Won,
    }

    public enum FightEnd
    {
        Won,
        LeftArea,
        Lost,
    }

    // What the scene reports about the fight each tick; the session never reads the scene itself.
    public readonly struct FightStatus
    {
        public FightStatus(bool challengerDown, bool opponentDown, bool challengerInArea)
        {
            ChallengerDown = challengerDown;
            OpponentDown = opponentDown;
            ChallengerInArea = challengerInArea;
        }

        public bool ChallengerDown { get; }
        public bool OpponentDown { get; }
        public bool ChallengerInArea { get; }
    }

    // One fight between a challenger (the player who started it) and an opponent in his area.
    // Agreed rules: the first hit or shove starts it; staying outside his cell for 10 s ends it with
    // both back to full health; losing returns to the checkpoint taken when the fight started, after
    // a fade; the player's health refills when a fight ends; the health bar shows during a fight and
    // briefly after. Plain C# with time passed in. Release 1 has one challenger; co-op would add more.
    public sealed class FightSession
    {
        public const float LeaveSeconds = 10f;
        public const float LossFadeSeconds = 1.2f;
        public const float HealthBarLingerSeconds = 3f;
        private float _sinceEnded = float.PositiveInfinity;

        public FightSession(StableId opponent)
        {
            if (!opponent.IsValid)
            {
                throw new ArgumentException("A fight needs an opponent with a stable ID.", nameof(opponent));
            }
            Opponent = opponent;
        }

        public StableId Opponent { get; }
        public StableId Challenger { get; private set; }
        public FightPhase Phase { get; private set; }
        public float TimeInPhase { get; private set; }
        public float TimeOutsideArea { get; private set; }
        public bool IsActive => Phase == FightPhase.Active;

        public bool ChallengerHealthVisible =>
            Phase == FightPhase.Active || Phase == FightPhase.Lost || _sinceEnded < HealthBarLingerSeconds;

        public event Action<StableId> Started;
        public event Action<FightEnd> Ended;

        // Raised when the loss fade has finished; the scene then restores the fight-start checkpoint.
        public event Action RestoreRequested;

        public bool TryStart(StableId challenger)
        {
            if (!challenger.IsValid || challenger == Opponent)
            {
                throw new ArgumentException("A fight is started by someone other than the opponent.", nameof(challenger));
            }
            if (Phase != FightPhase.NotStarted)
            {
                return false;
            }
            Challenger = challenger;
            TimeOutsideArea = 0f;
            _sinceEnded = float.PositiveInfinity;
            Enter(FightPhase.Active);
            Started?.Invoke(challenger);
            return true;
        }

        public void Tick(float deltaSeconds, FightStatus status)
        {
            if (!(deltaSeconds >= 0f) || float.IsInfinity(deltaSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds), "Tick time must be zero or positive and finite.");
            }
            TimeInPhase += deltaSeconds;
            _sinceEnded += deltaSeconds;
            switch (Phase)
            {
                case FightPhase.Active:
                    TickActive(deltaSeconds, status);
                    break;
                case FightPhase.Lost:
                    if (TimeInPhase >= LossFadeSeconds)
                    {
                        // The checkpoint was taken before the fight, so after it the fight has not started.
                        Enter(FightPhase.NotStarted);
                        _sinceEnded = float.PositiveInfinity;
                        RestoreRequested?.Invoke();
                    }
                    break;
            }
        }

        private void TickActive(float deltaSeconds, FightStatus status)
        {
            if (status.ChallengerDown)
            {
                Enter(FightPhase.Lost);
                Ended?.Invoke(FightEnd.Lost);
                return;
            }
            if (status.OpponentDown)
            {
                End(FightPhase.Won, FightEnd.Won);
                return;
            }
            TimeOutsideArea = status.ChallengerInArea ? 0f : TimeOutsideArea + deltaSeconds;
            if (TimeOutsideArea >= LeaveSeconds)
            {
                End(FightPhase.NotStarted, FightEnd.LeftArea);
            }
        }

        private void End(FightPhase next, FightEnd reason)
        {
            Enter(next);
            _sinceEnded = 0f;
            Ended?.Invoke(reason);
        }

        private void Enter(FightPhase next)
        {
            Phase = next;
            TimeInPhase = 0f;
        }
    }
}
