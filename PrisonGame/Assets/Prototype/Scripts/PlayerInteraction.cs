using UnityEngine;
using UnityEngine.InputSystem;

namespace PrisonGame.Prototype
{
    [RequireComponent(typeof(FirstPersonController))]
    public sealed class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private float reach = 2.4f;
        private Camera view;
        private FirstPersonController movement;
        private string message;
        private float messageUntil;
        private GUIStyle textStyle;
        public PrototypePickup HeldItem { get; private set; }
        public PrototypeInteractable Target { get; private set; }

        private void Awake() { view = GetComponentInChildren<Camera>(); movement = GetComponent<FirstPersonController>(); }

        private void Update()
        {
            if (view == null || !movement.ControlsActive) { Target = null; return; }
            if(GetComponent<LaundryHud>()?.InventoryVisible==true){Target=null;return;}
            FindTarget();
            if (Keyboard.current == null) return;
            if (Keyboard.current.rKey.wasPressedThisFrame) GetComponent<PlayerInventory>()?.PocketHeld(this);
            if (Keyboard.current.qKey.wasPressedThisFrame && HeldItem != null) TryPutDown();
            if (Keyboard.current.eKey.wasPressedThisFrame && Target != null) Target.Interact(this);
        }

        private void FindTarget()
        {
            Target = null;
            // A single first-hit ray prevents interactions through walls.
            if (Physics.Raycast(view.transform.position, view.transform.forward, out RaycastHit hit, reach,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                Target = hit.collider.GetComponentInParent<PrototypeInteractable>();
        }

        public void ShowMessage(string text, float duration = 3f) { message = text; messageUntil = Time.unscaledTime + duration; GetComponent<LaundryHud>()?.Notify(text,duration); }

        public void ShowDialogue(string text, float duration = 8f)
        {
            var hud=GetComponent<LaundryHud>();
            if(hud!=null)hud.Speak(text,duration);
            else ShowMessage(text,duration);
        }

        public void PickUp(PrototypePickup item, bool playSound = true, bool restoring = false)
        {
            if(item==null || (item.PocketOwner!=null && item.PocketOwner!=this))return;
            if (HeldItem != null) { ShowMessage("Your hands are full. Press Q to put the carried item down."); return; }
            var inventory = GetComponent<PlayerInventory>();
            if (!restoring && inventory != null && !inventory.Carries(item) && !inventory.CanAdd()) { inventory.Full(); return; }
            HeldItem = item;
            item.Attach(view.transform);
            if (playSound) SampleSoundEvents.Emit(this, SampleSound.Pickup, transform.position);
            if(playSound) ShowMessage(GetComponent<LaundryHud>()!=null ? "Holding " + item.ItemName : "Picked up " + item.ItemName + ". Press Q to put it down.");
        }

        internal void ReleaseHeldForPocket(PrototypePickup expected)
        { if (HeldItem == expected) HeldItem = null; }

        internal void ClearForRestore()
        {
            if (HeldItem != null) HeldItem.Place(HeldItem.transform.position);
            HeldItem = null; Target = null; message = null; messageUntil = 0; GetComponent<LaundryHud>()?.ClearNotices();
        }

        public bool ConsumeHeldItem(PrototypePickup expected)
        {
            if (expected == null || HeldItem != expected) return false;
            HeldItem = null;
            expected.gameObject.SetActive(false);
            Destroy(expected.gameObject);
            return true;
        }

        public bool TryPutDown()
        {
            if (HeldItem == null) return false;
            Vector3 half = HeldItem.HalfSize;
            RaycastHit support;
            bool surface = Physics.Raycast(view.transform.position, view.transform.forward, out support, reach,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) && support.normal.y > .75f;
            if (!surface)
            {
                Vector3 ahead = transform.position + transform.forward * .9f + Vector3.up * 1.8f;
                surface = Physics.Raycast(ahead, Vector3.down, out support, 2.2f,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) && support.normal.y > .75f;
            }
            if (!surface) { ShowMessage("Look at a clear floor or tabletop to put this down."); return false; }
            Vector3 position = support.point + Vector3.up * (half.y + .03f);
            return TryPlaceHeldAt(position);
        }

        public bool TryPlaceHeldAt(Vector3 position)
        {
            if (HeldItem == null) return false;
            Vector3 half = HeldItem.HalfSize;
            Vector3 travel = position - view.transform.position;
            if (travel.magnitude > reach) { ShowMessage("Step closer to place the item."); return false; }
            // Do not place across a wall, even if the downward support probe found a floor beyond it.
            bool blocked = Physics.Raycast(view.transform.position, travel.normalized, travel.magnitude - .02f,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            if (!blocked)
                foreach (Collider other in Physics.OverlapBox(position, half + Vector3.one * .01f, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore))
                    if (!other.transform.IsChildOf(HeldItem.transform)) { blocked = true; break; }
            if (blocked) { ShowMessage("Not enough space here. Step back or aim at a clear surface."); return false; }
            var item = HeldItem;
            HeldItem = null;
            item.Place(position);
            SampleSoundEvents.Emit(this, SampleSound.Place, position);
            ShowMessage("Put down " + item.ItemName + ".");
            return true;
        }

        private void OnGUI()
        {
            if (movement == null || !movement.ControlsActive) return;
            var modern=GetComponent<LaundryHud>(); if(modern!=null && (modern.Focused || modern.InventoryVisible))return;
            if (textStyle == null) textStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            Matrix4x4 old = GUI.matrix;
            float scale = Mathf.Clamp(Screen.height / 900f, .75f, 2.5f);
            GUI.matrix = Matrix4x4.Scale(Vector3.one * scale);
            float width = Screen.width / scale;
            float height = Screen.height / scale;
            if (Target != null) Draw(Target.Prompt(this), width, height / 2 + 38, 45);
            if (HeldItem != null) Draw("Carrying: " + HeldItem.ItemName + "   |   Q - Put down", width, height - 102, 40);
            if (modern==null && Time.unscaledTime < messageUntil) Draw(message, width, 28, 75);
            GUI.matrix = old;
        }

        private void Draw(string text, float screenWidth, float y, float height)
        {
            float width = Mathf.Min(650, screenWidth - 30);
            var rect = new Rect((screenWidth - width) / 2, y, width, height);
            GUI.Box(rect, GUIContent.none);
            GUI.Label(new Rect(rect.x + 12, rect.y, rect.width - 24, rect.height), text, textStyle);
        }
    }
}
