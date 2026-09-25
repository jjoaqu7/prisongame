using UnityEngine;

namespace PrisonGame.Prototype
{
    public sealed class SnackAssembly : PrototypeInteractable
    {
        [SerializeField] private GameObject packPrefab;
        [SerializeField] private GameObject crackersVisual;
        [SerializeField] private GameObject fruitVisual;
        public int Stage { get; private set; }
        public PlayerInteraction WorkingPlayer { get; private set; }
        public string NextStep => Stage == 0 ? "Add crackers" : Stage == 1 ? "Add dried fruit" : "Wrap snack pack";

        internal GameObject PackPrefab => packPrefab;
        internal void Restore(int stage, PlayerInteraction actor)
        {
            Stage = stage; WorkingPlayer = stage == 0 ? null : actor;
            if (crackersVisual != null) crackersVisual.SetActive(stage >= 1);
            if (fruitVisual != null) fruitVisual.SetActive(stage >= 2);
        }

        public override string Prompt(PlayerInteraction player)
        {
            if (WorkingPlayer != null && WorkingPlayer != player) return "Table is in use";
            if (player.HeldItem != null) return "Put down the carried item first (Q)";
            return "E - " + NextStep;
        }

        public override void Interact(PlayerInteraction player)
        {
            if (player == null) return;
            if (WorkingPlayer != null && WorkingPlayer != player) { player.ShowMessage("Someone is using this tray."); return; }
            if (player.HeldItem != null) { player.ShowMessage("Put down what you are carrying first (Q)."); return; }
            var supplies = player.GetComponent<SnackSupplies>();
            if (packPrefab == null || supplies == null) { player.ShowMessage("This table is not ready."); return; }
            var inventory = player.GetComponent<PlayerInventory>();
            // Finishing consumes a wrapper; its last unit frees a slot for the resulting pack.
            if (Stage == 2 && inventory != null && !inventory.CanAdd(supplies.Wrappers == 1 ? 0 : 1))
            { inventory.Full(); return; }
            if (!supplies.TakeIngredient(Stage))
            {
                var supplyStatus = player.GetComponent<RequestSupplyStatus>();
                player.ShowMessage(supplyStatus != null && supplyStatus.Inspection != null && supplyStatus.Inspection.Closed
                    ? "Missing ingredients. Supply box is under inspection until " + PrisonClock.Format(supplyStatus.Inspection.ReopensAt) + ". The deadline continues."
                    : "Collect supplies from the box beside the tray.");
                return;
            }
            WorkingPlayer = player;
            Stage++;
            SampleSoundEvents.Emit(player, Stage == 1 ? SampleSound.Crackers : Stage == 2 ? SampleSound.Fruit : SampleSound.Wrap, transform.position);
            if (Stage == 3)
            {
                var pack = Instantiate(packPrefab, transform.position + Vector3.up * .25f, Quaternion.identity);
                pack.GetComponent<SnackPackItem>().Initialize(supplies);
                player.PickUp(pack.GetComponent<PrototypePickup>(), false);
                var request = player.GetComponent<SnackRequest>();
                player.ShowMessage(request != null && !request.Accepted
                    ? "Snack pack ready. Talk to M5 to accept his request, then deliver the pack."
                    : "Snack pack ready. Carry it to M5 and press E to " + (request != null && request.Active ? "deliver" : "sell") + " for $" + (request != null && request.Active ? request.DeliveryPrice : SnackSupplies.SalePrice) + ".");
                Stage = 0;
                WorkingPlayer = null;
            }
            else player.ShowMessage(Stage == 1 ? "Crackers added. Next: add dried fruit." : "Fruit added. Next: wrap the pack.");
            if (crackersVisual != null) crackersVisual.SetActive(Stage >= 1);
            if (fruitVisual != null) fruitVisual.SetActive(Stage >= 2);
        }
    }
}
