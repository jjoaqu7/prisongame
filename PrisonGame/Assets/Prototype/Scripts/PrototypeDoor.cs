using UnityEngine;

namespace PrisonGame.Prototype
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class PrototypeDoor : PrototypeInteractable
    {
        [SerializeField] private Vector3 openOffset = new Vector3(0, 0, 1.5f);
        [SerializeField] private float speed = 2f;
        [SerializeField] private bool requiresOfficerKey;
        private bool unlocked;
        private Vector3 closedPosition;
        private bool opening;
        private BoxCollider doorCollider;
        public bool IsOpen => Vector3.Distance(transform.position, closedPosition + openOffset) < .01f;
        public bool Locked => requiresOfficerKey && !unlocked;
        public bool RequiresOfficerKey => requiresOfficerKey;

        internal SampleSaveData.Door Capture() => new SampleSaveData.Door { position = transform.position, opening = opening, unlocked = unlocked };
        internal void Restore(SampleSaveData.Door state)
        { transform.position = state!=null?state.position:closedPosition; opening = state!=null&&state.opening; unlocked = state!=null&&state.unlocked; }
        internal bool ValidState(SampleSaveData.Door state)
        {
            if(state==null)return true; // Before this room existed, it starts closed and locked.
            var delta=state.position-closedPosition;
            if(!float.IsFinite(delta.sqrMagnitude))return false;
            float along=openOffset.sqrMagnitude>0?Vector3.Dot(delta,openOffset)/openOffset.sqrMagnitude:0;
            if(along<-.001f||along>1.001f||(delta-openOffset*along).sqrMagnitude>.0001f)return false;
            return !requiresOfficerKey || state.unlocked || (!state.opening && delta.sqrMagnitude<.0001f);
        }

        private void Awake()
        {
            closedPosition = transform.position;
            doorCollider = GetComponent<BoxCollider>();
        }

        public override string Prompt(PlayerInteraction player) => Locked
            ? (player.GetComponent<LaundryDuty>()?.Data.hasOfficerKey==true ? "E - Unlock neighboring cell with Harris's key" : "Locked - requires Harris's key")
            : opening ? "E - Close cell door" : "E - Open cell door";

        public override void Interact(PlayerInteraction player)
        {
            if(player==null)return;
            if(Locked)
            {
                if(player.GetComponent<LaundryDuty>()?.Data.hasOfficerKey!=true)
                {player.ShowMessage("Locked. You need Harris's key.");return;}
                unlocked=true;
                player.ShowMessage("Neighboring cell unlocked");
            }
            if (opening && DoorwayOccupied())
            {
                player.ShowMessage("Doorway blocked. Move yourself or the parcel clear.");
                return;
            }
            opening = !opening;
            SampleSoundEvents.Emit(player, SampleSound.Door, transform.position);
        }

        private bool DoorwayOccupied()
        {
            Vector3 center = closedPosition + doorCollider.center;
            Vector3 half = doorCollider.size * .5f + Vector3.one * .08f;
            // Same-frame controller moves can precede physics query updates. Check the live
            // controller pose too, so a close interaction cannot trap a newly arrived player.
            var doorway = new Bounds(center,half*2);
            foreach(var controller in FindObjectsByType<CharacterController>(FindObjectsSortMode.None))
            {
                if(!controller.enabled)continue;
                var scale=controller.transform.lossyScale;
                float radius=controller.radius*Mathf.Max(Mathf.Abs(scale.x),Mathf.Abs(scale.z));
                var size=new Vector3(radius*2,Mathf.Max(controller.height*Mathf.Abs(scale.y),radius*2),radius*2);
                if(doorway.Intersects(new Bounds(controller.transform.TransformPoint(controller.center),size)))return true;
            }
            foreach (Collider other in Physics.OverlapBox(center, half, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore))
            {
                if (other.transform.IsChildOf(transform)) continue;
                if (other.GetComponentInParent<CharacterController>() != null || other.GetComponentInParent<PrototypePickup>() != null)
                    return true;
            }
            return false;
        }

        private void Update() => Step(Time.deltaTime);

        internal void Step(float seconds)
        {
            if(!float.IsFinite(seconds)||seconds<=0)return;
            // Check throughout closing, so walking into a closing doorway reopens it.
            if (!opening && Vector3.Distance(transform.position, closedPosition) > .01f && DoorwayOccupied()) opening = true;
            transform.position = Vector3.MoveTowards(transform.position,
                closedPosition + (opening ? openOffset : Vector3.zero), speed * seconds);
        }
    }
}
