using UnityEngine;
namespace PrisonGame.Prototype
{
    public sealed class LaundryInteraction : PrototypeInteractable
    {
        public enum Kind { Board, Rue, Dex, Tool, Guard }
        public Kind kind;
        public override string Prompt(PlayerInteraction player)
        {
            var duty=player.GetComponent<LaundryDuty>();if(duty==null)return "";
            switch(kind)
            {
                case Kind.Board:return duty.Data.phase==LaundryDuty.Phase.Active?"E - Read current laundry assignment":"E - Report for assigned laundry duty";
                case Kind.Rue:return duty.Data.hasTool?"E - Give valve key to Rue":"E - Talk to Rue (work continues)";
                case Kind.Dex:return duty.Data.pokerSecret?"E - Talk to Dex":duty.Data.favorDone?"E - Meet Dex - Rue's introduction":"E - Talk to Dex";
                case Kind.Tool:return duty.Data.favorAsked?"E - Pocket valve key (unauthorized)":"E - Examine valve key";
                default:return "E - Talk to Officer Vale";
            }
        }
        public override void Interact(PlayerInteraction player)
        {
            var d=player.GetComponent<LaundryDuty>();if(d==null)return;
            switch(kind)
            {
                case Kind.Board: if(!d.Report(player))player.ShowMessage("Assigned shift: two minutes total, one cumulative minute inside the blue work line. First warning: return within eight seconds; another breach fails the shift.",9);break;
                case Kind.Rue:d.TalkToRue(player);break;
                case Kind.Dex:d.TalkToDex(player);break;
                case Kind.Tool:if(!d.Data.favorAsked)player.ShowMessage("A laundry valve key. Rue may have a use for it. Supply room: authorized staff only.",7);else d.TakeTool(player);break;
                case Kind.Guard:player.ShowDialogue("Vale: Quiet talk is fine. Stay inside the blue work line. The supply room is off limits. Ignore my warning or wander off again and you fail the shift.",9);break;
            }
        }
    }
}
