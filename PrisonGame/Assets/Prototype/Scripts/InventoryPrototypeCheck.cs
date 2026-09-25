#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PrisonGame.Prototype
{
    // Shared by the Editor and opt-in executable integration fixture.
    public static class InventoryPrototypeCheck
    {
        public static void Run(PlayerInteraction actor, Action<bool,string> check)
        {
            var inventory=actor.GetComponent<PlayerInventory>();var stock=actor.GetComponent<SnackSupplies>();
            var duty=actor.GetComponent<LaundryDuty>();var save=actor.GetComponent<SampleSaveGame>();
            var movement=actor.GetComponent<FirstPersonController>();var clock=actor.GetComponent<SnackRequest>().Clock;
            var tray=UnityEngine.Object.FindFirstObjectByType<SnackAssembly>();
            var cursor=typeof(FirstPersonController).GetMethod("SetCursorCaptured",BindingFlags.Instance|BindingFlags.NonPublic);
            bool wasWalking=movement.ControlsActive;bool wasPaused=clock.Paused;
            void Pause(){cursor.Invoke(movement,new object[]{false});clock.SetPaused(true);Time.timeScale=0;}
            Pause();var original=save.Capture(actor);
            var clean=JsonUtility.FromJson<SampleSaveData>(JsonUtility.ToJson(original));
            clean.player.laundry=new LaundryDuty.State();clean.player.crackers=clean.player.fruit=clean.player.wrappers=0;
            clean.player.money=5;
            clean.player.starter=false;clean.world.trayStage=0;clean.world.packs=Array.Empty<SampleSaveData.Item>();
            foreach(var item in clean.world.parcels){item.held=false;item.pocketed=false;}
            void Reset(){Pause();save.Restore(actor,clean);clock.SetPaused(false);}
            PrototypePickup Pack()
            {
                var obj=UnityEngine.Object.Instantiate(tray.PackPrefab,actor.transform.position+Vector3.up,Quaternion.identity);
                obj.GetComponent<SnackPackItem>().Initialize(stock);return obj.GetComponent<PrototypePickup>();
            }
            try
            {
                Reset();check(inventory.Used==0,"Inventory starts empty");
                check(!duty.BlackmailOfficer(actor),"Officer key locked before learning secret");
                duty.TalkToDex(actor);check(!duty.Data.pokerSecret,"Secret requires Rue introduction");
                duty.Data.favorAsked=true;duty.Data.favorDone=true;duty.TalkToDex(actor);
                check(duty.Data.pokerSecret&&duty.Data.contactMet,"Dex reveals poker secret after favor");
                check(!duty.BlackmailOfficer(null),"Different actor cannot claim officer key");
                clock.SetPaused(true);check(!duty.BlackmailOfficer(actor),"Paused blackmail cannot grant key");clock.SetPaused(false);
                for(int i=0;i<6;i++){actor.PickUp(Pack());check(inventory.PocketHeld(actor),"Pocket pack "+(i+1));}
                check(inventory.Used==6&&actor.HeldItem==null,"Six pocketed packs fill all slots");
                var extra=Pack();actor.PickUp(extra);check(actor.HeldItem==null&&extra.PocketOwner==null,"Seventh object stays in world");
                check(!stock.CollectStarter()&&!stock.StarterCollected&&stock.Crackers==0,"Full inventory preserves starter allocation");
                check(!duty.BlackmailOfficer(actor)&&!duty.Data.officerKeyGiven&&!duty.Data.hasOfficerKey,"Full inventory keeps key reward available");
                var selected=inventory.Entries[0].item;
                check(!inventory.Hold(null,selected)&&!inventory.PocketHeld(null),"Inventory rejects different acting player");
                check(inventory.Hold(actor,selected)&&inventory.Used==6,"Holding a pocketed pack uses the same slot");
                int money=stock.Money;check(stock.SellHeldPack(actor)&&stock.Money==money+3&&inventory.Used==5,"Pocketed pack can be held and sold once");
                check(!stock.SellHeldPack(actor),"Repeated sale cannot duplicate payment");
                check(duty.BlackmailOfficer(actor)&&inventory.Used==6,"Freed slot permits officer key reward");
                check(!duty.BlackmailOfficer(actor)&&inventory.Used==6,"Officer key cannot be claimed twice");
                Pause();string path=Path.Combine("Temp","InventoryChecks","roundtrip.json");
                check(save.SaveTo(actor,path),"Pocketed packs and officer key save to isolated fixture");
                check(save.LoadFrom(actor,path)&&save.LoadFrom(actor,path),"Repeated inventory load succeeds");
                check(inventory.Used==6&&duty.Data.pokerSecret&&duty.Data.hasOfficerKey&&actor.HeldItem==null,"Load restores exact pockets, secret and key");
                check(actor.GetComponent<SnackRequest>().ReadyPacks==6,"Pocketed and loose packs remain available stock after load");
                check(inventory.Entries.Where(e=>e.item!=null).All(e=>!e.item.GetComponent<Collider>().enabled&&e.item.GetComponentsInChildren<Renderer>().All(r=>!r.enabled)),"Loaded pocketed objects have no visible mesh or collision");
                var corrupt=save.Capture(actor);corrupt.world.packs.First(p=>p.pocketed).held=true;
                bool rejected=false;try{save.Restore(actor,corrupt);}catch(InvalidDataException){rejected=true;}
                check(rejected&&inventory.Used==6,"Invalid dual held-pocketed ownership rejected atomically");
                corrupt=save.Capture(actor);corrupt.player.crackers=1;rejected=false;
                try{save.Restore(actor,corrupt);}catch(InvalidDataException){rejected=true;}
                check(rejected&&stock.Crackers==0,"Over-capacity save rejected without replacing progress");
                clock.SetPaused(false);duty.Data.phase=LaundryDuty.Phase.Active;duty.Data.warned=true;duty.Data.grace=.1f;
                var cc=actor.GetComponent<CharacterController>();cc.enabled=false;actor.transform.position=duty.room.transform.TransformPoint(new Vector3(11,.05f,4));cc.enabled=true;
                duty.Step(actor,1);check(duty.Data.phase==LaundryDuty.Phase.Failed&&duty.Data.hasOfficerKey&&duty.Data.pokerSecret,"Shift failure retains secret and key");
                check(duty.Report(actor)&&duty.Data.hasOfficerKey,"Shift retry retains key");

                Reset();check(stock.CollectStarter()&&inventory.Used==3&&stock.Crackers==2,"Starter ingredient quantities occupy three slots");
                actor.PickUp(Pack());inventory.PocketHeld(actor);
                actor.PickUp(Pack());inventory.PocketHeld(actor);
                actor.PickUp(Pack());inventory.PocketHeld(actor);
                check(inventory.Used==6,"Ingredient stacks and physical items share six-slot budget");
                int before=stock.Money;check(stock.Restock(out _)&&inventory.Used==6&&stock.Crackers==3&&stock.Money==before-1,"Restocking existing stacks at capacity charges exactly once");
                // Full stock with several wrappers: finishing must wait for a slot.
                tray.Restore(2,actor);int wrappers=stock.Wrappers;tray.Interact(actor);
                check(tray.Stage==2&&stock.Wrappers==wrappers&&actor.HeldItem==null,"Full inventory does not consume wrapper or spawn an uncarried pack");
                stock.TakeIngredient(2);stock.TakeIngredient(2);tray.Interact(actor);
                check(tray.Stage==0&&actor.HeldItem!=null&&inventory.Used==6,"Last wrapper frees a slot for the finished pack");

                Reset();var parcel=UnityEngine.Object.FindObjectsByType<PrototypePickup>(FindObjectsSortMode.None).First(p=>p.GetComponent<SnackPackItem>()==null);
                actor.PickUp(parcel);check(inventory.PocketHeld(actor)&&parcel.PocketOwner==actor,"Existing parcel can be pocketed");
                cc=actor.GetComponent<CharacterController>();cc.enabled=false;actor.transform.position=new Vector3(100,50,100);cc.enabled=true;
                check(!inventory.Drop(actor,parcel)&&parcel.PocketOwner==actor&&actor.HeldItem==null,"Unsupported drop preserves pocket ownership");
                Pause();save.Restore(actor,clean);clock.SetPaused(false);
                actor.PickUp(parcel);check(parcel.PocketOwner==null&&parcel.GetComponentsInChildren<Renderer>().All(r=>r.enabled),"Holding restores object visibility");
                check(actor.TryPlaceHeldAt(actor.transform.position+actor.transform.forward*.85f+Vector3.up*.3f)&&actor.HeldItem==null&&inventory.Used==0,"Clear physical placement frees its slot");
                Reset();check(inventory.Used==0&&!duty.Data.pokerSecret,"Older save without new fields still loads");
            }
            finally {Pause();save.Restore(actor,original);cursor.Invoke(movement,new object[]{wasWalking});clock.SetPaused(wasPaused);}
        }
    }
}
#endif
