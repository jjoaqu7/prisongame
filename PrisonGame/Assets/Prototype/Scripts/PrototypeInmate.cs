using UnityEngine;

namespace PrisonGame.Prototype
{
    public sealed class PrototypeInmate : PrototypeInteractable
    {
        public event System.Action<PlayerInteraction> Talked;

        public override string Prompt(PlayerInteraction player)
        {
            var buyer = GetComponent<SnackPackBuyer>();
            return buyer != null ? buyer.Prompt(player) : "E - Talk to inmate";
        }
        public override void Interact(PlayerInteraction player)
        {
            if (player == null) return;
            var buyer = GetComponent<SnackPackBuyer>();
            if (buyer != null) buyer.Interact(player);
            else player.ShowDialogue(player.HeldItem != null
                ? "Inmate: Nice parcel. Around here, even the wrapping has a price."
                : "Inmate: Welcome. The table is communal. The opinions are mandatory.", 6f);
            Talked?.Invoke(player);
        }
    }
}
