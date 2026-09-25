using UnityEngine;

namespace PrisonGame.Prototype
{
    [RequireComponent(typeof(SnackSupplies), typeof(PlayerInteraction))]
    public sealed class SnackLoopHud : MonoBehaviour
    {
        [SerializeField] private SnackAssembly assembly;
        [SerializeField] private SnackShelf shelf;
        private SnackSupplies stock;
        private PlayerInteraction actor;
        private FirstPersonController movement;
        private SnackRequest request;
        private GUIStyle titleStyle, bodyStyle;
        private bool warnedSoon, warnedLate;
        private RequestSupplyStatus supplyStatus;
        private bool announcedClosure, announcedReopening;

        private void Awake()
        {
            stock = GetComponent<SnackSupplies>();
            actor = GetComponent<PlayerInteraction>();
            movement = GetComponent<FirstPersonController>();
            request = GetComponent<SnackRequest>();
            supplyStatus = GetComponent<RequestSupplyStatus>();
        }

        internal void ResetAfterLoad()
        {
            warnedSoon = request != null && request.DueSoon;
            warnedLate = request != null && request.IsLate;
            var inspection = supplyStatus != null ? supplyStatus.Inspection : null;
            announcedClosure = inspection != null && inspection.Started;
            announcedReopening = inspection != null && inspection.Started && !inspection.Closed;
        }

        private void Update()
        {
            var inspection = supplyStatus != null ? supplyStatus.Inspection : null;
            if (inspection != null && inspection.Closed && !announcedClosure)
            {
                announcedClosure = true;
                actor.ShowMessage("Supply box closed: routine stock inspection. Reopens " + PrisonClock.Format(inspection.ReopensAt) + ". Your deadline continues.", 6f);
            }
            else if (inspection != null && inspection.Started && !inspection.Closed && !announcedReopening)
            {
                announcedReopening = true;
                actor.ShowMessage("Stock inspection finished. The supply box is open again; restocking is available.", 6f);
            }
            if (request == null || !request.Active || !request.Timed) return;
            if (request.IsLate && !warnedLate)
            {
                warnedLate = true;
                actor.ShowMessage("M5's deadline passed. Remaining packs now pay $2 each. Earlier earnings stay yours.", 6f);
            }
            else if (request.DueSoon && !warnedSoon)
            {
                warnedSoon = true;
                actor.ShowMessage("M5's request is due soon: 10 game minutes or less remain. Late packs pay $2 each.", 6f);
            }
        }

        public string NextAction
        {
            get
            {
                if (request != null && !request.Accepted) return "Meet M5 in the common room. Look at him to read and accept his request with E.";
                if (!stock.StarterCollected) return SupplyClosed ? WaitForSupplies : "Collect free starter supplies from the box on the common table.";
                if (actor.HeldItem != null && actor.HeldItem.GetComponent<SnackPackItem>() != null)
                    return request != null && request.Active ? "Carry this pack to M5. Press E to deliver for $" + request.DeliveryPrice + "." : "Carry this pack to M5. Press E to sell for $" + SnackSupplies.SalePrice + ".";
                if (request == null || !request.Active)
                {
                    if (shelf.Installed) return "Shelf earned! Store an item in your cell, or keep making packs.";
                    if (stock.Money >= SnackSupplies.ShelfPrice) return "Return to your cell and buy the shelf for $" + SnackSupplies.ShelfPrice + ".";
                }
                if (actor.HeldItem != null) return "Put down the carried item (Q), then use the assembly tray.";
                if (assembly.WorkingPlayer == actor)
                {
                    bool hasNext = assembly.Stage == 1 ? stock.Fruit > 0 : stock.Wrappers > 0;
                    if (hasNext) return "Return to the assembly tray: " + assembly.NextStep.ToLowerInvariant() + ".";
                    return SupplyClosed ? WaitForSupplies : "Restock missing ingredients at the supply box.";
                }
                if (stock.Crackers > 0 && stock.Fruit > 0 && stock.Wrappers > 0) return "Use the tray on the common table to assemble a snack pack.";
                if (stock.HasOutstandingPack) return "Pick up your finished pack from where you left it, then " + (request != null && request.Active ? "deliver" : "sell") + " to M5.";
                if (SupplyClosed) return WaitForSupplies;
                return stock.CanRecover ? "Collect a free recovery batch from the supply box." : "Restock at the supply box: $" + SnackSupplies.RestockPrice + " for one pack's supplies.";
            }
        }

        private bool SupplyClosed => supplyStatus != null && supplyStatus.Inspection != null && supplyStatus.Inspection.Closed;
        private string WaitForSupplies => "Supply inspection: restock when the box reopens at " + PrisonClock.Format(supplyStatus.Inspection.ReopensAt) + ". The deadline continues.";
        public string SupplyStatus => !SupplyClosed ? "Stock inspection finished. The supply box is open again."
            : "Cause: routine stock inspection.\nReopens: " + PrisonClock.Format(supplyStatus.Inspection.ReopensAt) +
              "\nLeft: " + PrisonClock.Remaining(supplyStatus.Inspection.MinutesLeft) + " (game time)\nUse ingredients or finished packs you already have.";

