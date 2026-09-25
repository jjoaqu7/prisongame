using UnityEngine;

namespace PrisonGame.Prototype
{
    [RequireComponent(typeof(PrototypePickup))]
    public sealed class SnackPackItem : MonoBehaviour
    {
        public SnackSupplies Source { get; private set; }
        public bool Sold { get; private set; }
        public void Initialize(SnackSupplies source) => Source = source;
        public void MarkSold() => Sold = true;
    }
}
