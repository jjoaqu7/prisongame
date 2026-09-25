using UnityEngine;

namespace PrisonGame.Prototype
{
    // One shared supply closure, deliberately triggered by the first sale in this study.
    public sealed class SupplyInspection : MonoBehaviour
    {
        public const int DurationMinutes = 10;
        [SerializeField] private PrisonClock clock;
        [SerializeField] private PlayerInteraction observedPlayer;
        [SerializeField] private GameObject closedSign;
        private SnackSupplies observedStock;
        public bool Started { get; private set; }
        public double ReopensAt { get; private set; }
        public bool Closed => isActiveAndEnabled && Started && clock != null && clock.TotalMinutes < ReopensAt;
        public double MinutesLeft => Closed ? System.Math.Max(0, ReopensAt - clock.TotalMinutes) : 0;

        internal void Restore(bool started, double reopensAt)
        { Started = started; ReopensAt = reopensAt; RefreshSign(); }

        private void OnEnable()
        {
            observedStock = observedPlayer != null ? observedPlayer.GetComponent<SnackSupplies>() : null;
            if (observedStock != null) observedStock.PackSold += OnSale;
        }

        private void OnDisable()
        {
            if (observedStock != null) observedStock.PackSold -= OnSale;
            if (closedSign != null) closedSign.SetActive(false);
        }

        private void OnSale(PlayerInteraction actor) => Begin(actor);

        public bool Begin(PlayerInteraction actor)
        {
            if (!isActiveAndEnabled || Started || clock == null || actor == null || actor != observedPlayer) return false;
            Started = true;
            ReopensAt = clock.TotalMinutes + DurationMinutes;
            RefreshSign();
            return true;
        }

        private void Update() => RefreshSign();
        private void RefreshSign()
        {
            if (closedSign != null && closedSign.activeSelf != Closed) closedSign.SetActive(Closed);
        }
    }
}