        public string RequestStatus
        {
            get
            {
                if (request == null) return "";
                if (!request.Accepted) return request.Timed
                    ? "M5 wants 3 snack packs.\n1 game hour from acceptance (5 real minutes).\n$3 each before the deadline; $2 each afterward.\nAccept by talking to M5."
                    : "M5 wants 3 snack packs.\n$3 per delivery ($9 total).\nNo deadline. Accept by talking to M5.";
                if (request.Complete) return "M5 received 3 / 3 snack packs.\n$" + request.Paid + " paid across three deliveries.\n" +
                    (request.Timed ? request.CompletedLate ? "Finished late; reduced payment applied.\n" : "Finished on time.\n" : "") + "You can keep selling packs to him.";
                string timing = !request.Timed ? "$3 per delivery ($9 total).\nNo deadline."
                    : request.IsLate ? "Deadline passed: " + PrisonClock.Format(request.DueAt) + "\nLate packs now pay $2 each.\nEarlier earnings are unchanged."
                    : "Due: " + PrisonClock.Format(request.DueAt) + "\nLeft: " + PrisonClock.Remaining(request.MinutesLeft) + " (game time)\n$3 each; late packs pay $2.";
                string supplyEffect = !SupplyClosed ? "" : supplyStatus.RestockBlocked
                    ? "\nRestock blocked: need supplies for " + supplyStatus.PacksNeedingSupplies + " more pack(s). Deadline continues."
                    : "\nBox closed; enough packs/supplies on hand. Deadline continues.";
                return "Delivered: " + request.Delivered + " / 3 to M5\nReady packs: " + request.ReadyPacks + " (carried or stored)\nStill to deliver: " + request.Remaining +
                    "\n" + timing + supplyEffect;
            }
        }

        private void OnGUI()
        {
            if (movement == null) return;
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold, wordWrap = true };
                bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true };
            }
            Matrix4x4 old = GUI.matrix;
            float scale = Mathf.Clamp(Screen.height / 900f, .75f, 2.5f);
            GUI.matrix = Matrix4x4.Scale(Vector3.one * scale);
            if (request != null && request.Timed)
            {
                float clockWidth = 270;
                var clockPanel = new Rect(Screen.width / scale - clockWidth - 18, 130, clockWidth, 76);
                GUI.Box(clockPanel, GUIContent.none);
                GUI.Label(new Rect(clockPanel.x + 12, clockPanel.y + 8, clockWidth - 24, 60),
                    "PRISON TIME" + (request.Clock.Paused ? " - PAUSED" : "") + "\n" + PrisonClock.Format(request.Clock.TotalMinutes) + "\n1 game minute = 5 real seconds", bodyStyle);
            }
            if (!movement.ControlsActive) { GUI.matrix = old; return; }
            float width = Mathf.Min(330, Screen.width / scale * .3f);
            var panel = new Rect(18, 130, width, 205);
            GUI.Box(panel, GUIContent.none);
            GUI.Label(new Rect(30, 140, width - 24, 28), shelf.Installed ? "YOUR CELL, IMPROVED" : "EARN A CELL SHELF", titleStyle);
            string moneyLine = shelf.Installed ? "Money: $" + stock.Money : "$" + stock.Money + " / $" + SnackSupplies.ShelfPrice + " shelf";
            GUI.Label(new Rect(30, 174, width - 24, 54), moneyLine + "  |  Packs sold: " + stock.Sales +
                "\nCrackers " + stock.Crackers + "  Fruit " + stock.Fruit + "  Wraps " + stock.Wrappers, bodyStyle);
            GUI.Label(new Rect(30, 239, width - 24, 86), NextAction, bodyStyle);
            float nextPanelY = panel.yMax + 12;
            if (request != null)
            {
                string heading = request.Complete ? "REQUEST COMPLETE" : request.IsLate ? "REQUEST OVERDUE" : request.DueSoon ? "REQUEST DUE SOON" : request.Accepted ? "M5'S SNACK REQUEST" : "REQUEST AVAILABLE";
                string status = RequestStatus;
                float textHeight = bodyStyle.CalcHeight(new GUIContent(status), width - 24);
                var requestPanel = new Rect(18, panel.yMax + 12, width, 50 + textHeight);
                GUI.Box(requestPanel, GUIContent.none);
                GUI.Label(new Rect(30, requestPanel.y + 8, width - 24, 28), heading, titleStyle);
                GUI.Label(new Rect(30, requestPanel.y + 40, width - 24, textHeight), status, bodyStyle);
                nextPanelY = requestPanel.yMax + 12;
            }
            if (supplyStatus != null && supplyStatus.Inspection != null && supplyStatus.Inspection.Started)
            {
                string status = SupplyStatus;
                float height = bodyStyle.CalcHeight(new GUIContent(status), width - 24);
                var supplyPanel = new Rect(18, nextPanelY, width, height + 50);
                GUI.Box(supplyPanel, GUIContent.none);
                GUI.Label(new Rect(30, nextPanelY + 8, width - 24, 28), SupplyClosed ? "SUPPLY BOX CLOSED" : "SUPPLY BOX REOPENED", titleStyle);
                GUI.Label(new Rect(30, nextPanelY + 40, width - 24, height), status, bodyStyle);
            }
            GUI.matrix = old;
        }
    }
}
