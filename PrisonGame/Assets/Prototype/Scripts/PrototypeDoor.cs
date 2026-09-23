using UnityEngine;

namespace PrisonGame.Prototype
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class PrototypeDoor : PrototypeInteractable
    {
        [SerializeField] private Vector3 openOffset = new Vector3(0, 0, 1.5f);
        [SerializeField] private float speed = 2f;
        private Vector3 closedPosition;
        private bool opening;
        private BoxCollider doorCollider;
        public bool IsOpen => Vector3.Distance(transform.position, closedPosition + openOffset) < .01f;

        private void Awake()
        {
            closedPosition = transform.position;
            doorCollider = GetComponent<BoxCollider>();
        }

        public override string Prompt(PlayerInteraction player) => opening ? "E - Close cell door" : "E - Open cell door";

        public override void Interact(PlayerInteraction player)
        {
            if (opening && DoorwayOccupied())
            {
                player.ShowMessage("Doorway blocked. Move yourself or the parcel clear.");
                return;
            }
            opening = !opening;
        }

        private bool DoorwayOccupied()
        {
            Vector3 center = closedPosition + doorCollider.center;
            Vector3 half = doorCollider.size * .5f + Vector3.one * .08f;
            foreach (Collider other in Physics.OverlapBox(center, half, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore))
            {
                if (other.transform.IsChildOf(transform)) continue;
                if (other.GetComponentInParent<CharacterController>() != null || other.GetComponentInParent<PrototypePickup>() != null)
                    return true;
            }
            return false;
        }

        private void Update()
        {
            // Check throughout closing, so walking into a closing doorway reopens it.
            if (!opening && Vector3.Distance(transform.position, closedPosition) > .01f && DoorwayOccupied()) opening = true;
            transform.position = Vector3.MoveTowards(transform.position,
                closedPosition + (opening ? openOffset : Vector3.zero), speed * Time.deltaTime);
        }
    }
}
