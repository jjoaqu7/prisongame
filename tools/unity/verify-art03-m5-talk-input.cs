if(!EditorApplication.isPlaying)throw new System.Exception("Enter Play mode first.");
var player=GameObject.Find("Player");var movement=player.GetComponent<PrisonGame.Prototype.FirstPersonController>();var interaction=player.GetComponent<PrisonGame.Prototype.PlayerInteraction>();var cc=player.GetComponent<CharacterController>();var cam=player.GetComponentInChildren<Camera>();
var motion=UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.InmateMotion>();var presentation=motion.GetComponentInChildren<PrisonGame.Prototype.InmateAnimation>();var animator=presentation.GetComponent<Animator>();
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var capture=movement.GetType().GetMethod("SetCursorCaptured",flags);var tick=interaction.GetType().GetMethod("Update",flags);
var oldPosition=player.transform.position;var oldRotation=player.transform.rotation;var oldCam=cam.transform.localRotation;var oldInmate=motion.transform.rotation;
var settings=UnityEngine.InputSystem.InputSystem.settings;var bg=settings.backgroundBehavior;var editor=settings.editorInputBehaviorInPlayMode;
var mode=animator.cullingMode;UnityEngine.InputSystem.Keyboard keyboard=null;var results=new System.Collections.Generic.List<string>();
void Check(bool value,string message){if(!value)throw new System.Exception("FAIL: "+message);results.Add("PASS: "+message);}
void Press(){UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState());UnityEngine.InputSystem.InputSystem.Update();UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.E));UnityEngine.InputSystem.InputSystem.Update();tick.Invoke(interaction,null);}
try{
    cc.enabled=false;player.transform.position=motion.transform.position+new Vector3(0,.05f,-1.8f);cc.enabled=true;
    cam.transform.rotation=Quaternion.LookRotation(motion.transform.position+Vector3.up*1.3f-cam.transform.position);Physics.SyncTransforms();
    settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
    keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();int sequence=motion.TalkSequence;
    capture.Invoke(movement,new object[]{false});Press();Check(motion.TalkSequence==sequence,"E with settings open does not start a gesture");
    capture.Invoke(movement,new object[]{true});Press();Check(motion.TalkSequence==sequence+1&&motion.IsGesturing,"E on the inmate starts the gesture through the real player interaction path");
    animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;presentation.RefreshAnimation();animator.Update(.3f);presentation.GetComponent<PrisonGame.Prototype.RigFloorClearance>().ApplyFloorClearance();
    Check(animator.GetCurrentAnimatorStateInfo(0).IsName("GesturePreview"),"E-triggered gesture reaches the actual Animator state");
    var message=(string)interaction.GetType().GetField("message",flags).GetValue(interaction);Check(message.StartsWith("Inmate:"),"E still presents the inmate response");
}finally{
    if(keyboard!=null)UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);settings.backgroundBehavior=bg;settings.editorInputBehaviorInPlayMode=editor;capture.Invoke(movement,new object[]{false});
    cc.enabled=false;player.transform.SetPositionAndRotation(oldPosition,oldRotation);cc.enabled=true;cam.transform.localRotation=oldCam;motion.transform.rotation=oldInmate;motion.GetType().GetField("talkRemaining",flags).SetValue(motion,0f);
    presentation.RefreshAnimation();animator.Play("Idle",0,0);animator.Update(0);presentation.GetComponent<PrisonGame.Prototype.RigFloorClearance>().ApplyFloorClearance();animator.cullingMode=mode;Physics.SyncTransforms();
}
System.IO.File.WriteAllLines("../docs/images/art03/m5-talk-input-checks.txt",results);return string.Join("\n",results);
