using System.Collections.Generic;
using UnityEngine;

namespace PrisonGame.Prototype
{
    // Personal carrying rules. Input and presentation live on the local player/HUD.
    public sealed class PlayerInventory : MonoBehaviour
    {
        public const int Capacity = 6;
        public sealed class Entry
        {
            public string name, detail;
            public int count;
            public PrototypePickup item;
        }
        PlayerInteraction owner;
        SnackSupplies supplies;
        LaundryDuty duty;
        void Awake() { owner = GetComponent<PlayerInteraction>(); supplies = GetComponent<SnackSupplies>(); duty = GetComponent<LaundryDuty>(); }

        public List<Entry> Entries
        {
            get
            {
                var result = new List<Entry>();
                if (duty.Data.hasTool) result.Add(new Entry { name = "Valve key", count = 1, detail = "Give to Rue with E while nearby." });
                if (duty.Data.hasOfficerKey) result.Add(new Entry { name = "Harris's key", count = 1, detail = "Opens the cell next to yours. Use E at its door." });
                AddStock(result, "Crackers", supplies.Crackers);
                AddStock(result, "Fruit", supplies.Fruit);
                AddStock(result, "Wrappers", supplies.Wrappers);
                foreach (var item in FindObjectsByType<PrototypePickup>(FindObjectsSortMode.InstanceID))
                    if (item.PocketOwner == owner || owner.HeldItem == item)
                        result.Add(new Entry { name = item.ItemName, count = 1, item = item,
                            detail = owner.HeldItem == item ? "In hand. R to pocket. Q to put down." : "In pockets. F to hold. Q to put down." });
                return result;
            }
        }
        static void AddStock(List<Entry> rows, string name, int count)
        { if (count > 0) rows.Add(new Entry { name = name, count = count, detail = "Used from pockets at the assembly tray." }); }
        public int Used => Entries.Count;
        public bool CanAdd(int slots = 1) => slots >= 0 && Used + slots <= Capacity;
        public int BatchSlots => (supplies.Crackers == 0 ? 1 : 0) + (supplies.Fruit == 0 ? 1 : 0) + (supplies.Wrappers == 0 ? 1 : 0);
        public void Full() => owner.ShowMessage("Inventory full - 6 slots. Put an item down or use supplies first.", 4);
        public bool Carries(PrototypePickup item) => item != null && (item.PocketOwner == owner || owner.HeldItem == item);

        public bool PocketHeld(PlayerInteraction actor)
        {
            if (actor != owner || owner.HeldItem == null) return false;
            var item = owner.HeldItem;
            if (!CanPocket(item)) { owner.ShowMessage("This item cannot go in your pockets."); return false; }
            // Moving between hand and pockets does not consume another slot.
            owner.ReleaseHeldForPocket(item);
            item.Pocket(owner);
            owner.ShowMessage("Pocketed " + item.ItemName);
            return true;
        }
        public static bool CanPocket(PrototypePickup item) => item != null &&
            (item.GetComponent<SnackPackItem>() != null || item.ItemName == "Parcel");
        public bool Hold(PlayerInteraction actor, PrototypePickup item)
        {
            if (actor != owner || item == null || item.PocketOwner != owner) return false;
            if (owner.HeldItem != null) { owner.ShowMessage("Pocket or put down the held item first."); return false; }
            owner.PickUp(item);
            return owner.HeldItem == item;
        }
        public bool Drop(PlayerInteraction actor, PrototypePickup item)
        {
            if (actor != owner || !Carries(item)) return false;
            bool wasPocketed = item.PocketOwner == owner;
            if (wasPocketed && !Hold(actor, item)) return false;
            if (owner.TryPutDown()) return true;
            // A blocked placement must not change ownership or occupy the player's hand.
            if (wasPocketed) { owner.ReleaseHeldForPocket(item); item.Pocket(owner); }
            return false;
        }
    }
}
