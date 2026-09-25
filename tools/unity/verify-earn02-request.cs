// Tracked-request integration checks through real E input. Run fresh Earn02 Play.
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
GameObject second=null, otherCustomer=null;
try{
    settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
    Check(!request.Accepted && request.Delivered==0 && hud.NextAction.Contains("Meet M5"),"Fresh scene offers a request without automatically accepting it");
    Check(buyer.Prompt(actor).Contains("Accept") && buyer.Prompt(actor).Contains("$3") && buyer.Prompt(actor).Contains("no deadline"),"Acceptance prompt states quantity, payment and no deadline");
    Check(hud.RequestStatus.Contains("3 snack packs") && hud.RequestStatus.Contains("$9 total"),"Persistent offer states the full request and total payment");
    Supply();Assemble();var first=actor.HeldItem;
    Check(request.ReadyPacks==1 && stock.Sales==0,"Assembling a pack increases ready stock without delivering it");
    Check(!request.Deliver(actor,buyer) && actor.HeldItem==first && stock.Money==0,"Cannot deliver or receive request payment before acceptance");
    Use(inmate,new Vector3(2.4f,.05f,3.5f),inmate.transform.position+Vector3.up*1.2f);
    Check(request.Accepted && request.Delivered==0 && stock.Money==0 && actor.HeldItem==first,"E explicitly accepts the request without consuming the carried pack or paying");
    Check(!request.Accept(actor,buyer),"Repeated acceptance cannot reset request progress");
    capture.Invoke(movement,new object[]{false});Press(UnityEngine.InputSystem.Key.E);
    Check(request.Delivered==0 && actor.HeldItem==first && stock.Money==0,"Settings-open E cannot deliver a held pack");
    second=new GameObject("Temporary other actor");second.transform.position=new Vector3(50,0,0);var otherCam=new GameObject("Camera").AddComponent<Camera>();otherCam.enabled=false;otherCam.transform.SetParent(second.transform);var other=second.AddComponent<PrisonGame.Prototype.PlayerInteraction>();second.GetComponent<PrisonGame.Prototype.FirstPersonController>().enabled=false;var otherStock=second.AddComponent<PrisonGame.Prototype.SnackSupplies>();var otherRequest=second.AddComponent<PrisonGame.Prototype.SnackRequest>();
    Check(!request.Deliver(other,buyer) && !request.Accept(other,buyer) && !otherRequest.Accepted && otherStock.Money==0,"Another actor cannot accept or advance this player's request");
    otherCustomer=new GameObject("Temporary other buyer");var wrongBuyer=otherCustomer.AddComponent<PrisonGame.Prototype.SnackPackBuyer>();
    Check(!request.Deliver(actor,wrongBuyer) && request.Delivered==0 && actor.HeldItem==first,"Delivery to a different customer cannot advance or consume this request");
    Sell();Check(request.Delivered==1 && stock.Money==3 && stock.Sales==1 && request.ReadyPacks==0 && actor.HeldItem==null,"First real E delivery consumes one pack, credits $3 and updates request stock/progress");
    Check(hud.RequestStatus.Contains("1 / 3") && hud.RequestStatus.Contains("Still to deliver: 2") && inmate.GetComponent<PrisonGame.Prototype.InmateMotion>().IsTalking,"HUD tracks remaining quantity and delivery retains M5's talking behavior");
    Sell();Check(request.Delivered==1 && stock.Money==3,"Repeated empty-handed E cannot duplicate progress or payment");
    Assemble();var placed=actor.HeldItem;
    cc.enabled=false;player.transform.position=new Vector3(2.7f,.05f,3.8f);cc.enabled=true;cam.transform.rotation=Quaternion.LookRotation(new Vector3(3.9f,.84f,4.25f)-cam.transform.position);Physics.SyncTransforms();
    Check(actor.TryPutDown() && request.ReadyPacks==1 && request.Delivered==1,"Placed physical packs remain ready stock without counting as delivered");
    Check(hud.NextAction.Contains("Pick up") && hud.RequestStatus.Contains("Ready packs: 1"),"Depleted ingredients with a stored pack direct the player to retrieve it");
    Use(placed,new Vector3(2.7f,.05f,3.8f),placed.transform.position);Sell();
    Check(request.Delivered==2 && stock.Money==6 && hud.NextAction.Contains("Restock"),"Retrieved pack advances to 2/3 and next action directs paid restocking");
    var parcel=UnityEngine.Object.FindObjectsByType<PrisonGame.Prototype.PrototypePickup>(FindObjectsSortMode.None).First(x=>x.GetComponent<PrisonGame.Prototype.SnackPackItem>()==null);
    parcel.Interact(actor);Sell();Check(actor.HeldItem==parcel && request.Delivered==2 && stock.Money==6,"A non-snack item is retained and earns no request credit");
    actor.ConsumeHeldItem(parcel);Supply();Assemble();Sell();
    Check(request.Complete && request.Delivered==3 && request.Remaining==0 && stock.Money==8 && stock.Sales==3,"Third delivery completes the request with exactly $9 earned minus $1 restock");
    Check(hud.RequestStatus.Contains("3 / 3") && hud.RequestStatus.Contains("$9 paid") && hud.NextAction.Contains("buy the shelf"),"Completion remains visible and guidance returns to the affordable shelf");
    Sell();Check(request.Complete && stock.Money==8 && !request.Accept(actor,buyer),"Completed request cannot restart or pay again on empty-handed input");
    Shelf();Check(shelf.Installed && stock.Money==0 && hud.NextAction.Contains("Shelf earned"),"Request earnings purchase the existing shelf without blocking completion guidance");
    Supply();Assemble();Shelf();Check(request.ReadyPacks==1 && actor.HeldItem==null,"A pack stored on the purchased shelf stays in the ready-stock count");
    var stored=UnityEngine.Object.FindObjectsByType<PrisonGame.Prototype.SnackPackItem>(FindObjectsSortMode.None).First(x=>!x.Sold).GetComponent<PrisonGame.Prototype.PrototypePickup>();
    Use(stored,new Vector3(-3.4f,.05f,-1.3f),stored.transform.position);Sell();
    Check(stock.Money==3 && stock.Sales==4 && request.Delivered==3 && request.Complete,"Ordinary sales continue after completion without overcounting the request");
    results.Add("INFO: Synthetic E input, real targeting/assembly/placement/economy. Temporary second actor verifies local state isolation only, not networking. Stop Play to reset.");
}finally{
    if(second!=null)UnityEngine.Object.DestroyImmediate(second);if(otherCustomer!=null)UnityEngine.Object.DestroyImmediate(otherCustomer);
    if(keyboard!=null)UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);settings.backgroundBehavior=bg;settings.editorInputBehaviorInPlayMode=input;capture.Invoke(movement,new object[]{false});cc.enabled=false;player.transform.SetPositionAndRotation(start,rotation);cc.enabled=true;cam.transform.localRotation=cameraRotation;Physics.SyncTransforms();
}
System.IO.File.WriteAllLines("../docs/images/earn02-request-checks.txt",results);return string.Join("\n",results);
