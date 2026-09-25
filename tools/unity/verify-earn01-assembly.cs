if(!EditorApplication.isPlaying)throw new System.Exception("Enter Play mode first.");
var player=GameObject.Find("Player");var interaction=player.GetComponent<PrisonGame.Prototype.PlayerInteraction>();var supplies=player.GetComponent<PrisonGame.Prototype.SnackSupplies>();
var assembly=UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.SnackAssembly>();var box=UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.SnackSupplyBox>();
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;var seek=interaction.GetType().GetMethod("FindTarget",flags);var cam=player.GetComponentInChildren<Camera>();var cc=player.GetComponent<CharacterController>();
var oldPosition=player.transform.position;var oldRotation=cam.transform.rotation;var results=new System.Collections.Generic.List<string>();
void Check(bool value,string message){if(!value)throw new System.Exception("FAIL: "+message);results.Add("PASS: "+message);}
try{
    if(supplies.StarterCollected||interaction.HeldItem!=null)throw new System.Exception("Run from fresh Play state.");
    assembly.Interact(interaction);Check(assembly.Stage==0,"Assembly refuses missing ingredients");
    cc.enabled=false;player.transform.position=new Vector3(2.7f,.05f,3.8f);cc.enabled=true;
    cam.transform.rotation=Quaternion.LookRotation(box.transform.position-cam.transform.position);Physics.SyncTransforms();seek.Invoke(interaction,null);
    Check(interaction.Target==box,"Supply box reachable from the open table end");
    box.Interact(interaction);Check(supplies.Crackers==2&&supplies.Fruit==2&&supplies.Wrappers==2,"Starter supply contains two complete recipes");
    box.Interact(interaction);Check(supplies.Crackers==2&&supplies.Fruit==2&&supplies.Wrappers==2,"Starter collection cannot be repeated");
    cam.transform.rotation=Quaternion.LookRotation(assembly.transform.position-cam.transform.position);Physics.SyncTransforms();seek.Invoke(interaction,null);Check(interaction.Target==assembly,"Assembly tray reachable from the open table end");
    assembly.Interact(interaction);Check(assembly.Stage==1&&supplies.Crackers==1&&supplies.Fruit==2,"First action places and consumes crackers");
    var second=new GameObject("Temporary second actor");
    try{second.AddComponent<CharacterController>();var view=new GameObject("Camera").AddComponent<Camera>();view.transform.SetParent(second.transform);second.AddComponent<PrisonGame.Prototype.FirstPersonController>();var other=second.AddComponent<PrisonGame.Prototype.PlayerInteraction>();second.AddComponent<PrisonGame.Prototype.SnackSupplies>().CollectStarter();assembly.Interact(other);Check(assembly.Stage==1&&assembly.WorkingPlayer==interaction,"Another actor cannot take over an occupied tray");}
    finally{UnityEngine.Object.DestroyImmediate(second);}
    assembly.Interact(interaction);Check(assembly.Stage==2&&supplies.Fruit==1,"Second action places and consumes dried fruit");
    assembly.Interact(interaction);var pack=interaction.HeldItem;Check(pack!=null&&pack.GetComponent<PrisonGame.Prototype.SnackPackItem>()!=null&&assembly.Stage==0,"Wrapping produces a physical held snack pack and clears the tray");
    Check(supplies.Crackers==1&&supplies.Fruit==1&&supplies.Wrappers==1,"One finished pack consumes exactly one recipe");
    assembly.Interact(interaction);Check(assembly.Stage==0&&supplies.Crackers==1,"Full hands block starting another pack without consuming supplies");
    cam.transform.rotation=Quaternion.LookRotation(new Vector3(3.9f,.84f,4.25f)-cam.transform.position);Physics.SyncTransforms();Check(interaction.TryPutDown(),"Finished pack can be placed on a clear table surface");
    Check(interaction.HeldItem==null&&pack.GetComponent<BoxCollider>().enabled,"Placed pack restores physical collision");
    pack.Interact(interaction);Check(interaction.HeldItem==pack,"Placed pack can be picked up again");
}finally{cc.enabled=false;player.transform.position=oldPosition;cc.enabled=true;cam.transform.rotation=oldRotation;Physics.SyncTransforms();}
System.IO.File.WriteAllLines("../docs/images/earn01-assembly-checks.txt",results);return string.Join("\n",results);
