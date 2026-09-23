using UnityEngine;

namespace PrisonGame.Prototype
{
    public sealed class PrototypeInmate : PrototypeInteractable
    {
        public override string Prompt(PlayerInteraction player) => "E - Talk to inmate";
        public override void Interact(PlayerInteraction player)
        {
            player.ShowMessage(player.HeldItem != null
                ? "Inmate: Nice parcel. Around here, even the wrapping has a price."
                : "Inmate: Welcome. The table is communal. The opinions are mandatory.", 6f);
        }
    }
}
