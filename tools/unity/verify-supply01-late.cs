// Supply closure, dependency and reopening integration. Run fresh Supply01 Play.
if(!EditorApplication.isPlaying)throw new System.Exception("Enter Play first.");
var player=GameObject.Find("Player");var actor=player.GetComponent<PrisonGame.Prototype.PlayerInteraction>();var stock=player.GetComponent<PrisonGame.Prototype.SnackSupplies>();var movement=player.GetComponent<PrisonGame.Prototype.FirstPersonController>();var cc=player.GetComponent<CharacterController>();var cam=player.GetComponentInChildren<Camera>();
var box=UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.SnackSupplyBox>();var tray=UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.SnackAssembly>();var shelf=UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.SnackShelf>();var inmate=UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.PrototypeInmate>();var hud=player.GetComponent<PrisonGame.Prototype.SnackLoopHud>();
if(stock.StarterCollected||actor.HeldItem!=null)throw new System.Exception("Use fresh Play state.");
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;var capture=movement.GetType().GetMethod("SetCursorCaptured",flags);var tick=actor.GetType().GetMethod("Update",flags);var seek=actor.GetType().GetMethod("FindTarget",flags);
var start=player.transform.position;var rotation=player.transform.rotation;var cameraRotation=cam.transform.localRotation;
var settings=UnityEngine.InputSystem.InputSystem.settings;var bg=settings.backgroundBehavior;var input=settings.editorInputBehaviorInPlayMode;UnityEngine.InputSystem.Keyboard keyboard=null;
var results=new System.Collections.Generic.List<string>();void Check(bool value,string text){if(!value)throw new System.Exception("FAIL: "+text);results.Add("PASS: "+text);}
void Press(UnityEngine.InputSystem.Key key){UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState());UnityEngine.InputSystem.InputSystem.Update();UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(key));UnityEngine.InputSystem.InputSystem.Update();tick.Invoke(actor,null);Physics.SyncTransforms();}
void Use(PrisonGame.Prototype.PrototypeInteractable target,Vector3 place,Vector3 aim){cc.enabled=false;player.transform.position=place;cc.enabled=true;cam.transform.rotation=Quaternion.LookRotation(aim-cam.transform.position);Physics.SyncTransforms();capture.Invoke(movement,new object[]{true});seek.Invoke(actor,null);if(actor.Target!=target)throw new System.Exception("Cannot target "+target.name+"; hit "+(actor.Target==null?"nothing":actor.Target.name));Press(UnityEngine.InputSystem.Key.E);}
void Supply()=>Use(box,new Vector3(2.7f,.05f,3.8f),box.transform.position);
void Assemble(){for(int i=0;i<3;i++)Use(tray,new Vector3(2.7f,.05f,3.8f),tray.transform.position);}
void Sell()=>Use(inmate,new Vector3(2.4f,.05f,3.5f),inmate.transform.position+Vector3.up*1.2f);
void Shelf()=>Use(shelf,new Vector3(-3.4f,.05f,-1.3f),new Vector3(-3.08f,1.47f,-2.85f));
var request=player.GetComponent<PrisonGame.Prototype.SnackRequest>();
var buyer=inmate.GetComponent<PrisonGame.Prototype.SnackPackBuyer>();
var supply=player.GetComponent<PrisonGame.Prototype.RequestSupplyStatus>();var inspection=supply.Inspection;var clock=request.Clock;var driver=player.GetComponent<PrisonGame.Prototype.SoloClockDriver>();var suspicion=player.GetComponent<PrisonGame.Prototype.GuardSuspicion>();
var hudTick=hud.GetType().GetMethod("Update",flags);var inspectTick=inspection.GetType().GetMethod("Update",flags);bool driverEnabled=driver.enabled;GameObject second=null,extraPack=null;
var sign=(GameObject)new SerializedObject(inspection).FindProperty("closedSign").objectReferenceValue;
try{
    settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();driver.enabled=false;clock.SetPaused(false);
    Sell();Supply();Assemble();clock.Advance(55*5);Sell();double originalDue=request.DueAt;
    Check(inspection.Closed && request.Delivered==1 && stock.Money==3 && !request.IsLate,"A first delivery near the deadline starts inspection and still pays $3");
    Assemble();clock.Advance((request.DueAt-clock.TotalMinutes)*5);
    Check(inspection.Closed && request.IsLate && request.DeliveryPrice==2,"Deadline can expire while the supply box remains closed");
    Check(hud.RequestStatus.Contains("Late packs now pay $2") && hud.RequestStatus.Contains("Restock blocked"),"Request feedback shows late payment and supply shortage together");
    Sell();Check(request.Delivered==2 && stock.Money==5 && request.Paid==5,"Existing finished packs remain deliverable during closure at the late price");
    Supply();Check(stock.Money==5 && stock.Crackers==0,"Closed box does not charge even after the order is overdue");
    clock.Advance((inspection.ReopensAt-clock.TotalMinutes)*5);inspectTick.Invoke(inspection,null);hudTick.Invoke(hud,null);
    Check(!inspection.Closed && request.DueAt==originalDue && request.IsLate && request.DeliveryPrice==2,"Reopening never extends the request deadline or restores the on-time price");
    Check(!hud.RequestStatus.Contains("Restock blocked") && hud.RequestStatus.Contains("Late packs now pay $2"),"Only the resolved supply problem clears; the overdue status remains");
    Supply();Assemble();Sell();Check(request.Complete && request.CompletedLate && request.Paid==7 && stock.Money==6,"The interrupted late request completes with the actual mixed $7 total and $1 refill cost");
    Check(hud.RequestStatus.Contains("$7 paid") && hud.SupplyStatus.Contains("open again"),"Completion and reopened-supply feedback both reflect final state");
}finally{
    if(keyboard!=null)UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);settings.backgroundBehavior=bg;settings.editorInputBehaviorInPlayMode=input;capture.Invoke(movement,new object[]{false});Time.timeScale=1;driver.enabled=driverEnabled;cc.enabled=false;player.transform.SetPositionAndRotation(start,rotation);cc.enabled=true;cam.transform.localRotation=cameraRotation;Physics.SyncTransforms();
}
System.IO.File.WriteAllLines("../docs/images/supply01-late-checks.txt",results);return string.Join("\n",results);
