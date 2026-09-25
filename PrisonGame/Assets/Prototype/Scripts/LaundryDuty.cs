using System;
using UnityEngine;

namespace PrisonGame.Prototype
{
    // Personal assignment and favor state; methods explicitly identify the acting player.
    public sealed class LaundryDuty : MonoBehaviour
    {
        public const float ShiftSeconds=120, WorkSeconds=60, DetectionSeconds=1, ReturnSeconds=8;
        public enum Phase { Unreported, Active, Completed, Failed }
        [Serializable] public sealed class State
        {
            public Phase phase; public float remaining=ShiftSeconds, worked, seen, grace;
            public bool warned, favorAsked, hasTool, favorDone, contactMet;
            public bool pokerSecret, officerKeyGiven, hasOfficerKey;
            public string outcome="";
        }
        public LaundryRoom room;
        public State Data { get; private set; } = new State();
        PlayerInteraction owner; PrisonClock clock;
        void Awake() { owner=GetComponent<PlayerInteraction>(); clock=GetComponent<SnackRequest>().Clock; }
        void Update() { Step(owner,Time.deltaTime); room.toolVisual.SetActive(!Data.hasTool && !Data.favorDone); }
        public bool InLaundry => room.transform.InverseTransformPoint(transform.position).z > -2;
        bool Allowed(PlayerInteraction actor) => actor==owner && !clock.Paused;
        public bool Report(PlayerInteraction actor)
        {
            if (!Allowed(actor) || Data.phase==Phase.Active) return false;
            Data.phase=Phase.Active; Data.remaining=ShiftSeconds; Data.worked=Data.seen=Data.grace=0; Data.warned=false;Data.outcome="";
            actor.ShowDialogue("Vale: Two-minute laundry shift. One minute of work inside the blue line. Stay on duty until relieved.",8);
            return true;
        }
        public void Step(PlayerInteraction actor,float seconds)
        {
            if (!Allowed(actor) || Data.phase!=Phase.Active || !float.IsFinite(seconds) || seconds<=0) return;
            float dt=Mathf.Min(seconds,Data.remaining); bool atWork=room.AtWork(actor);
            if(atWork) { Data.worked=Mathf.Min(WorkSeconds,Data.worked+dt);Data.seen=0;Data.grace=0; }
            else if(Data.grace>0)
            {
                Data.grace=Mathf.Max(0,Data.grace-dt);
                if(Data.grace<=0){Fail(actor,"You ignored Vale's return order.");return;}
            }
            else if(room.CanSee(actor))
            {
                Data.seen+=dt;
                if(Data.seen>=DetectionSeconds)
                {
                    Data.seen=0;
                    if(Data.warned){Fail(actor,"Vale caught you away again.");return;}
                    Data.warned=true;Data.grace=ReturnSeconds;
                    actor.ShowMessage("Vale: Back inside the blue work line. Eight seconds. This is your warning!",8);
                }
            }
            else Data.seen=0;
            Data.remaining=Mathf.Max(0,Data.remaining-dt);
            if(Data.remaining<=0)
            {
                if(Data.worked>=WorkSeconds){Data.phase=Phase.Completed;Data.grace=0;Data.outcome="Shift complete. You are relieved.";actor.ShowMessage(Data.outcome,6);}
                else Fail(actor,"Shift ended before you completed one minute of work.");
            }
        }
        void Fail(PlayerInteraction actor,string reason)
        {Data.phase=Phase.Failed;Data.grace=Data.seen=0;Data.outcome=reason+" Shift failed. Other progress kept. Report at the board to retry.";actor.ShowMessage(Data.outcome,8);}
        public void TalkToRue(PlayerInteraction actor)
        {
            if(!Allowed(actor))return;
            if(Data.hasTool){Data.hasTool=false;Data.favorDone=true;room.toolVisual.SetActive(false);actor.ShowDialogue("Rue: That valve key is what I needed. Tell Dex by the corridor that Rue sent you. He knows this place.",9);return;}
            if(Data.favorDone){actor.ShowDialogue("Rue: Dex is by the corridor. Tell him I sent you. Keep an eye on Vale while we talk.",7);return;}
            Data.favorAsked=true;
            actor.ShowDialogue("Rue: There's a valve key on the supply-room shelf. Bring it here and I'll introduce you to Dex. That room is off limits. Watch Vale's turns.",10);
        }
        public bool TakeTool(PlayerInteraction actor)
        {
            if(!Allowed(actor)||!Data.favorAsked||Data.hasTool||Data.favorDone||!room.InSupply(actor))return false;
            var inventory=GetComponent<PlayerInventory>();
            if(inventory!=null && !inventory.CanAdd()){inventory.Full();return false;}
            Data.hasTool=true;room.toolVisual.SetActive(false);SampleSoundEvents.Emit(actor,SampleSound.Pickup,actor.transform.position);
            actor.ShowMessage("+1 Valve key\nAdded to pockets",3);return true;
        }
        public void TalkToDex(PlayerInteraction actor)
        {
            if(!Allowed(actor))return;
            if(!Data.favorDone){actor.ShowDialogue("Dex: I don't know you yet. Rue can vouch for you; ask him at the laundry machines.",7);return;}
            Data.contactMet=true;
            Data.pokerSecret=true;
            actor.ShowDialogue("Dex: Rue sent you? Here's something useful. Harris skims extra from the poker games before splitting the take. He is in the common room. Mention the money and ask for the key to the cell next to yours.",12);
        }
        public bool BlackmailOfficer(PlayerInteraction actor)
        {
            if(!Allowed(actor)||!Data.pokerSecret)return false;
            if(Data.officerKeyGiven){actor.ShowDialogue("Harris: You got the key. The cell next to yours. Keep the poker money to yourself.",7);return false;}
            var inventory=GetComponent<PlayerInventory>();
            if(inventory!=null && !inventory.CanAdd()){actor.ShowDialogue("Harris: Your pockets are full. Make room, then we'll talk.",6);return false;}
            Data.officerKeyGiven=true;Data.hasOfficerKey=true;
            actor.ShowDialogue("Harris: Keep your voice down. Take this. It opens the cell next to yours. The poker money stays between us.",8);
            SampleSoundEvents.Emit(actor,SampleSound.Pickup,actor.transform.position);
            actor.ShowMessage("+1 Harris's key\nAdded to pockets",3);
            return true;
        }
        public State Capture() => JsonUtility.FromJson<State>(JsonUtility.ToJson(Data));
        public static bool Valid(State s)
        {
            return s!=null && Enum.IsDefined(typeof(Phase),s.phase) && float.IsFinite(s.remaining) && s.remaining>=0 && s.remaining<=240 &&
                float.IsFinite(s.worked)&&s.worked>=0&&s.worked<=120 && float.IsFinite(s.seen)&&s.seen>=0&&s.seen<DetectionSeconds &&
                float.IsFinite(s.grace)&&s.grace>=0&&s.grace<=ReturnSeconds && (s.grace==0 || (s.warned && s.phase==Phase.Active)) &&
                (!s.hasTool || (s.favorAsked&&!s.favorDone)) && (!s.favorDone||s.favorAsked) && (!s.contactMet||s.favorDone) &&
                (!s.pokerSecret||s.contactMet) && (!s.officerKeyGiven||s.pokerSecret) && (!s.hasOfficerKey||s.officerKeyGiven) &&
                (s.phase!=Phase.Completed || (s.remaining==0&&s.worked>=WorkSeconds)) && (s.phase!=Phase.Active||s.remaining>0) &&
                s.outcome!=null && s.outcome.Length<=500;
        }
        // Accept up to the original 120-second quota in older saves; retain earned work up to the new cap.
        public void Restore(State s) {Data=JsonUtility.FromJson<State>(JsonUtility.ToJson(s));Data.worked=Mathf.Min(Data.worked,WorkSeconds);Data.remaining=Mathf.Min(Data.remaining,ShiftSeconds);room.toolVisual.SetActive(!Data.hasTool&&!Data.favorDone);}
    }
}
