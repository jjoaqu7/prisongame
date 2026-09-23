// Pipeline eval_file integration checks. Run in Play mode in Room01_Blockout.
if(!EditorApplication.isPlaying)throw new System.Exception("Enter Play mode first.");
var player=GameObject.Find("Player");
var movement=player.GetComponent<PrisonGame.Prototype.FirstPersonController>();
var interaction=player.GetComponent<PrisonGame.Prototype.PlayerInteraction>();
var cc=player.GetComponent<CharacterController>();var cam=player.GetComponentInChildren<Camera>();
var door=GameObject.Find("Cell sliding door").GetComponent<PrisonGame.Prototype.PrototypeDoor>();
var parcel=UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.PrototypePickup>();
var inmate=UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.PrototypeInmate>();
if(interaction.HeldItem!=null)throw new System.Exception("Put the parcel down before running verification.");
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var seek=interaction.GetType().GetMethod("FindTarget",flags);
var move=movement.GetType().GetMethod("StepMovement",flags);
var doorTick=door.GetType().GetMethod("Update",flags);
var opening=door.GetType().GetField("opening",flags);
var closed=(Vector3)door.GetType().GetField("closedPosition",flags).GetValue(door);
var startP=player.transform.position;var startR=player.transform.rotation;var startCam=cam.transform.localRotation;
var startDoor=door.transform.position;var startOpen=opening.GetValue(door);
var parcelParent=parcel.transform.parent;var parcelP=parcel.transform.position;var parcelR=parcel.transform.rotation;
var rb=parcel.GetComponent<Rigidbody>();bool startKinematic=rb.isKinematic;
bool movementEnabled=movement.enabled;bool interactionEnabled=interaction.enabled;bool doorEnabled=door.enabled;
var inputSettings=UnityEngine.InputSystem.InputSystem.settings;
var previousBackground=inputSettings.backgroundBehavior;
var previousEditorInput=inputSettings.editorInputBehaviorInPlayMode;
UnityEngine.InputSystem.Keyboard keyboard=null;
var capture=movement.GetType().GetMethod("SetCursorCaptured",flags);
var interactionTick=interaction.GetType().GetMethod("Update",flags);
var results=new System.Collections.Generic.List<string>();
void Check(bool condition,string text){if(!condition)throw new System.Exception("FAIL: "+text);results.Add("PASS: "+text);}
void Place(Vector3 p,float yaw=90){cc.enabled=false;player.transform.SetPositionAndRotation(p,Quaternion.Euler(0,yaw,0));cc.enabled=true;cam.transform.localRotation=Quaternion.identity;Physics.SyncTransforms();}
void Aim(Vector3 point){cam.transform.rotation=Quaternion.LookRotation(point-cam.transform.position);Physics.SyncTransforms();seek.Invoke(interaction,null);}
void TickDoor(){for(int n=0;n<250;n++){doorTick.Invoke(door,null);Physics.SyncTransforms();}}
try {
    movement.enabled=false;interaction.enabled=false;door.enabled=false;
    Place(new Vector3(-3.4f,.05f,-.5f));door.transform.position=closed;opening.SetValue(door,false);Physics.SyncTransforms();
    Aim(closed+Vector3.up*.4f);Check(interaction.Target==door,"Door target and open prompt available at spawn");
    Check(door.Prompt(interaction).Contains("Open"),"Closed door has open prompt");
    for(int n=0;n<90;n++)move.Invoke(movement,new object[]{Vector2.up,1f/60});
    Check(player.transform.position.x < -2.3f,"Closed door physically blocks passage");
    Place(new Vector3(-3.4f,.05f,-.5f));door.Interact(interaction);TickDoor();
    Check(door.IsOpen,"Door slides fully open");
    for(int n=0;n<58;n++)move.Invoke(movement,new object[]{Vector2.up,1f/60});
    Check(player.transform.position.x>-.7f,"Open door permits controller passage");
    Place(new Vector3(-.7f,.05f,.45f),270);Aim(new Vector3(-1.82f,1.35f,.45f));
    Check(interaction.Target is PrisonGame.Prototype.PrototypeDoorControl,"Hall button remains reachable when door is open");
    interaction.Target.Interact(interaction);TickDoor();
    Check(Vector3.Distance(door.transform.position,closed)<.01f,"Hall button closes door");
    Place(new Vector3(-3.4f,.05f,.45f));Aim(new Vector3(-2.18f,1.35f,.45f));
    Check(interaction.Target is PrisonGame.Prototype.PrototypeDoorControl,"Cell button can reopen door");
    interaction.Target.Interact(interaction);TickDoor();
    Place(new Vector3(-2,.05f,-.5f));door.Interact(interaction);TickDoor();
    Check(door.IsOpen,"Door refuses to close on player");
    Place(new Vector3(-3.4f,.05f,-.5f));door.Interact(interaction);
    Place(new Vector3(-2,.05f,-.5f));TickDoor();
    Check(door.IsOpen,"Entering during closing reopens door");

    Place(new Vector3(-3.1f,.05f,.1f),0);Aim(parcel.transform.position);
    Check(interaction.Target==parcel,"Parcel target on desk");interaction.Target.Interact(interaction);
    Check(interaction.HeldItem==parcel && !parcel.GetComponent<BoxCollider>().enabled && rb.isKinematic,"Pickup attaches parcel without carried collision");
    interaction.PickUp(parcel);Check(interaction.HeldItem==parcel,"Full hands retain the existing item");
    Aim(new Vector3(-3.1f,.83f,1.5f));
    Check(interaction.TryPutDown(),"Parcel can be placed back on desk");
    Check(interaction.HeldItem==null && parcel.GetComponent<BoxCollider>().enabled && !rb.isKinematic,"Placed parcel restores physics and clears hands");
    Physics.SyncTransforms();Aim(parcel.transform.position);Check(interaction.Target==parcel,"Placed parcel can be targeted again");
    interaction.Target.Interact(interaction);
    Place(new Vector3(.5f,.05f,5.4f),90);Aim(new Vector3(2,1.7f,5.4f));
    Check(!interaction.TryPutDown() && interaction.HeldItem==parcel,"Drop across a wall is refused without losing parcel");
    Place(new Vector3(-.5f,.05f,-4),0);Aim(new Vector3(-.5f,0,-3.1f));
    Check(interaction.TryPutDown(),"Parcel can be placed on clear corridor floor");

    Place(new Vector3(.4f,.05f,5.4f));Aim(inmate.transform.position+Vector3.up*1.2f);
    Check(interaction.Target==null,"Solid wall blocks inmate interaction");
    Place(new Vector3(2.4f,.05f,1),0);Aim(inmate.transform.position+Vector3.up*1.2f);
    Check(interaction.Target!=inmate,"Inmate beyond reach cannot be targeted");
    Place(new Vector3(2.4f,.05f,3.5f),0);Aim(inmate.transform.position+Vector3.up*1.2f);
    Check(interaction.Target==inmate,"Nearby inmate can be targeted");interaction.Target.Interact(interaction);
    var message=(string)interaction.GetType().GetField("message",flags).GetValue(interaction);
    Check(message.StartsWith("Inmate:"),"Talking produces visible dialogue text");

    inputSettings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
    inputSettings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
    keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
    void Press(UnityEngine.InputSystem.Key key) {
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState());
        UnityEngine.InputSystem.InputSystem.Update();
        UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(key));
        UnityEngine.InputSystem.InputSystem.Update();
        interactionTick.Invoke(interaction,null);
    }
    Place(new Vector3(-.5f,.05f,-4),0);Aim(parcel.transform.position);
    capture.Invoke(movement,new object[]{false});Press(UnityEngine.InputSystem.Key.E);
    Check(interaction.HeldItem==null,"E does not interact while settings are open");
    capture.Invoke(movement,new object[]{true});Press(UnityEngine.InputSystem.Key.E);
    Check(interaction.HeldItem==parcel,"E keyboard event picks up targeted parcel");
    capture.Invoke(movement,new object[]{false});Press(UnityEngine.InputSystem.Key.Q);
    Check(interaction.HeldItem==parcel,"Q does not drop while settings are open");
    capture.Invoke(movement,new object[]{true});Press(UnityEngine.InputSystem.Key.Q);
    Check(interaction.HeldItem==null,"Q keyboard event puts parcel down");
    return string.Join("\n",results);
} finally {
    if(keyboard!=null)UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);
    inputSettings.backgroundBehavior=previousBackground;
    inputSettings.editorInputBehaviorInPlayMode=previousEditorInput;
    capture.Invoke(movement,new object[]{false});
    if(interaction.HeldItem!=null)interaction.GetType().GetProperty("HeldItem").GetSetMethod(true).Invoke(interaction,new object[]{null});
    rb.isKinematic=true;parcel.transform.SetParent(parcelParent,true);parcel.transform.SetPositionAndRotation(parcelP,parcelR);parcel.GetComponent<BoxCollider>().enabled=true;rb.isKinematic=startKinematic;
    door.transform.position=startDoor;opening.SetValue(door,startOpen);door.enabled=doorEnabled;
    cc.enabled=false;player.transform.SetPositionAndRotation(startP,startR);cc.enabled=true;cam.transform.localRotation=startCam;
    movement.enabled=movementEnabled;interaction.enabled=interactionEnabled;Physics.SyncTransforms();
}
