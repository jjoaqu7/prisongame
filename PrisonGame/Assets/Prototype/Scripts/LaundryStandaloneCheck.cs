#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
namespace PrisonGame.Prototype
{
    // Opt-in integration checks; fixtures stay beside the build, never in the user's save slot.
    public sealed class LaundryStandaloneCheck : MonoBehaviour
    {
        const BindingFlags Private=BindingFlags.NonPublic|BindingFlags.Instance;
        readonly List<string> results=new List<string>(); readonly List<string> errors=new List<string>();
        PlayerInteraction actor; FirstPersonController movement; LaundryDuty duty; PrisonClock clock; Keyboard keyboard;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Begin(){if(!Application.isEditor && Array.IndexOf(Environment.GetCommandLineArgs(),"-laundry-check")>=0)new GameObject("Laundry checks").AddComponent<LaundryStandaloneCheck>();}
        void Awake(){Application.runInBackground=true;Application.logMessageReceived+=Log;}
        void OnDestroy(){Application.logMessageReceived-=Log;}
        void Log(string t,string s,LogType k){if(k==LogType.Error||k==LogType.Exception||k==LogType.Assert)errors.Add(t+"\n"+s);}
        void Check(bool ok,string label){if(!ok)throw new Exception(label);results.Add("PASS: "+label);}
        void Invoke(object obj,string method,params object[] args)=>obj.GetType().GetMethod(method,Private).Invoke(obj,args);
        Vector3 World(Vector3 p)=>duty.room.transform.TransformPoint(p);
        void Place(Vector3 p){var cc=actor.GetComponent<CharacterController>();cc.enabled=false;actor.transform.position=World(p);cc.enabled=true;Physics.SyncTransforms();}
        void Use(LaundryInteraction.Kind kind,Vector3 p)
        {
            Place(p);var target=Array.Find(FindObjectsByType<LaundryInteraction>(FindObjectsSortMode.None),x=>x.kind==kind);
            var camera=actor.GetComponentInChildren<Camera>();var aim=target.transform.position;
            if(kind==LaundryInteraction.Kind.Rue||kind==LaundryInteraction.Kind.Dex||kind==LaundryInteraction.Kind.Guard)aim+=Vector3.up*1.2f;
            camera.transform.rotation=Quaternion.LookRotation(aim-camera.transform.position);Physics.SyncTransforms();
            Invoke(movement,"SetCursorCaptured",true);clock.SetPaused(false);Invoke(actor,"FindTarget");Check(actor.Target==target,"Ray targets "+kind);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.E));InputSystem.Update();Invoke(actor,"Update");
        }
        void Walk(Vector3 target)
        {
            var cc=actor.GetComponent<CharacterController>();var end=World(target);
            for(int i=0;i<300;i++){var delta=end-actor.transform.position;delta.y=0;if(delta.magnitude<.08f)break;cc.Move(delta.normalized*.05f+Vector3.down*.01f);}
            var difference=actor.transform.position-end;difference.y=0;Check(difference.magnitude<.12f,"CharacterController route reaches "+target);
        }
        void UseHarris()
        {
            var target=actor.GetComponent<GuardSuspicion>().Observer;
            Place(duty.room.transform.InverseTransformPoint(new Vector3(6.8f,.05f,5.2f)));
            var camera=actor.GetComponentInChildren<Camera>();
            camera.transform.rotation=Quaternion.LookRotation(target.transform.position+Vector3.up*1.2f-camera.transform.position);
            Physics.SyncTransforms();Invoke(movement,"SetCursorCaptured",true);clock.SetPaused(false);Invoke(actor,"FindTarget");
            Check(actor.Target==target&&target.Prompt(actor).Contains("Blackmail Harris"),"Ray targets Harris with explicit blackmail prompt");
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.E));InputSystem.Update();Invoke(actor,"Update");
        }
        void UseNeighborDoor()
        {
            var door=Array.Find(FindObjectsByType<PrototypeDoor>(FindObjectsSortMode.None),d=>d.RequiresOfficerKey);
            Place(duty.room.transform.InverseTransformPoint(new Vector3(-.75f,.05f,4.5f)));
            var camera=actor.GetComponentInChildren<Camera>();camera.transform.rotation=Quaternion.LookRotation(door.transform.position+Vector3.up*.2f-camera.transform.position);
            Physics.SyncTransforms();Invoke(actor,"FindTarget");
            Check(actor.Target==door&&door.Prompt(actor).Contains("Harris's key"),"Ray targets neighboring cell with key prompt");
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.E));InputSystem.Update();Invoke(actor,"Update");door.Step(3);
            Check(door.IsOpen&&!door.Locked,"E input unlocks and opens neighboring cell");
        }

        bool staging,showInventory;
        void LateUpdate(){if(!staging)return;Invoke(movement,"SetCursorCaptured",true);Time.timeScale=0;clock.SetPaused(false);InputSystem.QueueStateEvent(keyboard,showInventory?new KeyboardState(Key.Tab):new KeyboardState());InputSystem.Update();}
        IEnumerator CaptureUi(string dir)
        {
            staging=true;var hud=actor.GetComponent<LaundryHud>();
            duty.Restore(new LaundryDuty.State{phase=LaundryDuty.Phase.Active,remaining=84,worked=32,favorAsked=true,hasTool=true});
            actor.GetComponent<SnackSupplies>().CollectStarter();
            var tray=FindFirstObjectByType<SnackAssembly>();var pack=Instantiate(tray.PackPrefab);
            pack.GetComponent<SnackPackItem>().Initialize(actor.GetComponent<SnackSupplies>());
            actor.PickUp(pack.GetComponent<PrototypePickup>());actor.GetComponent<PlayerInventory>().PocketHeld(actor);
            var parcel=Array.Find(FindObjectsByType<PrototypePickup>(FindObjectsSortMode.None),p=>p.GetComponent<SnackPackItem>()==null);
            actor.PickUp(parcel);
            Place(new Vector3(3.7f,.05f,5.4f));movement.enabled=false;
            actor.GetComponentInChildren<Camera>().transform.rotation=Quaternion.LookRotation(World(new Vector3(6,1.5f,7))-actor.GetComponentInChildren<Camera>().transform.position);
            hud.ClearNotices();hud.Notify("+1 Valve key - Added to pockets");
            for(int i=0;i<3;i++)
            {
                showInventory=i==1;if(i==2){duty.Data.warned=true;duty.Data.grace=8;}
                yield return new WaitForSecondsRealtime(.2f);yield return new WaitForEndOfFrame();
                var capture=ScreenCapture.CaptureScreenshotAsTexture();if(capture!=null){File.WriteAllBytes(Path.Combine(dir,"hud-"+i+".png"),capture.EncodeToPNG());Destroy(capture);}
            }
            showInventory=false;actor.GetComponent<PlayerInventory>().PocketHeld(actor);
            duty.Restore(new LaundryDuty.State{favorAsked=true,favorDone=true,contactMet=true,pokerSecret=true,officerKeyGiven=true,hasOfficerKey=true});
            var camera=actor.GetComponentInChildren<Camera>();
            Place(duty.room.transform.InverseTransformPoint(new Vector3(6.8f,.05f,5.2f)));
            camera.transform.rotation=Quaternion.LookRotation(actor.GetComponent<GuardSuspicion>().Observer.transform.position+Vector3.up*1.4f-camera.transform.position);
            hud.ClearNotices();actor.ShowDialogue("Harris: Keep your voice down. Take this. It opens the cell next to yours. The poker money stays between us.",12);
            yield return new WaitForSecondsRealtime(.2f);yield return new WaitForEndOfFrame();
            var dialogueCapture=ScreenCapture.CaptureScreenshotAsTexture();if(dialogueCapture!=null){File.WriteAllBytes(Path.Combine(dir,"dialogue.png"),dialogueCapture.EncodeToPNG());Destroy(dialogueCapture);}
            var door=Array.Find(FindObjectsByType<PrototypeDoor>(FindObjectsSortMode.None),d=>d.RequiresOfficerKey);
            Place(duty.room.transform.InverseTransformPoint(new Vector3(-.7f,.05f,4.5f)));
            if(!door.IsOpen){door.Interact(actor);door.Step(3);}Physics.SyncTransforms();
            camera.transform.rotation=Quaternion.LookRotation(new Vector3(-4.5f,1.25f,4.5f)-camera.transform.position);hud.ClearNotices();
            yield return new WaitForSecondsRealtime(.2f);yield return new WaitForEndOfFrame();
            var cellCapture=ScreenCapture.CaptureScreenshotAsTexture();if(cellCapture!=null){File.WriteAllBytes(Path.Combine(dir,"neighbor-cell.png"),cellCapture.EncodeToPNG());Destroy(cellCapture);}
            staging=false;
        }
        IEnumerator Start()
        {
            string dir=Path.Combine(Path.GetDirectoryName(Application.dataPath),"LaundryCheck");Directory.CreateDirectory(dir);
            yield return new WaitForSecondsRealtime(1);
            var background=InputSystem.settings.backgroundBehavior;
            try
            {
                Check(SceneManager.GetActiveScene().name=="Duty01_Laundry","Laundry startup scene");
                actor=FindFirstObjectByType<PlayerInteraction>();movement=actor.GetComponent<FirstPersonController>();duty=actor.GetComponent<LaundryDuty>();clock=actor.GetComponent<SnackRequest>().Clock;
                Check(!movement.ControlsActive&&clock.Paused,"Startup is paused");
                foreach(var r in FindObjectsByType<Renderer>(FindObjectsSortMode.None))foreach(var m in r.sharedMaterials)if(m==null||m.shader==null||!m.shader.isSupported)throw new Exception("Unsupported material: "+r.name);
                Check(true,"All scene materials supported");
                actor.GetComponent<SoloClockDriver>().enabled=false;duty.enabled=false;duty.room.enabled=false;
                InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;keyboard=InputSystem.AddDevice<Keyboard>();
                Use(LaundryInteraction.Kind.Board,new Vector3(1.5f,.05f,-.8f));Check(duty.Data.phase==LaundryDuty.Phase.Active,"E starts shift");
                // Walk around the board, through the entrance and into the working strip.
                Walk(new Vector3(.5f,.05f,-.8f));Walk(new Vector3(.5f,.05f,3.8f));Walk(new Vector3(4.5f,.05f,3.8f));Walk(new Vector3(4.5f,.05f,5.2f));
                Use(LaundryInteraction.Kind.Rue,new Vector3(4.5f,.05f,5.2f));duty.Step(actor,20);Check(duty.Data.favorAsked&&duty.Data.worked==20,"E conversation and nearby work coexist");
                Walk(new Vector3(5.4f,.05f,4.5f));Walk(new Vector3(8,.05f,4.5f));Walk(new Vector3(8,.05f,3.5f));Walk(new Vector3(10.8f,.05f,3.5f));Walk(new Vector3(11.4f,.05f,4.3f));
                Use(LaundryInteraction.Kind.Tool,new Vector3(11.4f,.05f,4.3f));Check(duty.Data.hasTool,"E pockets supply-room tool");
                var save=actor.GetComponent<SampleSaveGame>();Invoke(movement,"SetCursorCaptured",false);clock.SetPaused(true);Time.timeScale=0;Check(save.SaveTo(actor,Path.Combine(dir,"fixture.json")),"Isolated active duty saves");
                duty.Restore(new LaundryDuty.State());Check(save.LoadFrom(actor,Path.Combine(dir,"fixture.json"))&&duty.Data.hasTool&&duty.Data.worked==20,"Disk load restores tool and work");
                Use(LaundryInteraction.Kind.Rue,new Vector3(4.5f,.05f,5.2f));Check(duty.Data.favorDone&&!duty.Data.hasTool,"E delivers favor");
                Use(LaundryInteraction.Kind.Dex,new Vector3(2.6f,.05f,.9f));Check(duty.Data.contactMet,"E completes introduction");
                Check(duty.Data.pokerSecret,"Dex conversation reveals the poker secret");
                Use(LaundryInteraction.Kind.Guard,new Vector3(7.5f,.05f,5.4f));Check(!duty.Data.hasOfficerKey,"Vale cannot grant Harris's key");
                UseHarris();Check(duty.Data.hasOfficerKey,"E blackmail obtains Harris's key");
                Check(actor.GetComponent<LaundryHud>().DialogueText.StartsWith("Harris:"),"E conversation renders Harris immediately");
                UseNeighborDoor();
                var hud=actor.GetComponent<LaundryHud>();Check(hud!=null,"Modern HUD exists");
                Check(Array.Find(hud.PocketRows,x=>x.StartsWith("Valve key"))==null,"Delivered key disappears from inventory view");
                hud.ClearNotices();hud.Notify("Pickup check");Check(hud.NoticeText=="Pickup check","Warm notification receives messages");
                hud.ClearNotices();Check(hud.NoticeText==null,"Load cleanup clears transient notices");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Tab));InputSystem.Update();Check(hud.InventoryVisible,"Tab reveals inventory");Invoke(actor,"Update");Check(actor.Target==null,"Inventory view blocks world interaction targeting");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();Check(!hud.InventoryVisible,"Releasing Tab hides inventory");
                Place(new Vector3(3.7f,.05f,5.4f));duty.Step(actor,220);Check(duty.Data.phase==LaundryDuty.Phase.Completed,"Full shift completes in executable");
                InventoryPrototypeCheck.Run(actor,Check);
                DialogueCellCheck.Run(actor,Check);
                var inventory=actor.GetComponent<PlayerInventory>();
                var parcel=Array.Find(FindObjectsByType<PrototypePickup>(FindObjectsSortMode.None),p=>p.GetComponent<SnackPackItem>()==null);
                actor.PickUp(parcel);
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.R));InputSystem.Update();Invoke(actor,"Update");
                Check(actor.HeldItem==null&&parcel.PocketOwner==actor,"R input pockets held parcel");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Tab,Key.Digit2,Key.F));InputSystem.Update();Invoke(hud,"Update");
                Check(actor.HeldItem==parcel&&parcel.PocketOwner==null,"Tab plus selection and F input holds selected item");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Q));InputSystem.Update();Invoke(actor,"Update");
                Check(actor.HeldItem==null&&parcel.PocketOwner==null&&inventory.Used==1,"Q input places object and frees slot");
                actor.GetComponentInChildren<Camera>().transform.rotation=Quaternion.LookRotation(World(new Vector3(6,1.5f,7))-actor.GetComponentInChildren<Camera>().transform.position);
                var camera=actor.GetComponentInChildren<Camera>();var rt=new RenderTexture(1280,720,24);var old=camera.targetTexture;var active=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(dir,"room.png"),tex.EncodeToPNG());camera.targetTexture=old;RenderTexture.active=active;Destroy(tex);Destroy(rt);
            }
            catch(Exception e){errors.Add(e.ToString());}
            finally{InputSystem.settings.backgroundBehavior=background;}
            if(errors.Count==0)yield return CaptureUi(dir);
            if(keyboard!=null)InputSystem.RemoveDevice(keyboard);
            yield return null;
            File.WriteAllText(Path.Combine(dir,"results.txt"),(errors.Count==0?"PASS":"FAIL")+"\n"+string.Join("\n",results)+"\n"+string.Join("\n",errors));Application.Quit(errors.Count==0?0:1);
        }
    }
}
#endif
