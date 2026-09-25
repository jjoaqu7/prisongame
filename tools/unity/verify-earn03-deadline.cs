// Deadline boundary and solo pause integration. Run fresh Earn03 Play.
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
var clock=request.Clock;var driver=player.GetComponent<PrisonGame.Prototype.SoloClockDriver>();
var driverTick=driver.GetType().GetMethod("Update",flags);var focus=driver.GetType().GetMethod("OnApplicationFocus",flags);var hudTick=hud.GetType().GetMethod("Update",flags);
bool driverEnabled=driver.enabled;float initialScale=Time.timeScale;
void Resume(){focus.Invoke(driver,new object[]{true});capture.Invoke(movement,new object[]{true});driverTick.Invoke(driver,null);}
try{
    settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
    Check(clock!=null && clock.Paused && Time.timeScale==0 && !request.Accepted,"Fresh timed scene starts paused in settings without an accepted request");
    driver.enabled=false;
    capture.Invoke(movement,new object[]{false});driverTick.Invoke(driver,null);double before=clock.TotalMinutes;clock.Advance(60);
    Check(clock.TotalMinutes==before && Time.timeScale==0,"Settings pause both the world simulation and prison clock");
    driver.GetType().GetField("lastSample",flags).SetValue(driver,Time.realtimeSinceStartupAsDouble-600);Resume();Check(clock.TotalMinutes==before,"First resumed frame discards the paused interval instead of charging it");Check(!clock.Paused && Time.timeScale==1,"Resuming focused gameplay restores world simulation and clock");
    focus.Invoke(driver,new object[]{false});before=clock.TotalMinutes;clock.Advance(60);
    Check(clock.TotalMinutes==before && Time.timeScale==0,"Focus loss immediately freezes the clock and world simulation");
    capture.Invoke(movement,new object[]{false});focus.Invoke(driver,new object[]{true});driverTick.Invoke(driver,null);
    Check(clock.Paused,"Returning focus alone does not dismiss settings or resume time");
    Resume();Press(UnityEngine.InputSystem.Key.Escape);driverTick.Invoke(driver,null);
    Check(clock.Paused && Time.timeScale==0,"Escape pauses before the next timed-request input is processed");
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState());UnityEngine.InputSystem.InputSystem.Update();Resume();
    before=clock.TotalMinutes;clock.Advance(-10);clock.Advance(double.NaN);clock.Advance(double.PositiveInfinity);Check(clock.TotalMinutes==before,"Invalid elapsed times cannot reverse or corrupt the clock");
    var rates=new[]{30,60,144};foreach(var rate in rates){var obj=new GameObject("Clock rate fixture");try{var c=obj.AddComponent<PrisonGame.Prototype.PrisonClock>();c.SetPaused(false);for(int i=0;i<300*rate;i++)c.Advance(1.0/rate);Check(System.Math.Abs(c.TotalMinutes-540)<.000001,"Five real minutes advances one game hour at "+rate+" FPS");}finally{UnityEngine.Object.DestroyImmediate(obj);}}
    Check(PrisonGame.Prototype.PrisonClock.Format(1440)=="Day 2 00:00:00" && PrisonGame.Prototype.PrisonClock.Remaining(-1)=="0m 00s","Clock labels handle midnight and never show a negative countdown");
    Check(buyer.Prompt(actor).Contains("1 game hour") && buyer.Prompt(actor).Contains("$2 if late") && hud.RequestStatus.Contains("5 real minutes"),"Offer states duration and late-payment consequence before acceptance");
    Sell();double acceptedAt=clock.TotalMinutes;
    Check(request.Accepted && System.Math.Abs(request.DueAt-acceptedAt-60)<.000001 && stock.Money==0,"Acceptance fixes a one-game-hour deadline without paying or consuming anything");
    double due=request.DueAt;Sell();Check(request.DueAt==due,"Repeated talking cannot extend the accepted deadline");
    Supply();Assemble();clock.Advance((request.DueAt-clock.TotalMinutes-10)*5);hudTick.Invoke(hud,null);
    Check(request.DueSoon && !request.IsLate && request.DeliveryPrice==3,"Final ten game minutes trigger a warning while retaining on-time payment");
    string message=(string)actor.GetType().GetField("message",flags).GetValue(actor);Check(message.Contains("due soon"),"Approaching deadline produces a clear reminder");
    actor.ShowMessage("sentinel");hudTick.Invoke(hud,null);Check((string)actor.GetType().GetField("message",flags).GetValue(actor)=="sentinel","Due-soon reminder is not repeated every frame");
    capture.Invoke(movement,new object[]{false});driverTick.Invoke(driver,null);before=clock.TotalMinutes;clock.Advance(600);Press(UnityEngine.InputSystem.Key.E);
    Check(clock.TotalMinutes==before && request.Delivered==0 && actor.HeldItem!=null,"Pausing near the deadline preserves remaining time and blocks a delivery");
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState());UnityEngine.InputSystem.InputSystem.Update();Resume();
    clock.Advance((request.DueAt-clock.TotalMinutes-.001)*5);Sell();
    Check(request.Delivered==1 && stock.Money==3 && request.Paid==3 && !request.IsLate,"Delivery immediately before the deadline pays the full $3");
    Assemble();clock.Advance((request.DueAt-clock.TotalMinutes)*5);hudTick.Invoke(hud,null);
    Check(System.Math.Abs(clock.TotalMinutes-request.DueAt)<.000001 && request.IsLate && request.DeliveryPrice==2 && request.MinutesLeft==0,"At the deadline boundary the request becomes overdue and price drops to $2");
    Check(stock.Money==3 && buyer.Prompt(actor).Contains("($2)") && hud.NextAction.Contains("$2") && hud.RequestStatus.Contains("Late packs now pay $2"),"Existing earnings stay intact and prompt/HUD agree on the late price");
    message=(string)actor.GetType().GetField("message",flags).GetValue(actor);Check(message.Contains("deadline passed"),"Crossing the deadline produces an explicit consequence notification");
    actor.ShowMessage("sentinel");hudTick.Invoke(hud,null);Check((string)actor.GetType().GetField("message",flags).GetValue(actor)=="sentinel","Overdue notification is not repeated every frame");
    Sell();Check(request.Delivered==2 && stock.Money==5 && request.Paid==5,"Late delivery pays $2 without altering the earlier $3 payment");
    Sell();Check(stock.Money==5 && request.Delivered==2,"Repeated late delivery input cannot duplicate payment or progress");
    Supply();Assemble();Sell();Check(request.Complete && request.CompletedLate && request.Paid==7 && stock.Money==6,"Mixed on-time and late deliveries complete with the actual $7 total minus $1 restock");
    Check(hud.RequestStatus.Contains("$7 paid") && hud.RequestStatus.Contains("Finished late"),"Completion reports actual earnings and late status instead of promising $9");
    double finishDue=request.DueAt;clock.Advance(1000);Sell();Check(request.Paid==7 && request.DueAt==finishDue && stock.Money==6,"Time and repeated talk after completion cannot alter earnings or restart the request");
    Supply();Assemble();Sell();Check(stock.Money==8 && stock.Sales==4 && request.Paid==7,"Regular $3 sales resume after completion and can still fund the shelf");
    Shelf();Check(shelf.Installed && stock.Money==0,"Late completion still permits earning and purchasing the existing shelf");
    // A second personal request exercises the on-time completion path against the same shared clock.
    var original=request;UnityEngine.Object.DestroyImmediate(request);request=player.AddComponent<PrisonGame.Prototype.SnackRequest>();var so=new SerializedObject(request);so.FindProperty("customer").objectReferenceValue=buyer;so.FindProperty("clock").objectReferenceValue=clock;so.ApplyModifiedPropertiesWithoutUndo();
    request.Accept(actor,buyer);for(int i=0;i<3;i++){Supply();Assemble();Sell();}
    Check(request.Complete && !request.CompletedLate && request.Paid==9,"All-on-time completion pays exactly $9");
    clock.Advance(301);Check(!request.IsLate && !request.CompletedLate && request.Paid==9,"Passing the deadline after on-time completion does not retroactively penalize the request");
    results.Add("INFO: Clock boundary advances are controlled; interaction uses synthetic E through actual targets. Pause/focus drivers exercised directly. On-time fixture replaces personal request only at test end. Stop Play resets all test state.");
}finally{
    if(keyboard!=null)UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);settings.backgroundBehavior=bg;settings.editorInputBehaviorInPlayMode=input;capture.Invoke(movement,new object[]{false});driver.enabled=driverEnabled;cc.enabled=false;player.transform.SetPositionAndRotation(start,rotation);cc.enabled=true;cam.transform.localRotation=cameraRotation;Physics.SyncTransforms();
}
System.IO.File.WriteAllLines("../docs/images/earn03-deadline-checks.txt",results);return string.Join("\n",results);
