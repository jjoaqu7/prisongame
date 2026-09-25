using UnityEngine;

namespace PrisonGame.Prototype
{
    public sealed class SnackSupplyBox : PrototypeInteractable
    {
        [SerializeField] private SupplyInspection inspection;
        public bool Closed => inspection != null && inspection.Closed;
        public string ClosureMessage => "Supply box closed for a routine stock inspection. Reopens " +
            PrisonClock.Format(inspection.ReopensAt) + ". Use supplies you already have; the request deadline continues.";

        public override string Prompt(PlayerInteraction player)
        {
            if (Closed) return "Stock inspection - reopens " + PrisonClock.Format(inspection.ReopensAt);
            var supplies = player.GetComponent<SnackSupplies>();
            if (supplies == null) return "Supplies unavailable";
            if (!supplies.BatchFits) return "Inventory full - make room for supplies";
            if (!supplies.StarterCollected) return "E - Collect starter supplies (2 packs)";
            if (supplies.Crackers >= 6 || supplies.Fruit >= 6 || supplies.Wrappers >= 6) return "Supplies full (6 batches)";
            return supplies.CanRecover ? "E - Collect a free recovery batch" : "E - Restock 1 pack's supplies ($" + SnackSupplies.RestockPrice + ")";
        }

        public override void Interact(PlayerInteraction player)
        {
            if (player == null) return;
            if (Closed) { player.ShowMessage(ClosureMessage, 6f); return; }
            var supplies = player.GetComponent<SnackSupplies>();
            if (supplies == null) return;
            if (!supplies.BatchFits) { player.GetComponent<PlayerInventory>()?.Full(); return; }
            if (supplies.CollectStarter())
            {
                player.ShowMessage("Supplies for two packs collected. Use the assembly tray beside this box.");
                SampleSoundEvents.Emit(player, SampleSound.Restock, transform.position);
            }
            else if (supplies.Restock(out bool free))
            {
                player.ShowMessage(free ? "One free batch to get you going again." : "Restocked one pack's supplies for $" + SnackSupplies.RestockPrice + ".");
                SampleSoundEvents.Emit(player, SampleSound.Restock, transform.position);
            }
            else player.ShowMessage(supplies.Money < SnackSupplies.RestockPrice
                ? "No money spent. Assemble or sell the stock you already have first."
                : "Supplies full. Use a batch before restocking.");
        }
    }
}
