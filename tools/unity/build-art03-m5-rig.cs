// Create a generic, weighted deformation study from the saved refined M5.
// No humanoid retargeting, navigation, root motion or gameplay animation rules.
if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)throw new System.Exception("Editor must be idle.");
for(int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)throw new System.Exception("Save pending edits first.");
const string folder="Assets/Prototype/Art03Rig";
const string destination="Assets/Scenes/Art03_M5_Rig.unity";
if(AssetDatabase.IsValidFolder(folder)||System.IO.File.Exists(destination))throw new System.Exception("Rig study already exists; edit saved assets.");
AssetDatabase.CreateFolder("Assets/Prototype","Art03Rig");AssetDatabase.CreateFolder(folder,"Clips");
var model=new GameObject("M5_Rigged");
var boneList=new System.Collections.Generic.List<Transform>();
var indices=new System.Collections.Generic.Dictionary<string,int>();
Transform Bone(string name,string parent,Vector3 position){
    var t=new GameObject(name).transform;t.SetParent(parent==null?model.transform:boneList[indices[parent]],false);
    t.position=position;indices.Add(name,boneList.Count);boneList.Add(t);return t;
}
Bone("Hips",null,new Vector3(0,.91f,0));
Bone("Spine","Hips",new Vector3(0,1.115f,0));
Bone("Chest","Spine",new Vector3(0,1.32f,0));
Bone("Neck","Chest",new Vector3(0,1.412f,0));
Bone("Head","Neck",new Vector3(0,1.49f,0));
foreach(int sign in new[]{-1,1}){
    string side=sign<0?"Left":"Right";
    Bone(side+"UpperArm","Chest",new Vector3(sign*.128f,1.327f,0));
    Bone(side+"Forearm",side+"UpperArm",new Vector3(sign*.159f,1.039f,-.003f));
    Bone(side+"Hand",side+"Forearm",new Vector3(sign*.176f,.821f,-.007f));
    Bone(side+"Thigh","Hips",new Vector3(sign*.063f,.895f,0));
    Bone(side+"Shin",side+"Thigh",new Vector3(sign*.063f,.51f,0));
    Bone(side+"Foot",side+"Shin",new Vector3(sign*.068f,.092f,-.012f));
}
float S(float a,float b,float v){float t=Mathf.InverseLerp(a,b,v);return t*t*(3-2*t);}
BoneWeight Weights(string part,Vector3 p){
    var w=new float[boneList.Count];void Add(string name,float value){w[indices[name]]+=Mathf.Max(0,value);}
    string side=part.StartsWith("Left")?"Left":part.StartsWith("Right")?"Right":p.x<0?"Left":"Right";
    if(part=="JoinedShirt"){
        float arm=S(.075f,.156f,Mathf.Abs(p.x))*S(1.145f,1.25f,p.y)*(1-S(1.332f,1.395f,p.y));
        float spine=S(.99f,1.14f,p.y),chest=S(1.17f,1.31f,p.y);
        Add("Hips",(1-arm)*(1-spine));Add("Spine",(1-arm)*spine*(1-chest));Add("Chest",(1-arm)*spine*chest);Add(side+"UpperArm",arm);
    }else if(part=="JoinedTrousers"){
        float thigh=(1-S(.82f,.93f,p.y))*Mathf.Lerp(1,S(.015f,.05f,Mathf.Abs(p.x)),S(.80f,.86f,p.y));
        float shin=1-S(.465f,.565f,p.y);float foot=1-S(.105f,.175f,p.y);
        Add("Hips",1-thigh);Add(side+"Thigh",thigh*(1-shin));Add(side+"Shin",thigh*shin*(1-foot));Add(side+"Foot",thigh*shin*foot);
    }else if(part=="JoinedHeadNeck"){
        float neck=S(1.378f,1.425f,p.y),head=S(1.435f,1.505f,p.y);
        Add("Chest",1-neck);Add("Neck",neck*(1-head));Add("Head",neck*head);
    }else if(part.Contains("JoinedArmHand")){
        float forearm=1-S(.989f,1.092f,p.y),hand=1-S(.788f,.861f,p.y);
        Add(side+"UpperArm",1-forearm);Add(side+"Forearm",forearm*(1-hand));Add(side+"Hand",forearm*hand);
    }else if(part.Contains("Shoe")||part.Contains("Sole"))Add(side+"Foot",1);
    else Add("Head",1);
    var ids=new System.Collections.Generic.List<int>();for(int i=0;i<w.Length;i++)if(w[i]>0)ids.Add(i);
    ids.Sort((a,b)=>w[b].CompareTo(w[a]));float sum=0;for(int i=0;i<Mathf.Min(4,ids.Count);i++)sum+=w[ids[i]];
    if(sum<=0)throw new System.Exception("Unweighted vertex: "+part);
    var bw=new BoneWeight();bw.boneIndex0=ids[0];bw.weight0=w[ids[0]]/sum;
    if(ids.Count>1){bw.boneIndex1=ids[1];bw.weight1=w[ids[1]]/sum;}
    if(ids.Count>2){bw.boneIndex2=ids[2];bw.weight2=w[ids[2]]/sum;}
    if(ids.Count>3){bw.boneIndex3=ids[3];bw.weight3=w[ids[3]]/sum;}
    return bw;
}
var source=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prototype/Art03Refined/M5_Refined.prefab"));
var vertices=new System.Collections.Generic.List<Vector3>();var normals=new System.Collections.Generic.List<Vector3>();var weights=new System.Collections.Generic.List<BoneWeight>();
var materials=new System.Collections.Generic.List<Material>();var submeshes=new System.Collections.Generic.List<System.Collections.Generic.List<int>>();
var parts=new System.Collections.Generic.List<string>();
try{
    foreach(var mf in source.GetComponentsInChildren<MeshFilter>()){
        var mesh=mf.sharedMesh;var matrix=source.transform.worldToLocalMatrix*mf.transform.localToWorldMatrix;
        var material=mf.GetComponent<Renderer>().sharedMaterial;int slot=materials.IndexOf(material);
        if(slot<0){slot=materials.Count;materials.Add(material);submeshes.Add(new System.Collections.Generic.List<int>());}
        int offset=vertices.Count;var v=mesh.vertices;var n=mesh.normals;
        for(int i=0;i<v.Length;i++){var p=matrix.MultiplyPoint3x4(v[i]);vertices.Add(p);normals.Add(matrix.MultiplyVector(n[i]).normalized);weights.Add(Weights(mf.name,p));}
        foreach(int index in mesh.triangles)submeshes[slot].Add(index+offset);
        parts.Add(mf.name+"|"+offset+"|"+v.Length);
    }
}finally{UnityEngine.Object.DestroyImmediate(source);}
var skinMesh=new Mesh();skinMesh.name="M5_Weighted";
if(vertices.Count>65535)skinMesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
skinMesh.SetVertices(vertices);skinMesh.SetNormals(normals);skinMesh.subMeshCount=materials.Count;
for(int i=0;i<submeshes.Count;i++)skinMesh.SetTriangles(submeshes[i],i);
skinMesh.boneWeights=weights.ToArray();
var bindposes=new Matrix4x4[boneList.Count];
for(int i=0;i<boneList.Count;i++)bindposes[i]=boneList[i].worldToLocalMatrix*model.transform.localToWorldMatrix;
skinMesh.bindposes=bindposes;skinMesh.RecalculateBounds();AssetDatabase.CreateAsset(skinMesh,folder+"/M5_Weighted.asset");
var visual=new GameObject("SkinnedVisual");visual.transform.SetParent(model.transform,false);
var renderer=visual.AddComponent<SkinnedMeshRenderer>();renderer.sharedMesh=skinMesh;renderer.sharedMaterials=materials.ToArray();renderer.bones=boneList.ToArray();renderer.rootBone=model.transform;renderer.quality=SkinQuality.Bone4;
// Bounds include intended arm gestures and stride tests; verify deformed samples separately.
renderer.localBounds=new Bounds(new Vector3(0,1.02f,0),new Vector3(1.5f,2.22f,1.4f));renderer.updateWhenOffscreen=false;
string PathTo(string name)=>AnimationUtility.CalculateTransformPath(boneList[indices[name]],model.transform);
AnimationClip Clip(string name,float duration,System.Func<string,float,Vector3> rotation){
    var clip=new AnimationClip();clip.name=name;clip.frameRate=30;
    foreach(var pair in indices){
        var x=new AnimationCurve();var y=new AnimationCurve();var z=new AnimationCurve();var w=new AnimationCurve();
        int steps=Mathf.RoundToInt(duration*30);
        for(int i=0;i<=steps;i++){float t=duration*i/steps;var q=Quaternion.Euler(rotation(pair.Key,t/duration));x.AddKey(t,q.x);y.AddKey(t,q.y);z.AddKey(t,q.z);w.AddKey(t,q.w);}
        foreach(var entry in new[]{new System.Collections.Generic.KeyValuePair<string,AnimationCurve>("x",x),new System.Collections.Generic.KeyValuePair<string,AnimationCurve>("y",y),new System.Collections.Generic.KeyValuePair<string,AnimationCurve>("z",z),new System.Collections.Generic.KeyValuePair<string,AnimationCurve>("w",w)})
            AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve(PathTo(pair.Key),typeof(Transform),"m_LocalRotation."+entry.Key),entry.Value);
    }
    var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=true;settings.loopBlend=false;AnimationUtility.SetAnimationClipSettings(clip,settings);
    clip.EnsureQuaternionContinuity();AssetDatabase.CreateAsset(clip,folder+"/Clips/"+name+".anim");return clip;
}
var idle=Clip("M5_Idle",4f,(name,t)=>{
    float a=Mathf.Sin(t*Mathf.PI*2);if(name=="Spine")return new Vector3(.8f*a,0,.35f*a);
    if(name=="Chest")return new Vector3(-.4f*a,0,0);
    if(name=="Head")return new Vector3(0,1.2f*a,0);
    return Vector3.zero;
});
var walk=Clip("M5_WalkPreview",1.2f,(name,t)=>{
    float a=Mathf.Sin(t*Mathf.PI*2);float phase=name.StartsWith("Left")?a:-a;
    if(name.EndsWith("Thigh"))return new Vector3(22*phase,0,0);
    if(name.EndsWith("Shin"))return new Vector3(-38*Mathf.Max(0,phase),0,0);
    if(name.EndsWith("Foot"))return new Vector3(12*Mathf.Max(0,phase),0,0);
    if(name.EndsWith("UpperArm"))return new Vector3(-12*phase,0,0);
    if(name.EndsWith("Forearm"))return new Vector3(6+6*Mathf.Max(0,-phase),0,0);
    return Vector3.zero;
});
var gesture=Clip("M5_GesturePreview",2.4f,(name,t)=>{
    float a=.5f-.5f*Mathf.Cos(t*Mathf.PI*2);
    if(name=="LeftUpperArm")return new Vector3(12*a,0,-18*a);
    if(name=="LeftForearm")return new Vector3(65*a,0,0);
    if(name=="Head")return new Vector3(5*Mathf.Sin(t*Mathf.PI*4),-12*a,0);
    return Vector3.zero;
});
var controller=UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(folder+"/M5_Study.controller");
var stateMachine=controller.layers[0].stateMachine;var idleState=stateMachine.AddState("Idle");idleState.motion=idle;stateMachine.defaultState=idleState;
var walkState=stateMachine.AddState("WalkPreview");walkState.motion=walk;
var gestureState=stateMachine.AddState("GesturePreview");gestureState.motion=gesture;
controller.AddParameter("Preview",AnimatorControllerParameterType.Int);
foreach(var entry in new[]{new System.Collections.Generic.KeyValuePair<int,UnityEditor.Animations.AnimatorState>(0,idleState),new System.Collections.Generic.KeyValuePair<int,UnityEditor.Animations.AnimatorState>(1,walkState),new System.Collections.Generic.KeyValuePair<int,UnityEditor.Animations.AnimatorState>(2,gestureState)}){
    var transition=stateMachine.AddAnyStateTransition(entry.Value);transition.hasExitTime=false;transition.duration=.15f;transition.canTransitionToSelf=false;
    transition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals,entry.Key,"Preview");
}
var animator=model.AddComponent<Animator>();animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.CullUpdateTransforms;
PrefabUtility.SaveAsPrefabAsset(model,folder+"/M5_Rigged.prefab");UnityEngine.Object.DestroyImmediate(model);
System.IO.Directory.CreateDirectory("../docs/images/art03");
System.IO.File.WriteAllLines("../docs/images/art03/m5-rig-parts.txt",parts);
var scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Art03_M5_Refined.unity");
if(!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene,destination))throw new System.Exception("Could not create rig scene.");
var inmate=GameObject.Find("Inmate - M5 static study");
foreach(Transform child in inmate.transform)child.gameObject.SetActive(false);
PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(folder+"/M5_Rigged.prefab"),inmate.transform);
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
return "Created 17-bone generic rig, normalized weights, one skinned renderer/eight material slots, Idle and two isolated preview clips. "+vertices.Count+" vertices; geometry retained. Saved Art03_M5_Rig; no NPC navigation or talking hookup added.";
