#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PrisonGame.Prototype
{
    public static class DialogueCellCheck
    {
        public static void Run(PlayerInteraction actor, Action<bool,string> check)
        {
            var hud=actor.GetComponent<LaundryHud>();var duty=actor.GetComponent<LaundryDuty>();
            var save=actor.GetComponent<SampleSaveGame>();var clock=actor.GetComponent<SnackRequest>().Clock;
            var movement=actor.GetComponent<FirstPersonController>();var driver=actor.GetComponent<SoloClockDriver>();
            var cursor=typeof(FirstPersonController).GetMethod("SetCursorCaptured",BindingFlags.Instance|BindingFlags.NonPublic);
            var door=UnityEngine.Object.FindObjectsByType<PrototypeDoor>(FindObjectsSortMode.None).Single(d=>d.RequiresOfficerKey);
            bool walking=movement.ControlsActive,paused=clock.Paused,driving=driver.enabled;driver.enabled=false;
            void Pause(){cursor.Invoke(movement,new object[]{false});clock.SetPaused(true);Time.timeScale=0;}
            void Place(Vector3 position){var cc=actor.GetComponent<CharacterController>();cc.enabled=false;actor.transform.position=position;cc.enabled=true;Physics.SyncTransforms();}
            void Walk(Vector3 end)
            {
                var cc=actor.GetComponent<CharacterController>();
                for(int i=0;i<300;i++){var delta=end-actor.transform.position;delta.y=0;if(delta.magnitude<.08f)break;cc.Move(delta.normalized*.04f+Vector3.down*.01f);}
                Physics.SyncTransforms();
            }
            Pause();var original=save.Capture(actor);
            try
            {
                hud.ClearNotices();hud.Notify("Old pickup notice",12);hud.Notify("Another queued notice",12);
                actor.ShowDialogue("Rue: First response",9);
                check(hud.DialogueText=="Rue: First response","Dialogue appears immediately despite notification backlog");
                actor.ShowDialogue("Dex: New response",12);
                check(hud.DialogueText=="Dex: New response"&&hud.NoticeText=="Old pickup notice","New speaker immediately replaces dialogue without erasing pickup notices");
                duty.Data.phase=LaundryDuty.Phase.Active;duty.Data.warned=true;duty.Data.grace=8;
                actor.ShowDialogue("Harris: Visible response",8);
                check(hud.WarningVisible&&hud.DialogueText=="Harris: Visible response","Conversation and active return warning coexist");
                hud.ClearNotices();check(hud.DialogueText==null&&hud.NoticeText==null,"Load cleanup removes dialogue and notices");

                var clean=JsonUtility.FromJson<SampleSaveData>(JsonUtility.ToJson(original));clean.player.laundry=new LaundryDuty.State();clean.world.neighboringCell=null;
                save.Restore(actor,clean);clock.SetPaused(false);
                Place(new Vector3(-.9f,.05f,4.5f));door.Interact(actor);door.Step(1);
                check(door.Locked&&!door.IsOpen,"Neighbor cell refuses entry without key");
                Walk(new Vector3(-3.4f,.05f,4.5f));check(actor.transform.position.x> -2,"Locked door physically blocks the character");
                duty.Data.favorAsked=true;duty.Data.favorDone=true;duty.TalkToDex(actor);
                check(hud.DialogueText.Contains("Harris")&&!hud.DialogueText.Contains("Vale"),"Dex identifies Harris and the neighboring cell");
                var vale=UnityEngine.Object.FindObjectsByType<LaundryInteraction>(FindObjectsSortMode.None).First(i=>i.kind==LaundryInteraction.Kind.Guard);
                vale.Interact(actor);check(!duty.Data.hasOfficerKey&&!vale.Prompt(actor).Contains("Blackmail"),"Laundry officer retains only his own dialogue");
                actor.GetComponent<GuardSuspicion>().Observer.Interact(actor);
                check(duty.Data.hasOfficerKey&&hud.DialogueText.StartsWith("Harris:"),"Harris gives the key with immediate dialogue");
                check(actor.GetComponent<PlayerInventory>().Entries.Any(e=>e.name=="Harris's key"&&e.detail.Contains("cell")),"Inventory names Harris's key and its destination");
                Place(new Vector3(-.9f,.05f,4.5f));door.Interact(actor);door.Step(.2f);
                check(!door.Locked&&duty.Data.hasOfficerKey&&!door.IsOpen,"Key unlocks door permanently without being consumed");
                Pause();string path=Path.Combine("Temp","DialogueCellChecks","roundtrip.json");
                check(save.SaveTo(actor,path),"Partially open neighboring cell saves to isolated fixture");
                var midway=door.transform.position;door.Step(3);
                check(save.LoadFrom(actor,path)&&Vector3.Distance(door.transform.position,midway)<.001f&&!door.Locked,"Load restores unlock and partial slide position");
                door.Step(3);Physics.SyncTransforms();check(door.IsOpen,"Loaded door finishes opening");
                Place(new Vector3(-.8f,.05f,4.5f));Walk(new Vector3(-3.35f,.05f,4.5f));
                check(actor.transform.position.x< -3.2f,"Character can walk from corridor into unlocked cell");
                Walk(new Vector3(-3.35f,.05f,6));check(actor.transform.position.z>5.85f,"Interior furniture leaves a usable walking route");
                Walk(new Vector3(-3.35f,.05f,4.5f));Walk(new Vector3(-2,.05f,4.5f));
                check(Vector3.Distance(actor.transform.position,new Vector3(-2,.05f,4.5f))<.15f,"Character reaches center of doorway for obstruction check");
                var prePosition=door.transform.position;
                bool occupied=(bool)typeof(PrototypeDoor).GetMethod("DoorwayOccupied",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(door,null);
                door.Interact(actor);door.Step(2);check(door.IsOpen,"Door refuses to close on player in doorway"+(door.IsOpen?"":" player="+actor.transform.position+" doorBefore="+prePosition+" after="+door.transform.position+" occupied="+occupied));
                Walk(new Vector3(-.8f,.05f,4.5f));door.Interact(actor);door.Step(3);
                check(!door.IsOpen&&!door.Locked,"Player can exit and close the unlocked door");
                var bad=save.Capture(actor);bad.world.neighboringCell.position+=Vector3.up;
                bool rejected=false;try{save.Restore(actor,bad);}catch(InvalidDataException){rejected=true;}
                check(rejected&&!door.Locked,"Invalid neighbor door position rejected without losing unlock");
                var old=save.Capture(actor);old.world.neighboringCell=null;
                save.Restore(actor,old);check(door.Locked&&duty.Data.hasOfficerKey,"Old save without room state loads closed while retaining earned key");
                clock.SetPaused(false);door.Interact(actor);door.Step(3);
                check(door.IsOpen&&!door.Locked,"Previously earned officer key opens the new room");
            }
            finally{Pause();save.Restore(actor,original);cursor.Invoke(movement,new object[]{walking});clock.SetPaused(paused);driver.enabled=driving;}
        }
    }
}
#endif
