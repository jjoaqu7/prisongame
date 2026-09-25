using UnityEngine;

namespace PrisonGame.Prototype
{
    [RequireComponent(typeof(PrototypeInmate))]
    public sealed class SnackPackBuyer : MonoBehaviour
    {
        public string Prompt(PlayerInteraction actor)
        {
            var request = actor.GetComponent<SnackRequest>();
            if (request != null && request.Customer == this)
            {
                if (!request.Accepted) return request.Timed
                    ? "E - Accept: 3 packs in 1 game hour | $3 each, $2 if late"
                    : "E - Accept M5's request: 3 snack packs, $3 each, no deadline";
                if (request.Active && HasPack(actor)) return "E - Deliver snack pack to M5 ($" + request.DeliveryPrice + ") | " + request.Delivered + "/3 delivered";
            }
            return HasPack(actor) ? "E - Sell snack pack to M5 ($" + SnackSupplies.SalePrice + ")" : "E - Talk to M5";
        }

        private static bool HasPack(PlayerInteraction actor) => actor.HeldItem != null && actor.HeldItem.GetComponent<SnackPackItem>() != null;

        public void Interact(PlayerInteraction actor)
        {
            if (actor == null) return;
            var stock = actor.GetComponent<SnackSupplies>();
            var request = actor.GetComponent<SnackRequest>();
            if (request != null && request.Customer == this && !request.Complete)
            {
                if (!request.Accepted)
                {
                    if (request.Accept(actor, this)) actor.ShowDialogue(request.Timed
                        ? "M5: Request accepted. 3 packs by " + PrisonClock.Format(request.DueAt) + ". $3 each before then; $2 for late packs."
                        : "M5: Bring me 3 snack packs. $3 for each delivery, $9 total. No deadline. Request accepted.", 6f);
                    return;
                }
                int payment = request.DeliveryPrice;
                if (request.Deliver(actor, this))
                    actor.ShowDialogue(request.Complete
                        ? "M5: That's all three. Request complete! +$" + payment + " ($" + request.Paid + " paid in total). You have $" + stock.Money + "."
                        : "M5: +$" + payment + " | " + request.Delivered + "/3 delivered. " + (request.IsLate ? "Deadline passed; late packs pay $2. " : "") + "You have $" + stock.Money + ".", 6f);
                else actor.ShowDialogue("M5: " + request.Delivered + "/3 delivered. Bring " + request.Remaining + " more snack pack(s); $" + request.DeliveryPrice + " each. " +
                    (request.Timed ? request.IsLate ? "The deadline passed; late packs pay $2." : "Due " + PrisonClock.Format(request.DueAt) + "." : "No deadline."), 6f);
                return;
            }
            if (stock != null && stock.SellHeldPack(actor))
                actor.ShowDialogue("Inmate: Good. Lunch needed a sequel. +$" + SnackSupplies.SalePrice +
                    "  |  You have $" + stock.Money + ".", 6f);
            else actor.ShowDialogue("Inmate: I buy snack packs for $" + SnackSupplies.SalePrice +
                " each. Supplies and an assembly tray are on the common table.", 6f);
        }
    }
}
