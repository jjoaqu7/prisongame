using UnityEngine;

namespace PrisonGame.Prototype
{
    // One personal request. Rules receive the actor and recipient, never local input.
    [RequireComponent(typeof(SnackSupplies), typeof(PlayerInteraction))]
    public sealed class SnackRequest : MonoBehaviour
    {
        public const int Required = 3;
        [SerializeField] private SnackPackBuyer customer;
        [SerializeField] private PrisonClock clock;
        public const int DeadlineMinutes = 60;
        public const int LatePrice = 2;
        public PrisonClock Clock => clock;
        public bool Timed => clock != null;
        public double DueAt { get; private set; }
        public double MinutesLeft => Timed && Accepted ? System.Math.Max(0, DueAt - clock.TotalMinutes) : 0;
        public bool CompletedLate { get; private set; }
        public bool IsLate => Timed && Accepted && (Complete ? CompletedLate : clock.TotalMinutes >= DueAt);
        public bool DueSoon => Active && Timed && !IsLate && MinutesLeft <= 10;
        public int DeliveryPrice => IsLate ? LatePrice : SnackSupplies.SalePrice;
        public int Paid { get; private set; }
        public SnackPackBuyer Customer => customer;
        public bool Accepted { get; private set; }
        public int Delivered { get; private set; }
        public bool Complete => Accepted && Delivered == Required;
        public bool Active => Accepted && !Complete;
        public int Remaining => Required - Delivered;

        public int ReadyPacks
        {
            get
            {
                var stock = GetComponent<SnackSupplies>();
                int count = 0;
                foreach (var pack in FindObjectsByType<SnackPackItem>(FindObjectsSortMode.None))
                    if (!pack.Sold && pack.Source == stock) count++;
                return count;
            }
        }

        internal void Restore(SampleSaveData.Personal s)
        {
            Accepted = s.accepted; Delivered = s.delivered; Paid = s.paid;
            DueAt = s.dueAt; CompletedLate = s.completedLate;
        }

        public bool Accept(PlayerInteraction actor, SnackPackBuyer recipient)
        {
            if (!Owns(actor) || recipient == null || recipient != customer || Accepted) return false;
            Accepted = true;
            if (Timed) DueAt = clock.TotalMinutes + DeadlineMinutes;
            return true;
        }

        public bool Deliver(PlayerInteraction actor, SnackPackBuyer recipient)
        {
            if (!Owns(actor) || !Active || recipient == null || recipient != customer) return false;
            int price = DeliveryPrice;
            bool late = IsLate;
            if (!GetComponent<SnackSupplies>().SellHeldPack(actor, price)) return false;
            Paid += price;
            Delivered++;
            if (Complete) CompletedLate = late;
            return true;
        }

        private bool Owns(PlayerInteraction actor) => actor != null && actor.GetComponent<SnackRequest>() == this;
    }
}
