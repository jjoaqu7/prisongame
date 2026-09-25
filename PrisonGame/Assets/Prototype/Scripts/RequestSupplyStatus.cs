using UnityEngine;

namespace PrisonGame.Prototype
{
    // Derive this player's dependency on the closed supplier from current stock and work.
    [RequireComponent(typeof(SnackSupplies), typeof(SnackRequest), typeof(PlayerInteraction))]
    public sealed class RequestSupplyStatus : MonoBehaviour
    {
        [SerializeField] private SupplyInspection inspection;
        [SerializeField] private SnackAssembly assembly;
        public SupplyInspection Inspection => inspection;

        public int CraftablePacks
        {
            get
            {
                var stock = GetComponent<SnackSupplies>();
                int crackers = stock.Crackers, fruit = stock.Fruit, wraps = stock.Wrappers, partial = 0;
                if (assembly != null && assembly.WorkingPlayer == GetComponent<PlayerInteraction>())
                {
                    if (assembly.Stage == 1 && fruit > 0 && wraps > 0) { partial = 1; fruit--; wraps--; }
                    else if (assembly.Stage == 2 && wraps > 0) { partial = 1; wraps--; }
                }
                return partial + Mathf.Min(crackers, fruit, wraps);
            }
        }

        public int PacksNeedingSupplies
        {
            get
            {
                var request = GetComponent<SnackRequest>();
                return request.Active ? Mathf.Max(0, request.Remaining - request.ReadyPacks - CraftablePacks) : 0;
            }
        }

        public bool RestockBlocked => inspection != null && inspection.Closed && PacksNeedingSupplies > 0;
    }
}
