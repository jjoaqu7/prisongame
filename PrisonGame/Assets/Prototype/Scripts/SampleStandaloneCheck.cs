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
    // Opt-in, local Development Player checks. No runtime service or user-slot writes.
    public sealed class SampleStandaloneCheck : MonoBehaviour
    {
        const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        readonly List<string> results = new List<string>();
        readonly List<string> errors = new List<string>();
        PlayerInteraction actor;
        FirstPersonController movement;
        SnackRequest request;
        SnackSupplies stock;
        SampleSaveGame save;
        PrisonClock clock;
        SnackAssembly tray;
        SnackSupplyBox box;
        PrototypeInmate inmate;
        Keyboard keyboard;
        string directory, phase;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Begin()
        {
            var args = Environment.GetCommandLineArgs();
            if (Application.isEditor || (Array.IndexOf(args, "-sample-check-write") < 0 && Array.IndexOf(args, "-sample-check-read") < 0)) return;
            new GameObject("Sample standalone check").AddComponent<SampleStandaloneCheck>();
        }
        void Awake() { Application.runInBackground = true; Application.logMessageReceived += Log; }
        void OnDestroy() { Application.logMessageReceived -= Log; }
        void Log(string text, string stack, LogType kind)
        { if (kind == LogType.Error || kind == LogType.Exception || kind == LogType.Assert) errors.Add(text + "\n" + stack); }
        void Check(bool ok, string label)
        { if (!ok) throw new InvalidOperationException("FAIL: " + label); results.Add("PASS: " + label); }
        static void Invoke(object obj, string method, params object[] args) => obj.GetType().GetMethod(method, Private).Invoke(obj, args);
        void Place(Vector3 position)
        {
            var cc = actor.GetComponent<CharacterController>(); cc.enabled = false;
            actor.transform.position = position; cc.enabled = true; Physics.SyncTransforms();
        }
        void Use(PrototypeInteractable target, Vector3 position, Vector3 aim)
        {
            Place(position);
            var camera = actor.GetComponentInChildren<Camera>();
            camera.transform.rotation = Quaternion.LookRotation(aim - camera.transform.position);
            Physics.SyncTransforms(); Invoke(movement, "SetCursorCaptured", true);
            Invoke(actor, "FindTarget");
            if (actor.Target != target) throw new Exception("Cannot target " + target.name);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E)); InputSystem.Update();
            Invoke(actor, "Update");
        }
        void Supply() => Use(box, new Vector3(2.7f,.05f,3.8f), box.transform.position);
        void Assemble(int count = 3)
        { for(int i=0;i<count;i++) Use(tray,new Vector3(2.7f,.05f,3.8f),tray.transform.position); }
        void Sell() => Use(inmate,new Vector3(2.4f,.05f,3.5f),inmate.transform.position+Vector3.up*1.2f);
        void Pause() { Invoke(movement,"SetCursorCaptured",false); clock.SetPaused(true); Time.timeScale=0; }

        IEnumerator Start()
        {
            phase = Array.IndexOf(Environment.GetCommandLineArgs(), "-sample-check-read") >= 0 ? "read" : "write";
            directory = Path.Combine(Path.GetDirectoryName(Application.dataPath), "SampleCheck");
            Directory.CreateDirectory(directory);
            yield return new WaitForSecondsRealtime(1);
            yield return new WaitForEndOfFrame();
            var background = InputSystem.settings.backgroundBehavior;
            try
            {
                Check(SceneManager.GetActiveScene().name=="Save01_Progress", "Combined sample is the startup scene");
                actor=FindFirstObjectByType<PlayerInteraction>(); movement=actor.GetComponent<FirstPersonController>();
                request=actor.GetComponent<SnackRequest>();stock=actor.GetComponent<SnackSupplies>();save=actor.GetComponent<SampleSaveGame>();clock=request.Clock;
                tray=FindFirstObjectByType<SnackAssembly>();box=FindFirstObjectByType<SnackSupplyBox>();inmate=FindFirstObjectByType<PrototypeInmate>();
                Check(save!=null && tray!=null && box!=null && inmate!=null, "Combined gameplay and save components included");
                Check(!movement.ControlsActive && clock.Paused && Time.timeScale==0, "Startup settings pause controls and simulation");
                Check(!request.Accepted && stock.Money==0, "Ordinary startup does not automatically load progress");
                int materials=0;
                foreach(var renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None)) foreach(var material in renderer.sharedMaterials)
                { CheckMaterial(material,renderer.name); materials++; }
                Check(materials>0,"All "+materials+" rendered materials supported");
                Capture(Path.Combine(directory,phase+"-startup.png"));
                actor.GetComponent<SoloClockDriver>().enabled=false; Pause();
                InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
                keyboard=InputSystem.AddDevice<Keyboard>();
                if(phase=="write") WriteChecks(); else ReadChecks();
            }
            catch(Exception e) { errors.Add(e.ToString()); }
            finally
            {
                if(keyboard!=null)InputSystem.RemoveDevice(keyboard);
                InputSystem.settings.backgroundBehavior=background;
                if(movement!=null)Invoke(movement,"SetCursorCaptured",false);
            }
            if(errors.Count==0) yield return ProbeAudio();
            yield return null;
            string report=(errors.Count==0?"PASS":"FAIL")+" - combined Windows sample "+phase+"\nUnity: "+Application.unityVersion+"\nGraphics: "+SystemInfo.graphicsDeviceType+"\n"+string.Join("\n",results)+"\n"+string.Join("\n",errors);
            File.WriteAllText(Path.Combine(directory,phase+"-results.txt"),report); Debug.Log(report);
            Application.Quit(errors.Count==0?0:1);
        }
        IEnumerator ProbeAudio()
        {
            var audio=actor.GetComponent<SampleAudio>();
            if(audio==null) { errors.Add("Missing sample audio presenter");yield break; }
            audio.SetVolumes(.65f,.35f,false);
            var data=new float[4096];AudioListener.GetOutputData(data,0);
            double end=Time.realtimeSinceStartupAsDouble+.8;float peak=0;bool emitted=false;
            while(Time.realtimeSinceStartupAsDouble<end)
            {
                clock.SetPaused(false);Invoke(movement,"SetCursorCaptured",true);Invoke(audio,"SetPaused",false);
                if(!emitted){SampleSoundEvents.Emit(actor,SampleSound.Wrap,actor.transform.position);emitted=true;}
                AudioListener.GetOutputData(data,0);
                foreach(float value in data)peak=Mathf.Max(peak,Mathf.Abs(value));
                yield return null;
            }
            try { Check(peak>.000001f && peak<.98f,"Audio listener produces non-silent unclipped mixed output; peak="+peak); }
            catch(Exception e){errors.Add(e.ToString());}
            Pause();Invoke(audio,"SetPaused",true);
        }
        void CheckMaterial(Material m,string name)
        { if(m==null || m.shader==null || !m.shader.isSupported) throw new Exception("Unsupported material: "+name); }
        void WriteChecks()
        {
            var inspection=FindFirstObjectByType<SupplyInspection>();var guard=actor.GetComponent<GuardSuspicion>();
            Sell();Check(request.Accepted && request.DueAt==540,"Real E accepts the one-hour request");
            Supply();Check(stock.StarterCollected && stock.Crackers==2,"Real E collects starter stock");
            Assemble();Check(actor.HeldItem!=null,"Real E assembles and carries a pack");
            Sell();Check(stock.Money==3 && request.Delivered==1 && inspection.Closed,"First sale pays and starts supply inspection");
            Supply();Check(stock.Money==3 && stock.Crackers==1,"Closed supply box refuses collection without charging");
            Assemble(1);Check(tray.Stage==1 && stock.Crackers==0,"Second pack's unfinished work consumes its cracker");
            Place(new Vector3(6.8f,.05f,5.7f));clock.SetPaused(false);guard.Step(4);
            Check(Mathf.Abs(guard.Level-50)<.001f && guard.Rising,"Observed trespass raises suspicion in the executable");
            clock.Advance(10);Pause();
            Check(clock.TotalMinutes==482 && request.MinutesLeft==58 && inspection.MinutesLeft==8,"Request and inspection share the advancing prison clock");
            Check(save.SaveTo(actor,Path.Combine(directory,"progress.json")),"Executable writes isolated progress to disk");
            var saved=save.Capture(actor);File.WriteAllText(Path.Combine(directory,"expected.json"),JsonUtility.ToJson(saved));
            Check(saved.player.suspicion==50 && saved.world.trayStage==1,"Saved snapshot contains guard and unfinished work");
        }
        void ReadChecks()
        {
            var inspection=FindFirstObjectByType<SupplyInspection>();var guard=actor.GetComponent<GuardSuspicion>();
            string path=Path.Combine(directory,"progress.json");
            Check(save.LoadFrom(actor,path),"A second executable process loads the first process's save");
            var expected=JsonUtility.FromJson<SampleSaveData>(File.ReadAllText(Path.Combine(directory,"expected.json")));
            Check(stock.Money==3 && stock.Crackers==0 && stock.Fruit==1 && stock.Wrappers==1,"Money and partial ingredients survive process restart");
            Check(request.Delivered==1 && request.Paid==3 && request.DueAt==540,"Request progress and deadline survive process restart");
            Check(tray.Stage==1 && tray.WorkingPlayer==actor && guard.Level==50,"Partial tray ownership and suspicion survive restart");
            Check(Vector3.Distance(actor.transform.position,expected.player.position)<.001f,"Saved player position survives restart");
            Check(clock.TotalMinutes==482 && inspection.MinutesLeft==8 && clock.Paused && !movement.ControlsActive,"Offline time does not advance timers and load remains paused");
            Assemble(2);Check(actor.HeldItem!=null && request.ReadyPacks==1,"Restored partial tray produces a deliverable pack");
            Sell();Check(request.Delivered==2 && stock.Money==6,"Delivery after loading pays exactly once");
            Place(new Vector3(6.8f,.05f,4.8f));clock.SetPaused(false);guard.Step(3);clock.Advance(40);Pause();
            Check(guard.Level==0 && !inspection.Closed,"Leaving clears suspicion and the inspection reopens on time");
            Supply();Check(stock.Money==5 && stock.Crackers==1,"Reopened box restores paid restocking");
            Assemble();Sell();Check(request.Complete && request.Paid==9 && stock.Money==8,"Loaded session completes the request with correct earnings");
            var shelf=FindFirstObjectByType<SnackShelf>();Use(shelf,new Vector3(-3.4f,.05f,-1.3f),new Vector3(-3.08f,1.47f,-2.85f));
            Check(shelf.Installed && stock.Money==0,"Real E purchases the earned cell shelf");
            Pause();Check(save.SaveTo(actor,path) && File.Exists(path+".bak"),"Completed progress replaces the save and retains a backup");
            Check(save.LoadFrom(actor,path) && shelf.Installed && request.Complete,"Completed shelf and request load correctly");
            string before=JsonUtility.ToJson(save.Capture(actor));File.WriteAllText(Path.Combine(directory,"invalid.json"),"{bad");
            Check(!save.LoadFrom(actor,Path.Combine(directory,"invalid.json")) && JsonUtility.ToJson(save.Capture(actor))==before,"Malformed save cannot replace live progress");
            Capture(Path.Combine(directory,"completed.png"));
        }
        static void Capture(string path)
        {
            var camera=Camera.main;var target=RenderTexture.GetTemporary(1280,720,24,RenderTextureFormat.ARGB32);
            var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;var texture=new Texture2D(1280,720,TextureFormat.RGB24,false);
            try { camera.targetTexture=target;camera.Render();RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG()); }
            finally { camera.targetTexture=oldTarget;RenderTexture.active=oldActive;RenderTexture.ReleaseTemporary(target);Destroy(texture); }
        }
    }
}
#endif
