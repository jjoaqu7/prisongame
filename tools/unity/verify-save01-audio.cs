if(!EditorApplication.isPlaying)throw new System.Exception("Enter fresh Save01 Play.");
var player=GameObject.Find("Player");var audio=player.GetComponent<PrisonGame.Prototype.SampleAudio>();var actor=player.GetComponent<PrisonGame.Prototype.PlayerInteraction>();var stock=player.GetComponent<PrisonGame.Prototype.SnackSupplies>();var request=player.GetComponent<PrisonGame.Prototype.SnackRequest>();var clock=request.Clock;var movement=player.GetComponent<PrisonGame.Prototype.FirstPersonController>();var driver=player.GetComponent<PrisonGame.Prototype.SoloClockDriver>();var tray=UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.SnackAssembly>();var box=UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.SnackSupplyBox>();var saver=player.GetComponent<PrisonGame.Prototype.SampleSaveGame>();
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var voices=(AudioSource[])audio.GetType().GetField("voices",flags).GetValue(audio);var ambient=(AudioSource)audio.GetType().GetField("ambience",flags).GetValue(audio);
void Invoke(string name,params object[] args)=>audio.GetType().GetMethod(name,flags).Invoke(audio,args);
var result=new System.Collections.Generic.List<string>();void Check(bool ok,string name){if(!ok)throw new System.Exception("FAIL: "+name);result.Add("PASS: "+name);}
float effects=audio.EffectsVolume,ambience=audio.AmbienceVolume;var pos=player.transform.position;
driver.enabled=false;movement.GetType().GetMethod("SetCursorCaptured",flags).Invoke(movement,new object[]{false});clock.SetPaused(true);Time.timeScale=0;Invoke("SetPaused",true);
try{
 Check(audio!=null && voices.Length==8,"One audio presenter uses eight reusable voices");
 Check(UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length==1,"Scene has one audio listener");
 Check(audio.Paused && !ambient.isPlaying && ambient.loop,"Settings pause the looping ambience");
 int clips=0;foreach(var guid in AssetDatabase.FindAssets("t:AudioClip",new[]{"Assets/Prototype/Audio01"})){
 var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guid));var data=new float[clip.samples*clip.channels];Check(clip.loadState==AudioDataLoadState.Loaded && clip.GetData(data,0) && data.Any(x=>Mathf.Abs(x)>.001f) && data.All(x=>float.IsFinite(x)&&Mathf.Abs(x)<.98f),"Decoded non-silent unclipped clip: "+clip.name);clips++;
 }Check(clips==14,"All fourteen licensed clips imported");
 audio.SetVolumes(.65f,.35f,false);driver.enabled=false;clock.SetPaused(false);Time.timeScale=1;Invoke("SetPaused",false);
 Check(ambient.isPlaying,"Resume starts the room ambience");
 int count=audio.PlayedSounds;tray.Interact(actor);Check(audio.PlayedSounds==count,"Missing ingredients do not play successful assembly audio");
 box.Interact(actor);Check(audio.PlayedSounds==count+1,"Successful starter collection plays one restock cue");
 count=audio.PlayedSounds;tray.Interact(actor);tray.Interact(actor);tray.Interact(actor);Check(audio.PlayedSounds==count+3,"Three assembly stages each play once; wrapping does not double-play pickup");
 var parcel=UnityEngine.Object.FindObjectsByType<PrisonGame.Prototype.PrototypePickup>(FindObjectsSortMode.None).First(x=>x.GetComponent<PrisonGame.Prototype.SnackPackItem>()==null);count=audio.PlayedSounds;actor.PickUp(parcel);Check(audio.PlayedSounds==count,"Full hands do not play a pickup cue");
 request.Accept(actor,request.Customer);request.Deliver(actor,request.Customer);Check(audio.PlayedSounds==count+1,"Successful sale plays one confirmation");count=audio.PlayedSounds;box.Interact(actor);Check(audio.PlayedSounds==count,"Inspection-blocked collection stays silent");
 var door=UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.PrototypeDoor>();door.Interact(actor);Check(audio.PlayedSounds==count+1,"Door interaction plays a muted metal cue");
 var latest=voices.First(v=>v.isPlaying && v.clip.name=="door");Vector3 worldPosition=latest.transform.position;player.transform.position+=Vector3.right*.5f;Check(latest.transform.position==worldPosition && latest.spatialBlend==1,"World sound stays at its source when the player moves");
 int steps=audio.FootstepsPlayed;Vector3 point=Vector3.zero;Invoke("MeasureSteps",point,true,true);for(int i=0;i<10;i++)Invoke("MeasureSteps",point,true,false);Check(audio.FootstepsPlayed==steps,"Standing or pushing without movement produces no footsteps");
 for(int i=0;i<3;i++){point+=Vector3.right*.5f;Invoke("MeasureSteps",point,true,false);}Check(audio.FootstepsPlayed==steps+1,"Grounded travelled distance produces a footstep");
 steps=audio.FootstepsPlayed;Invoke("MeasureSteps",point+Vector3.right*10,true,false);Invoke("MeasureSteps",point+Vector3.right*10.5f,false,false);Check(audio.FootstepsPlayed==steps,"Teleport and airborne motion do not produce footsteps");
 clock.SetPaused(true);Invoke("SetPaused",true);Check(!ambient.isPlaying && voices.All(v=>!v.isPlaying),"Pause stops ambience and active voices");count=audio.PlayedSounds;PrisonGame.Prototype.SampleSoundEvents.Emit(actor,PrisonGame.Prototype.SampleSound.Sale,player.transform.position);Check(audio.PlayedSounds==count,"Paused world cues are ignored");
 var saved=saver.Capture(actor);saver.Restore(actor,saved);Check(audio.PlayedSounds==count && voices.All(v=>!v.isPlaying),"Loading is silent and clears pre-load sounds");
 clock.SetPaused(false);Invoke("SetPaused",false);audio.SetVolumes(0,0,false);count=audio.PlayedSounds;PrisonGame.Prototype.SampleSoundEvents.Emit(actor,PrisonGame.Prototype.SampleSound.Pickup,player.transform.position);Check(audio.PlayedSounds==count && ambient.volume==0,"Both volume controls can mute their channels");
 audio.SetVolumes(.65f,.35f,false);audio.enabled=false;count=audio.PlayedSounds;PrisonGame.Prototype.SampleSoundEvents.Emit(actor,PrisonGame.Prototype.SampleSound.Place,player.transform.position);Check(audio.PlayedSounds==count,"Disabled presenter unsubscribes from cues");audio.enabled=true;Invoke("SetPaused",false);PrisonGame.Prototype.SampleSoundEvents.Emit(actor,PrisonGame.Prototype.SampleSound.Place,player.transform.position);Check(audio.PlayedSounds==count+1 && ambient.isPlaying,"Re-enabled presenter restores ambience without duplicate subscriptions");
 Check(voices.All(v=>v.dopplerLevel==0 && v.maxDistance==10),"World sources have bounded distance and no doppler pitch jumps");
}finally{audio.SetVolumes(effects,ambience,false);clock.SetPaused(true);Invoke("SetPaused",true);movement.GetType().GetMethod("SetCursorCaptured",flags).Invoke(movement,new object[]{false});driver.enabled=true;}
System.IO.File.WriteAllLines("../docs/evidence/audio01-checks.txt",result);return string.Join("\n",result);
