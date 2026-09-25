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
    Check(!inspection.Started && !box.Closed && !sign.activeSelf,"Fresh study begins with an open box and no inspection notice");
    Sell();double due=request.DueAt;Supply();Check(stock.Crackers==2 && supply.CraftablePacks==2 && supply.PacksNeedingSupplies==1,"Starter stock covers two of three requested packs and identifies one missing recipe");
    Use(tray,new Vector3(2.7f,.05f,3.8f),tray.transform.position);Check(tray.Stage==1 && supply.CraftablePacks==2,"Crackers already on the tray still count toward a finishable pack");
    Use(tray,new Vector3(2.7f,.05f,3.8f),tray.transform.position);Check(tray.Stage==2 && supply.CraftablePacks==2,"Fruit already on the tray is not double-counted or lost from supply progress");
    Use(tray,new Vector3(2.7f,.05f,3.8f),tray.transform.position);Check(supply.CraftablePacks==1 && request.ReadyPacks==1 && supply.PacksNeedingSupplies==1,"Finished pack moves from craftable to ready without changing the shortage");
    Sell();hudTick.Invoke(hud,null);double reopening=inspection.ReopensAt;
    Check(inspection.Closed && box.Closed && sign.activeSelf && reopening==clock.TotalMinutes+10,"First actual sale starts one ten-game-minute inspection and activates its physical notice");
    Check(stock.Money==3 && request.Delivered==1 && request.DueAt==due,"Inspection preserves sale payment, delivered count and original deadline");
    Check(supply.RestockBlocked && supply.PacksNeedingSupplies==1 && hud.RequestStatus.Contains("Restock blocked") && hud.RequestStatus.Contains("Deadline continues"),"Request explicitly links its missing recipe to the closure and continuing deadline");
    Check(hud.NextAction.Contains("assemble") && hud.SupplyStatus.Contains("routine stock inspection") && hud.SupplyStatus.Contains("Reopens"),"Guidance uses available stock while the closure panel states cause and reopening time");
    actor.ShowMessage("sentinel");hudTick.Invoke(hud,null);Check((string)actor.GetType().GetField("message",flags).GetValue(actor)=="sentinel","Closure announcement does not repeat every frame");
    int money=stock.Money,crackers=stock.Crackers;Supply();Supply();Check(stock.Money==money && stock.Crackers==crackers && stock.Fruit==1 && stock.Wrappers==1,"Repeated real E at the closed box neither charges money nor adds ingredients");
    Check(box.Prompt(actor).Contains("inspection") && !inspection.Begin(actor) && inspection.ReopensAt==reopening,"Prompt explains closure and repeated triggers cannot extend it");
    second=new GameObject("Other actor fixture");second.transform.position=new Vector3(50,0,0);var otherCam=new GameObject("Camera").AddComponent<Camera>();otherCam.enabled=false;otherCam.transform.SetParent(second.transform);var other=second.AddComponent<PrisonGame.Prototype.PlayerInteraction>();second.GetComponent<PrisonGame.Prototype.FirstPersonController>().enabled=false;var otherStock=second.AddComponent<PrisonGame.Prototype.SnackSupplies>();
    box.Interact(other);Check(!otherStock.StarterCollected && otherStock.Money==0,"Shared closure also blocks another actor's free starter collection");
    Check(!inspection.Begin(other) && inspection.ReopensAt==reopening,"Another actor cannot reset the controlled inspection trigger");
    // A physical ready-pack fixture models existing stock from another source, independent of this box.
    var prefab=(GameObject)new SerializedObject(tray).FindProperty("packPrefab").objectReferenceValue;extraPack=UnityEngine.Object.Instantiate(prefab,new Vector3(-4,1,-1),Quaternion.identity);extraPack.GetComponent<PrisonGame.Prototype.SnackPackItem>().Initialize(stock);
    Check(supply.PacksNeedingSupplies==0 && !supply.RestockBlocked && hud.RequestStatus.Contains("enough packs/supplies"),"Existing physical stock that covers the order removes the shortage warning during closure");
    UnityEngine.Object.DestroyImmediate(extraPack);extraPack=null;Check(supply.RestockBlocked,"Removing ready stock automatically restores the actual shortage warning");
    Use(tray,new Vector3(2.7f,.05f,3.8f),tray.transform.position);Check(supply.CraftablePacks==1 && supply.PacksNeedingSupplies==1,"An unfinished pack during closure remains accounted for correctly");
    Use(tray,new Vector3(2.7f,.05f,3.8f),tray.transform.position);Use(tray,new Vector3(2.7f,.05f,3.8f),tray.transform.position);var placed=actor.HeldItem;
    cam.transform.rotation=Quaternion.LookRotation(new Vector3(3.9f,.84f,4.25f)-cam.transform.position);Physics.SyncTransforms();Check(actor.TryPutDown() && request.ReadyPacks==1 && hud.NextAction.Contains("Pick up"),"A stored pack remains usable and guidance retrieves it instead of waiting for supplies");
    Use(placed,new Vector3(2.7f,.05f,3.8f),placed.transform.position);Sell();Check(request.Delivered==2 && stock.Money==6 && inspection.ReopensAt==reopening,"Second delivery works during closure without restarting the inspection");
    Check(hud.NextAction.Contains("reopens") && supply.PacksNeedingSupplies==1,"With all usable stock exhausted, guidance gives the reopening time");
    double left=request.MinutesLeft,inspectionLeft=inspection.MinutesLeft;clock.SetPaused(true);clock.Advance(120);Check(request.MinutesLeft==left && inspection.MinutesLeft==inspectionLeft,"Pause freezes the inspection and request countdown together");clock.SetPaused(false);
    cc.enabled=false;player.transform.position=new Vector3(6.8f,.05f,5.7f);cc.enabled=true;Physics.SyncTransforms();suspicion.Step(4);clock.Advance(5);Check(suspicion.Level>=49.99f && inspection.Closed && request.MinutesLeft<left,"Guard suspicion, supply closure and a running request deadline can coexist");
    clock.Advance((inspection.ReopensAt-clock.TotalMinutes-.001)*5);Supply();Check(box.Closed && stock.Money==6 && stock.Crackers==0,"Supply access remains blocked immediately before the reopening boundary");
    clock.Advance((inspection.ReopensAt-clock.TotalMinutes)*5);inspectTick.Invoke(inspection,null);hudTick.Invoke(hud,null);
    Check(!inspection.Closed && !box.Closed && !sign.activeSelf && inspection.MinutesLeft==0,"At the exact reopening boundary access returns and the physical closed notice disappears");
    Check(!supply.RestockBlocked && !hud.RequestStatus.Contains("Restock blocked") && hud.NextAction.Contains("Restock") && hud.SupplyStatus.Contains("open again"),"Reopening clears the blocked-request status and directs normal restocking");
    actor.ShowMessage("sentinel");hudTick.Invoke(hud,null);Check((string)actor.GetType().GetField("message",flags).GetValue(actor)=="sentinel","Reopening announcement is not repeated every frame");
    Supply();Check(stock.Money==5 && supply.CraftablePacks==1 && supply.PacksNeedingSupplies==0,"Normal $1 restock resumes and updates the request's supply coverage");
    Assemble();Sell();Check(request.Complete && request.Paid==9 && stock.Money==8 && !inspection.Closed,"Request can finish on time after the interruption, with normal earnings");
    Shelf();Check(shelf.Installed && stock.Money==0,"The full interrupted loop still reaches the shelf reward");
    Supply();Assemble();Sell();Check(!inspection.Closed && inspection.ReopensAt==reopening && stock.Money==3,"Later sales do not repeat the one-off inspection; normal trading continues");
    box.Interact(other);Check(otherStock.StarterCollected && otherStock.Crackers==2,"Reopening restores supply access for the other actor too");
    results.Add("INFO: Real E targeting, assembly, sale and placement; controlled game-clock boundary advances. One extra physical pack fixture tests sufficient pre-existing stock; no extra supplies granted in the saved scene. Stop Play resets test state.");
}finally{
    if(second!=null)UnityEngine.Object.DestroyImmediate(second);if(extraPack!=null)UnityEngine.Object.DestroyImmediate(extraPack);
    if(keyboard!=null)UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);settings.backgroundBehavior=bg;settings.editorInputBehaviorInPlayMode=input;capture.Invoke(movement,new object[]{false});Time.timeScale=1;driver.enabled=driverEnabled;cc.enabled=false;player.transform.SetPositionAndRotation(start,rotation);cc.enabled=true;cam.transform.localRotation=cameraRotation;Physics.SyncTransforms();
}
System.IO.File.WriteAllLines("../docs/images/supply01-inspection-checks.txt",results);return string.Join("\n",results);
