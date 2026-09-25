// Runtime captures after actual motion and talk commands; restore all capture changes.
if(!EditorApplication.isPlaying)throw new System.Exception("Enter Play mode first.");
var motion=UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.InmateMotion>();var presentation=motion.GetComponentInChildren<PrisonGame.Prototype.InmateAnimation>();var animator=presentation.GetComponent<Animator>();var clearance=presentation.GetComponent<PrisonGame.Prototype.RigFloorClearance>();
var cam=Camera.main;var player=cam.GetComponentInParent<PrisonGame.Prototype.PlayerInteraction>();var cc=player.GetComponent<CharacterController>();
var root=motion.transform;var start=root.position;var rotation=root.rotation;var camPosition=cam.transform.position;var camRotation=cam.transform.rotation;float fov=cam.fieldOfView;var playerPosition=player.transform.position;var mode=animator.cullingMode;
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;var step=motion.GetType().GetMethod("Step",flags);var talk=motion.GetType().GetField("talkRemaining",flags);
void Tick(){step.Invoke(motion,new object[]{1f/60});presentation.RefreshAnimation();animator.Update(1f/60);clearance.ApplyFloorClearance();Physics.SyncTransforms();}
void Shot(string name){
    // Force a fresh deformation snapshot after manual Animator evaluation; camera-only
    // rendering can otherwise reuse the previous frame's skinning buffers.
    var skin=presentation.GetComponentInChildren<SkinnedMeshRenderer>();var snapshot=new Mesh();skin.BakeMesh(snapshot);
    var display=new GameObject("Temporary deformed capture");display.transform.SetParent(skin.transform,false);
    display.AddComponent<MeshFilter>().sharedMesh=snapshot;display.AddComponent<MeshRenderer>().sharedMaterials=skin.sharedMaterials;skin.enabled=false;
    cam.transform.position=new Vector3(3.15f,1.55f,2.9f);cam.transform.LookAt(root.position+Vector3.up*1.03f);cam.fieldOfView=65;
    var rt=new RenderTexture(1440,1080,24);var tex=new Texture2D(1440,1080,TextureFormat.RGB24,false);var old=RenderTexture.active;
    try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1440,1080),0,0);tex.Apply();System.IO.File.WriteAllBytes("../docs/images/art03/m5-behavior-"+name+".png",tex.EncodeToPNG());}
    finally{cam.targetTexture=null;RenderTexture.active=old;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);skin.enabled=true;UnityEngine.Object.DestroyImmediate(display);UnityEngine.Object.DestroyImmediate(snapshot);}
}
try{
    animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;talk.SetValue(motion,0f);motion.Stop();root.rotation=Quaternion.identity;
    animator.Rebind();animator.SetInteger("Preview",0);animator.Update(0);cc.enabled=false;player.transform.position=new Vector3(-3.4f,.05f,-.5f);cc.enabled=true;Physics.SyncTransforms();
    motion.SetDestination(start+Vector3.back*1.2f);for(int i=0;i<55;i++)Tick();Shot("walking");
    cc.enabled=false;player.transform.position=new Vector3(2.4f,.05f,3.1f);cc.enabled=true;Physics.SyncTransforms();
    motion.GetComponent<PrisonGame.Prototype.PrototypeInmate>().Interact(player);for(int i=0;i<65;i++)Tick();Shot("talking");
}finally{
    motion.Stop();talk.SetValue(motion,0f);root.SetPositionAndRotation(start,rotation);cc.enabled=false;player.transform.position=playerPosition;cc.enabled=true;
    cam.transform.SetPositionAndRotation(camPosition,camRotation);cam.fieldOfView=fov;presentation.RefreshAnimation();animator.Play("Idle",0,0);animator.Update(0);clearance.ApplyFloorClearance();animator.cullingMode=mode;Physics.SyncTransforms();
}
return "Captured actual walking and talk-triggered gesture in the common area; restored runtime state. Camera renders exclude overlay UI.";
