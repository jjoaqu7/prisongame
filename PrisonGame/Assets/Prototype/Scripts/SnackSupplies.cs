using UnityEngine;

namespace PrisonGame.Prototype
{
    // Personal ingredient stock. World stations receive the acting player explicitly.
    public sealed class SnackSupplies : MonoBehaviour
    {
        public const int SalePrice = 3;
        public const int RestockPrice = 1;
        public const int ShelfPrice = 8;
        public int Money { get; private set; }
        public int Sales { get; private set; }
        public int Crackers { get; private set; }
        public int Fruit { get; private set; }
        public int Wrappers { get; private set; }
        public bool StarterCollected { get; private set; }
        public event System.Action<PlayerInteraction> PackSold;

        internal void Restore(SampleSaveData.Personal s)
        {
            Money = s.money; Sales = s.sales; Crackers = s.crackers;
            Fruit = s.fruit; Wrappers = s.wrappers; StarterCollected = s.starter;
        }

        public bool CollectStarter()
        {
            if (StarterCollected) return false;
            if (!BatchFits) return false;
            StarterCollected = true;
            AddBatch(2);
            return true;
        }

        public bool BatchFits => GetComponent<PlayerInventory>() == null ||
            GetComponent<PlayerInventory>().CanAdd(GetComponent<PlayerInventory>().BatchSlots);

        private void AddBatch(int count)
        {
            if (count <= 0) return;
            Crackers += count;
            Fruit += count;
            Wrappers += count;
        }

        public bool HasOutstandingPack
        {
            get
            {
                foreach (var pack in FindObjectsByType<SnackPackItem>(FindObjectsSortMode.None))
                    if (pack.Source == this && !pack.Sold) return true;
                return false;
            }
        }

        public bool CanRecover => StarterCollected && Money == 0 && Crackers == 0 &&
            Fruit == 0 && Wrappers == 0 && !HasOutstandingPack;

        public bool Restock(out bool free)
        {
            free = CanRecover;
            if (!StarterCollected || Crackers >= 6 || Fruit >= 6 || Wrappers >= 6) return false;
            if (!BatchFits) return false;
            if (!free && !Spend(RestockPrice)) return false;
            AddBatch(1);
            return true;
        }

        public bool SellHeldPack(PlayerInteraction actor) => SellHeldPack(actor, SalePrice);

        internal bool SellHeldPack(PlayerInteraction actor, int price)
        {
            if (price <= 0 || price > SalePrice) return false;
            if (actor == null || actor.GetComponent<SnackSupplies>() != this || actor.HeldItem == null) return false;
            var item = actor.HeldItem;
            var pack = item.GetComponent<SnackPackItem>();
            if (pack == null || pack.Sold || !actor.ConsumeHeldItem(item)) return false;
            pack.MarkSold();
            Money += price;
            Sales++;
            PackSold?.Invoke(actor);
            SampleSoundEvents.Emit(actor, SampleSound.Sale, actor.transform.position);
            return true;
        }

        public bool Spend(int amount)
        {
            if (amount <= 0 || Money < amount) return false;
            Money -= amount;
            return true;
        }

        public bool TakeIngredient(int stage)
        {
            if (stage == 0 && Crackers > 0) { Crackers--; return true; }
            if (stage == 1 && Fruit > 0) { Fruit--; return true; }
            if (stage == 2 && Wrappers > 0) { Wrappers--; return true; }
            return false;
        }
    }
}
