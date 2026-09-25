using UnityEngine;

namespace PrisonGame.Prototype
{
    public sealed class GuardObserver : PrototypeInteractable
    {
        [SerializeField] private RestrictedArea area;
        [SerializeField] private Transform eyes;
        public string GuardName => "Officer Harris";
        public RestrictedArea Area => area;

        public bool CanSee(PlayerInteraction actor)
        {
            if (!isActiveAndEnabled || actor == null || eyes == null) return false;
            Vector3 delta = actor.transform.position + Vector3.up * 1.1f - eyes.position;
            if (delta.magnitude > 6f) return false;
            Vector3 planar = new Vector3(delta.x, 0, delta.z);
            if (Vector3.Angle(transform.forward, planar) > 60f) return false;
            // Include the player's Ignore Raycast layer; ignore our body, the target and area triggers.
            foreach (var hit in Physics.RaycastAll(eyes.position, delta.normalized, delta.magnitude, ~0, QueryTriggerInteraction.Ignore))
                if (!hit.transform.IsChildOf(transform) && !hit.transform.IsChildOf(actor.transform)) return false;
            return true;
        }

        public override string Prompt(PlayerInteraction actor)
        {
            var duty=actor.GetComponent<LaundryDuty>();
            return duty!=null && duty.Data.pokerSecret && !duty.Data.officerKeyGiven
                ? "E - Blackmail Harris: poker money for a key" : "E - Talk to Officer Harris";
        }
        public override void Interact(PlayerInteraction actor)
        {
            if (actor == null) return;
            var duty=actor.GetComponent<LaundryDuty>();
            if(duty!=null && duty.Data.pokerSecret){duty.BlackmailOfficer(actor);return;}
            var suspicion = actor.GetComponent<GuardSuspicion>();
            actor.ShowDialogue(suspicion != null && suspicion.Observer == this && suspicion.Level >= 40
                ? "Harris: Outside the marked area. Now. Leave and my suspicion will fall."
                : "Harris: Marked corner is staff only. Stay outside the line and we won't have a problem.", 6f);
        }
    }
}
