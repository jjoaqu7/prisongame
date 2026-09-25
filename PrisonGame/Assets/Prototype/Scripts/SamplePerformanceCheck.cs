#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace PrisonGame.Prototype
{
    // Opt-in diagnostic only. Real scene rendering goes to a full-resolution texture so a
    // hidden window cannot skip world rendering. Presentation/display latency is not measured.
    [DefaultExecutionOrder(-200)]
    public sealed class SamplePerformanceCheck : MonoBehaviour
    {
        struct Frame { public int phase, operation; public double ms,cpu,gpu; public long allocation,draws; public double main,render,presentWait; public ulong timestamp; public long loop,scripts,gui,gc,present,renderWait; }
        [Serializable] sealed class MemoryPoint
        { public string phase; public long unityAllocated,unityReserved,monoUsed,processWorkingSet; public int gen0; }
        [Serializable] sealed class PhaseResult
        {
            public string phase; public int frames,over16ms,over33ms,over50ms;
            public double averageMs,p50Ms,p95Ms,p99Ms,maxMs,averageFps,averageAllocationBytes,averageDrawCalls,cpuP95Ms,gpuP95Ms;
            public int timingSamples;
        }
        [Serializable] sealed class Report
        {
            public string status,unity,scene,cpu,gpu,graphics,os,method;
            public int width,height,systemRamMB,vramMB,cpuThreads,vSync,targetFrameRate,sales,footsteps,soundCues,walkCorners;
            public double walkedMetres;
            public int preferredFrameLimit; public bool frameLimitControlsVerified; public bool frameTimingEnabled,gcRecorderValid,drawRecorderValid,visible; public List<string> markerStatus=new List<string>();
            public List<PhaseResult> phases=new List<PhaseResult>();
            public List<MemoryPoint> memory=new List<MemoryPoint>();
            public List<double> saveMilliseconds=new List<double>(),loadMilliseconds=new List<double>();
            public List<string> errors=new List<string>();
        }
        readonly List<Frame> frames=new List<Frame>(100000);
        readonly Report report=new Report();
        readonly FrameTiming[] timing=new FrameTiming[1];
        readonly System.Diagnostics.Stopwatch stopwatch=new System.Diagnostics.Stopwatch();
        readonly Vector3[] route={new Vector3(2f,.05f,.8f),new Vector3(6.5f,.05f,.8f),new Vector3(6.5f,.05f,1.8f),new Vector3(2f,.05f,1.8f)};
        readonly string[] names={"warmup","walking","activity","save_load","post_load_walking"};
        PlayerInteraction actor;FirstPersonController movement;SnackRequest request;SnackSupplies stock;
        SampleSaveGame save;PrisonClock clock;SnackAssembly tray;SnackSupplyBox box;PrototypeInmate inmate;
        SampleAudio audio;Keyboard keyboard;Camera camera;RenderTexture target;
        Action<bool> capture;
        ProfilerRecorder gcRecorder,drawRecorder;
        readonly string[] markerNames={"PlayerLoop","Update.ScriptRunBehaviourUpdate","GUI.Repaint","GC.Collect","Gfx.WaitForPresentOnGfxThread","Gfx.WaitForRenderThread"};
        readonly ProfilerRecorder[] markers=new ProfilerRecorder[6];
        bool visible,captureMenu;
        double phaseStart,nextAction;int phase,lastPhase,lastOperation,corner=1,saveCycle;
        Vector3 previousPosition;bool ready,finished;string directory,savePath;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Begin()
        {
            if(Application.isEditor || Array.IndexOf(Environment.GetCommandLineArgs(),"-sample-performance")<0)return;
            new GameObject("Sample performance diagnostic").AddComponent<SamplePerformanceCheck>();
        }
        void Awake(){Application.runInBackground=true;Application.logMessageReceived+=OnLog;}
        void OnLog(string message,string stack,LogType type)
        {if(type==LogType.Error || type==LogType.Exception || type==LogType.Assert)report.errors.Add(message+"\n"+stack);}
        void Start()
        {
            try
            {
                visible=Array.IndexOf(Environment.GetCommandLineArgs(),"-sample-visible")>=0;
                var args=Environment.GetCommandLineArgs();int capIndex=Array.IndexOf(args,"-sample-fps");
                if(capIndex>=0)Application.targetFrameRate=int.Parse(args[capIndex+1]);
                directory=Path.Combine(Path.GetDirectoryName(Application.dataPath),"Performance",Screen.width+"x"+Screen.height+(visible?"-visible-"+Application.targetFrameRate:""));
                Directory.CreateDirectory(directory);savePath=Path.Combine(directory,"benchmark-save.json");
                if(SceneManager.GetActiveScene().name!="Save01_Progress")throw new Exception("Wrong benchmark scene");
                actor=FindFirstObjectByType<PlayerInteraction>();movement=actor.GetComponent<FirstPersonController>();
                request=actor.GetComponent<SnackRequest>();stock=actor.GetComponent<SnackSupplies>();save=actor.GetComponent<SampleSaveGame>();clock=request.Clock;
                audio=actor.GetComponent<SampleAudio>();tray=FindFirstObjectByType<SnackAssembly>();box=FindFirstObjectByType<SnackSupplyBox>();inmate=FindFirstObjectByType<PrototypeInmate>();
                actor.GetComponent<SoloClockDriver>().enabled=false;
                var flags=BindingFlags.NonPublic|BindingFlags.Instance;
                capture=(Action<bool>)Delegate.CreateDelegate(typeof(Action<bool>),movement,typeof(FirstPersonController).GetMethod("SetCursorCaptured",flags));
                ((InputAction)typeof(FirstPersonController).GetField("lookAction",flags).GetValue(movement)).Disable();
                InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
                keyboard=InputSystem.AddDevice<Keyboard>();
                camera=actor.GetComponentInChildren<Camera>();
                if(!visible){target=new RenderTexture(Screen.width,Screen.height,24,RenderTextureFormat.ARGB32);target.Create();camera.targetTexture=target;}
                gcRecorder=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Allocated In Frame",1);
                drawRecorder=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Draw Calls Count",1);
                report.unity=Application.unityVersion;report.scene=SceneManager.GetActiveScene().name;
                report.cpu=SystemInfo.processorType;report.gpu=SystemInfo.graphicsDeviceName;report.graphics=SystemInfo.graphicsDeviceType.ToString();report.os=SystemInfo.operatingSystem;
                report.width=Screen.width;report.height=Screen.height;report.systemRamMB=SystemInfo.systemMemorySize;report.vramMB=SystemInfo.graphicsMemorySize;report.cpuThreads=SystemInfo.processorCount;
                report.vSync=QualitySettings.vSyncCount;report.targetFrameRate=Application.targetFrameRate;
                report.frameTimingEnabled=FrameTimingManager.IsFeatureEnabled();report.gcRecorderValid=gcRecorder.Valid;report.drawRecorderValid=drawRecorder.Valid;
                report.method="Hidden-window Development Player; explicit Camera.Render each LateUpdate to full-resolution texture. HUD/menu rendering and display presentation are NOT measured. Uncapped existing settings. Eight-second warmup, 20s walking, 20s accelerated action loop, 16s alternating save/load while paused, 20s post-load walking. Helper queues keyboard input and overrides focus pause. Inspection is advanced to allow repeat crafting. Save fixture is isolated. Editor remains open. Not shipping-build certification or a long leak soak. Working-set zero means unavailable from Mono; external process samples supplement it.";
                report.visible=visible;report.preferredFrameLimit=movement.FrameLimit;
                int originalLimit=movement.FrameLimit;int appliedLimit=Application.targetFrameRate;
                movement.SetFrameLimit(120,false);bool controls=Application.targetFrameRate==120;
                movement.SetFrameLimit(-1,false);controls &= Application.targetFrameRate==-1;
                movement.SetFrameLimit(17,false);controls &= Application.targetFrameRate==60;
                movement.SetFrameLimit(originalLimit,false);Application.targetFrameRate=appliedLimit;
                report.frameLimitControlsVerified=controls;
                if(!controls)throw new Exception("Frame-limit control verification failed");
                if(visible)report.method="Visible Development Player with normal camera rendering, HUD and settings UI; no explicit Camera.Render. Same scripted route/actions and isolated saves as offscreen baseline; Editor remains open. FrameTiming values have delayed availability: use frameStartTimestamp to align, not CSV row adjacency. Profiler marker values are previous-frame nanoseconds, -1 unavailable. Target frame rate recorded; no persistent settings changed.";
                audio.SetVolumes(.65f,.35f,false);Place(route[0]);previousPosition=actor.transform.position;
                phaseStart=Time.realtimeSinceStartupAsDouble;nextAction=phaseStart;ready=true;if(visible)StartCoroutine(CaptureWindow());
            }
            catch(Exception e){report.errors.Add(e.ToString());Finish();}
        }
        System.Collections.IEnumerator CaptureWindow()
        {
            yield return new WaitForSecondsRealtime(2);
            yield return new WaitForEndOfFrame();
            var screenshot=ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(directory,"window.png"),screenshot.EncodeToPNG());Destroy(screenshot);
            captureMenu=true;SetRunning(false);
            yield return null;
            yield return new WaitForEndOfFrame();
            screenshot=ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(directory,"settings.png"),screenshot.EncodeToPNG());Destroy(screenshot);
            captureMenu=false;SetRunning(true);
        }
        void Place(Vector3 p)
        {var cc=actor.GetComponent<CharacterController>();cc.enabled=false;actor.transform.position=p;cc.enabled=true;Physics.SyncTransforms();}
        void Input(bool walk)
        {InputSystem.QueueStateEvent(keyboard,walk?new KeyboardState(Key.W):new KeyboardState());InputSystem.Update();}
        void SetRunning(bool run)
        {capture(run);clock.SetPaused(!run);Time.timeScale=run?1:0;}
        void Update()
        {
            if(!ready || finished)return;
            try
            {
                double now=Time.realtimeSinceStartupAsDouble;
                if(lastPhase>0)
                {
                    double cpu=-1,gpu=-1;
                    if(report.frameTimingEnabled && FrameTimingManager.GetLatestTimings(1,timing)>0){cpu=timing[0].cpuFrameTime;gpu=timing[0].gpuFrameTime;}
                    frames.Add(new Frame{phase=lastPhase,operation=lastOperation,ms=Time.unscaledDeltaTime*1000,cpu=cpu,gpu=gpu,allocation=gcRecorder.Valid?gcRecorder.LastValue:-1,draws=drawRecorder.Valid?drawRecorder.LastValue:-1,
                        main=timing[0].cpuMainThreadFrameTime,render=timing[0].cpuRenderThreadFrameTime,presentWait=timing[0].cpuMainThreadPresentWaitTime,timestamp=timing[0].frameStartTimestamp,
                        loop=Marker(0),scripts=Marker(1),gui=Marker(2),gc=Marker(3),present=Marker(4),renderWait=Marker(5)});
                }
                lastOperation=0;
                FrameTimingManager.CaptureFrameTimings();
                double duration=phase==0?8:phase==3?16:20;
                if(now-phaseStart>=duration)
                {
                    Snapshot(names[phase]);if(phase==0)StartMarkers();phase++;phaseStart=now;nextAction=now+1;
                    if(phase==5){Finish();return;}
                    if(phase==2){Place(new Vector3(2.7f,.05f,3.1f));camera.transform.rotation=Quaternion.LookRotation(new Vector3(5,1.2f,5)-camera.transform.position);}
                    if(phase==4){Place(route[0]);corner=1;previousPosition=actor.transform.position;}
                    lastOperation=3;
                }
                lastPhase=phase;
                if(phase==3)
                {
                    SetRunning(false);Input(false);
                    if(now>=nextAction)
                    {
                        stopwatch.Restart();bool success;
                        if(saveCycle++%2==0){success=save.SaveTo(actor,savePath);report.saveMilliseconds.Add(stopwatch.Elapsed.TotalMilliseconds);lastOperation=1;}
                        else{success=save.LoadFrom(actor,savePath);report.loadMilliseconds.Add(stopwatch.Elapsed.TotalMilliseconds);lastOperation=2;}
                        if(!success)throw new Exception(save.Status);
                        nextAction=now+1;
                    }
                }
                else
                {
                    SetRunning(!captureMenu);clock.Advance(Time.unscaledDeltaTime);
                    bool walking=phase==1 || phase==4;
                    if(walking)
                    {
                        Vector3 delta=actor.transform.position-previousPosition;delta.y=0;report.walkedMetres+=delta.magnitude;previousPosition=actor.transform.position;
                        Vector3 offset=route[corner]-actor.transform.position;offset.y=0;
                        if(offset.magnitude<.25f){corner=(corner+1)%route.Length;report.walkCorners++;offset=route[corner]-actor.transform.position;offset.y=0;}
                        if(offset.sqrMagnitude>.001f)actor.transform.rotation=Quaternion.LookRotation(offset);
                    }
                    Input(walking);
                    if(phase==2 && now>=nextAction){Act();nextAction=now+.35;lastOperation=4;}
                }
            }
            catch(Exception e){report.errors.Add(e.ToString());Finish();}
        }
        void Act()
        {
            if(!request.Accepted){inmate.Interact(actor);return;}
            if(actor.HeldItem!=null){inmate.Interact(actor);return;}
            if(tray.Stage>0 || (stock.Crackers>0 && stock.Fruit>0 && stock.Wrappers>0)){tray.Interact(actor);return;}
            if(box.Closed){var inspection=FindFirstObjectByType<SupplyInspection>();clock.Advance((inspection.ReopensAt-clock.TotalMinutes)*5+.01);}
            box.Interact(actor);
        }
        void LateUpdate()
        {
            if(ready && !finished && !visible)camera.Render();
        }
        long Marker(int i)=>markers[i].Valid?markers[i].LastValue:-1;
        void StartMarkers()
        {
            var handles=new List<Unity.Profiling.LowLevel.Unsafe.ProfilerRecorderHandle>();
            Unity.Profiling.LowLevel.Unsafe.ProfilerRecorderHandle.GetAvailable(handles);
            var descriptions=handles.Select(Unity.Profiling.LowLevel.Unsafe.ProfilerRecorderHandle.GetDescription).ToArray();
            File.WriteAllLines(Path.Combine(directory,"available-markers.txt"),descriptions.Select(d=>d.Name));
            for(int i=0;i<markerNames.Length;i++)
            {
                foreach(var d in descriptions)if(d.Name==markerNames[i]){markers[i]=ProfilerRecorder.StartNew(d.Category,d.Name,1);break;}
                report.markerStatus.Add(markerNames[i]+": "+markers[i].Valid);
            }
        }
        void Snapshot(string label)
        {
            long working;using(var process=System.Diagnostics.Process.GetCurrentProcess())working=process.WorkingSet64;
            report.memory.Add(new MemoryPoint{phase=label,unityAllocated=Profiler.GetTotalAllocatedMemoryLong(),unityReserved=Profiler.GetTotalReservedMemoryLong(),monoUsed=Profiler.GetMonoUsedSizeLong(),processWorkingSet=working,gen0=GC.CollectionCount(0)});
        }
        static double P(double[] sorted,double p)=>sorted.Length==0?-1:sorted[Math.Min(sorted.Length-1,(int)Math.Ceiling(p*sorted.Length)-1)];
        void Finish()
        {
            if(finished)return;finished=true;
            try
            {
                if(actor!=null){report.sales=stock.Sales;report.footsteps=audio.FootstepsPlayed;report.soundCues=audio.PlayedSounds;capture(false);}
                if(ready && (report.walkedMetres<40 || report.walkCorners<8 || report.sales<5 || report.footsteps<20 || report.saveMilliseconds.Count<5 || report.loadMilliseconds.Count<5))report.errors.Add("Benchmark scenario coverage insufficient");
                foreach(int index in Enumerable.Range(1,4))
                {
                    var subset=frames.Where(f=>f.phase==index && f.operation!=3).ToArray();if(subset.Length==0)continue;
                    var ms=subset.Select(f=>f.ms).OrderBy(x=>x).ToArray();var cpus=subset.Where(f=>f.cpu>0).Select(f=>f.cpu).OrderBy(x=>x).ToArray();var gpus=subset.Where(f=>f.gpu>0).Select(f=>f.gpu).OrderBy(x=>x).ToArray();
                    report.phases.Add(new PhaseResult{phase=names[index],frames=ms.Length,averageMs=ms.Average(),averageFps=1000/ms.Average(),p50Ms=P(ms,.5),p95Ms=P(ms,.95),p99Ms=P(ms,.99),maxMs=ms.Last(),over16ms=ms.Count(x=>x>1000.0/60),over33ms=ms.Count(x=>x>1000.0/30),over50ms=ms.Count(x=>x>50),averageAllocationBytes=subset.Average(x=>(double)x.allocation),averageDrawCalls=subset.Average(x=>(double)x.draws),cpuP95Ms=P(cpus,.95),gpuP95Ms=P(gpus,.95),timingSamples=gpus.Length});
                }
                report.status=report.errors.Count==0?"COMPLETE":"FAILED";
                if(report.phases.Any(p=>p.averageDrawCalls<=0)){report.errors.Add("No measured rendering in one or more phases");report.status="FAILED";}
                Directory.CreateDirectory(directory);File.WriteAllText(Path.Combine(directory,"report.json"),JsonUtility.ToJson(report,true));
                using(var writer=new StreamWriter(Path.Combine(directory,"frames.csv")))
                {
                    writer.WriteLine("phase,previous_operation,frame_ms,cpu_ms,gpu_ms,gc_alloc_bytes,draw_calls,timing_timestamp,timing_main_ms,timing_render_ms,timing_present_wait_ms,loop_ns,scripts_ns,gui_ns,gc_ns,present_ns,render_wait_ns");
                    foreach(var f in frames)writer.WriteLine(string.Format(CultureInfo.InvariantCulture,"{0},{1},{2:F4},{3:F4},{4:F4},{5},{6},{7},{8:F4},{9:F4},{10:F4},{11},{12},{13},{14},{15},{16}",names[f.phase],f.operation,f.ms,f.cpu,f.gpu,f.allocation,f.draws,f.timestamp,f.main,f.render,f.presentWait,f.loop,f.scripts,f.gui,f.gc,f.present,f.renderWait));
                }
                if(target!=null)
                {
                    var prior=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,target.width,target.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(directory,"render.png"),image.EncodeToPNG());RenderTexture.active=prior;Destroy(image);
                }
            }
            catch(Exception e){Debug.LogError(e);Application.Quit(1);return;}
            Application.Quit(report.errors.Count==0?0:1);
        }
        void OnDestroy()
        {
            Application.logMessageReceived-=OnLog;gcRecorder.Dispose();drawRecorder.Dispose();for(int i=0;i<markers.Length;i++)markers[i].Dispose();
            if(keyboard!=null)InputSystem.RemoveDevice(keyboard);
            if(target!=null){if(camera!=null)camera.targetTexture=null;target.Release();Destroy(target);}
        }
    }
}
#endif
