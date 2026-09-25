using UnityEngine;

namespace PrisonGame.Prototype
{
    [RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
    public sealed class PrototypePickup : PrototypeInteractable
    {
        [SerializeField] private string itemName = "Parcel";
        private Rigidbody body;
        private BoxCollider itemCollider;
        public string ItemName => itemName;
        public PlayerInteraction PocketOwner { get; private set; }
        public Vector3 HalfSize => Vector3.Scale(GetComponent<BoxCollider>().size, transform.lossyScale) * .5f;

        private void Awake() { body = GetComponent<Rigidbody>(); itemCollider = GetComponent<BoxCollider>(); }
        public override string Prompt(PlayerInteraction player) => player.HeldItem == null ? "E - Pick up " + itemName : "Put down the carried item first (Q)";
        public override void Interact(PlayerInteraction player) => player.PickUp(this);

        public void Attach(Transform camera)
        {
            PocketOwner = null;
            SetVisible(true);
            body.isKinematic = true;
            itemCollider.enabled = false;
            transform.SetParent(camera, true);
            transform.localPosition = new Vector3(.28f, -.28f, .65f);
            transform.localRotation = Quaternion.identity;
        }

        public void Place(Vector3 position)
        {
            PocketOwner = null;
            SetVisible(true);
            transform.SetParent(null, true);
            transform.SetPositionAndRotation(position, Quaternion.identity);
            itemCollider.enabled = true;
            body.isKinematic = false;
            body.useGravity = true;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        internal void Pocket(PlayerInteraction owner)
        {
            PocketOwner = owner;
            body.isKinematic = true;
            itemCollider.enabled = false;
            transform.SetParent(owner.transform, false);
            transform.localPosition = Vector3.up;
            SetVisible(false);
        }
        void SetVisible(bool visible)
        { foreach (var renderer in GetComponentsInChildren<Renderer>()) renderer.enabled = visible; }
    }
}
