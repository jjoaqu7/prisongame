if(!EditorApplication.isPlaying)throw new System.Exception("Enter Play mode first.");
var skin=UnityEngine.Object.FindFirstObjectByType<SkinnedMeshRenderer>();
var animator=skin.GetComponentInParent<Animator>();var root=animator.gameObject;
var mode=animator.cullingMode;int previous=animator.GetInteger("Preview");
var collider=animator.GetComponentInParent<PrisonGame.Prototype.PrototypeInmate>().GetComponent<CapsuleCollider>();
var position=collider.transform.position;var results=new System.Collections.Generic.List<string>();
void Check(bool value,string message){if(!value)throw new System.Exception("FAIL: "+message);results.Add("PASS: "+message);}
var first=new Mesh();var second=new Mesh();
try{
    animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
    animator.SetInteger("Preview",0);animator.Play("Idle",0,0);animator.Update(0);skin.BakeMesh(first);
    animator.Update(1);skin.BakeMesh(second);
    Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"),"Animator runs Idle state in Play mode");
    float movement=0;var a=first.vertices;var b=second.vertices;for(int i=0;i<a.Length;i++)movement=Mathf.Max(movement,Vector3.Distance(a[i],b[i]));
    Check(movement>.0001f && movement<.05f,"Idle produces small actual skinned movement");
    foreach(var pair in new[]{new System.Collections.Generic.KeyValuePair<int,string>(1,"WalkPreview"),new System.Collections.Generic.KeyValuePair<int,string>(2,"GesturePreview"),new System.Collections.Generic.KeyValuePair<int,string>(0,"Idle")}){
        animator.SetInteger("Preview",pair.Key);animator.Update(.02f);animator.Update(.4f);
        Check(animator.GetCurrentAnimatorStateInfo(0).IsName(pair.Value),"Preview parameter switches to "+pair.Value);
        skin.BakeMesh(first);
        foreach(var p in first.vertices)if(!skin.bounds.Contains(skin.transform.TransformPoint(p)))throw new System.Exception("Animated geometry outside culling bounds");
    }
    Check(Vector3.Distance(collider.transform.position,position)<.00001f,"Preview clips do not move gameplay position or collider");
    Check(!animator.applyRootMotion,"Root motion is disabled for the study");
    Check(root.GetComponentsInChildren<Renderer>().Length==1,"Only the skinned visual is active inside the rig");
    results.Add("INFO: maximum idle vertex displacement over one second="+movement.ToString("F5")+" m");
}finally{
    animator.SetInteger("Preview",previous);animator.Play(previous==1?"WalkPreview":previous==2?"GesturePreview":"Idle",0,0);animator.Update(0);animator.cullingMode=mode;
    UnityEngine.Object.DestroyImmediate(first);UnityEngine.Object.DestroyImmediate(second);
}
System.IO.File.WriteAllLines("../docs/images/art03/m5-rig-runtime-checks.txt",results);
return string.Join("\n",results);
