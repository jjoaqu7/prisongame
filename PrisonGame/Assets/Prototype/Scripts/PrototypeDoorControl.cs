using UnityEngine;

namespace PrisonGame.Prototype
{
    public sealed class PrototypeDoorControl : PrototypeInteractable
    {
        [SerializeField] private PrototypeDoor door;
        public override string Prompt(PlayerInteraction player) => door != null ? door.Prompt(player) : "Door unavailable";
        public override void Interact(PlayerInteraction player) { if (door != null) door.Interact(player); }
    }
}
