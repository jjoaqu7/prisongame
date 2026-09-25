using UnityEngine;

namespace PrisonGame.Prototype
{
    public sealed class SnackShelf : PrototypeInteractable
    {
        [SerializeField] private GameObject shelfVisual;
        [SerializeField] private Transform[] slots;
        public bool Installed { get; private set; }

        internal void Restore(bool installed) { Installed = installed; shelfVisual.SetActive(installed); }

        public override string Prompt(PlayerInteraction actor)
        {
            if (!Installed) return "E - Buy cell shelf ($" + SnackSupplies.ShelfPrice + ")";
            return actor.HeldItem != null ? "E - Store carried item on shelf" : "Cell shelf installed - aim at an item to pick it up";
        }

        public override void Interact(PlayerInteraction actor)
        {
            if (actor == null) return;
            if (!Installed)
            {
                var stock = actor.GetComponent<SnackSupplies>();
                if (stock == null || !stock.Spend(SnackSupplies.ShelfPrice))
                {
                    actor.ShowMessage("The shelf costs $" + SnackSupplies.ShelfPrice + ". Sell more snack packs to M5.");
                    return;
                }
                Installed = true;
                SampleSoundEvents.Emit(actor, SampleSound.Shelf, transform.position);
                shelfVisual.SetActive(true);
                actor.ShowMessage("Cell shelf installed. Carry an item here and press E to store it.", 6f);
                return;
            }
            if (actor.HeldItem == null) { actor.ShowMessage("Your shelf is ready. Bring an item to store."); return; }
            foreach (var slot in slots)
            {
                Vector3 point = slot.position + Vector3.up * (actor.HeldItem.HalfSize.y + .03f);
                bool occupied = false;
                foreach (var other in Physics.OverlapBox(point, actor.HeldItem.HalfSize + Vector3.one * .01f,
                    Quaternion.identity, ~0, QueryTriggerInteraction.Ignore))
                    if (!other.transform.IsChildOf(actor.HeldItem.transform)) { occupied = true; break; }
                if (!occupied && actor.TryPlaceHeldAt(point)) return;
            }
            actor.ShowMessage("No clear shelf space. Pick up a stored item first.");
        }
    }
}
