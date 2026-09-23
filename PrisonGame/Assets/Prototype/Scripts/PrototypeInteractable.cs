using UnityEngine;

namespace PrisonGame.Prototype
{
    public abstract class PrototypeInteractable : MonoBehaviour
    {
        public abstract string Prompt(PlayerInteraction player);
        public abstract void Interact(PlayerInteraction player);
    }
}
