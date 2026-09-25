// Full loop through real E/Q input, ray targeting and physical carry state. Run fresh Play.
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
try{
    settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
    Check(stock.Money==0&&hud.NextAction.Contains("starter"),"HUD begins with zero money and starter-supply objective");
    Shelf();Check(!shelf.Installed&&stock.Money==0,"Insufficient funds do not install or charge for the shelf");
    Supply();Supply();Check(stock.Crackers==2&&stock.Fruit==2&&stock.Wrappers==2&&stock.Money==0,"Starter supplies claimed once; unaffordable restock changes nothing");
    Check(hud.NextAction.Contains("assemble"),"Objective advances to assembly after collecting supplies");
    Assemble();var first=actor.HeldItem;var firstPack=first.GetComponent<PrisonGame.Prototype.SnackPackItem>();Check(firstPack.Source==stock&&hud.NextAction.Contains("M5"),"Finished physical pack is tracked and objective directs the player to M5");
    Sell();Check(stock.Money==3&&stock.Sales==1&&actor.HeldItem==null&&firstPack.Sold&&!first.gameObject.activeSelf,"E sells one physical pack once, clears hands and credits $3");
    Sell();Check(stock.Money==3&&stock.Sales==1,"Repeated E with empty hands cannot duplicate a sale");
    Check(inmate.GetComponent<PrisonGame.Prototype.InmateMotion>().IsTalking,"Sale triggers M5's existing dialogue/gesture state");
    Assemble();Sell();Check(stock.Money==6&&stock.Sales==2&&stock.Crackers==0,"Two starter packs produce $6 without negative ingredients");
    Check(hud.NextAction.Contains("Restock"),"Objective directs depleted stock to paid restocking");
    Supply();Check(stock.Money==5&&stock.Crackers==1&&stock.Fruit==1&&stock.Wrappers==1,"Restocking costs $1 for exactly one complete recipe");
    Assemble();Sell();Check(stock.Money==8&&stock.Sales==3&&hud.NextAction.Contains("shelf"),"Third sale reaches the $8 shelf objective");
    Shelf();Check(shelf.Installed&&stock.Money==0&&hud.NextAction.Contains("Shelf earned"),"Buying the shelf deducts $8 and completes the visible objective");
    var visual=(GameObject)new SerializedObject(shelf).FindProperty("shelfVisual").objectReferenceValue;Check(visual.activeSelf&&visual.GetComponentsInChildren<BoxCollider>().Length==4,"Purchased shelf has visible boards and physical storage support");
    Shelf();Check(stock.Money==0,"Using an installed shelf cannot charge a second purchase");
    Supply();Check(stock.Crackers==1&&stock.Money==0,"No-stock/no-money recovery supplies let the loop continue after buying the shelf");
    Assemble();var stored=actor.HeldItem;Shelf();Check(actor.HeldItem==null&&stored.GetComponent<BoxCollider>().enabled,"E stores a carried pack on the new shelf");
    Check(stored.transform.position.y>1.12f&&stored.transform.position.z<-2.4f,"Stored pack is physically on the cell shelf");
    Check(!stock.CanRecover,"A stored unsold pack prevents farming free recovery supplies");
    Supply();Check(stock.Crackers==0&&stock.Money==0,"Recovery is refused while an unsold pack remains in the world");
    Use(stored,new Vector3(-3.4f,.05f,-1.3f),stored.transform.position);Check(actor.HeldItem==stored,"Stored pack can be picked up through the normal E target");
    Sell();Check(stock.Money==3&&stock.Sales==4,"A retrieved stored pack sells normally");
    Supply();Supply();Supply();Check(stock.Money==0&&stock.Crackers==3,"Three paid refills are charged once each");
    Assemble();Shelf();Assemble();Shelf();Assemble();var held=actor.HeldItem;Shelf();Check(actor.HeldItem==held,"Full shelf keeps the carried item instead of losing or overlapping it");
    int money=stock.Money,sales=stock.Sales;capture.Invoke(movement,new object[]{false});Press(UnityEngine.InputSystem.Key.E);Check(stock.Money==money&&stock.Sales==sales&&actor.HeldItem==held,"Settings-open E does not trigger a transaction");
    Check(!stock.Spend(-1)&&!stock.Spend(1)&&stock.Money==0,"Invalid or unaffordable spending cannot create money or go negative");
    Sell();var parcel=UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.PrototypePickup>();
    foreach(var candidate in UnityEngine.Object.FindObjectsByType<PrisonGame.Prototype.PrototypePickup>(FindObjectsSortMode.None))if(candidate.GetComponent<PrisonGame.Prototype.SnackPackItem>()==null){parcel=candidate;break;}
    parcel.Interact(actor);money=stock.Money;sales=stock.Sales;Sell();Check(actor.HeldItem==parcel&&stock.Money==money&&stock.Sales==sales,"M5 does not consume or pay for a non-snack parcel");
    results.Add("INFO: Complete supply/assemble/carry/sell/restock/upgrade/storage loop exercised with synthetic E input through real target checks. State intentionally remains for visual inspection; stop Play to reset.");
}finally{if(keyboard!=null)UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);settings.backgroundBehavior=bg;settings.editorInputBehaviorInPlayMode=input;capture.Invoke(movement,new object[]{false});cc.enabled=false;player.transform.SetPositionAndRotation(start,rotation);cc.enabled=true;cam.transform.localRotation=cameraRotation;Physics.SyncTransforms();}
System.IO.File.WriteAllLines("../docs/images/earn01-loop-checks.txt",results);return string.Join("\n",results);
