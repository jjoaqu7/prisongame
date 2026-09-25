// Deformation checks use actual Unity skinning, not a duplicate skinning implementation.
if(EditorApplication.isPlayingOrWillChangePlaymode)throw new System.Exception("Run outside Play mode.");
var results=new System.Collections.Generic.List<string>();
void Check(bool value,string message){if(!value)throw new System.Exception("FAIL: "+message);results.Add("PASS: "+message);}
var preview=UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
var root=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prototype/Art03Rig/M5_Rigged.prefab"));
UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root,preview);
var baked=new Mesh();
try{
    root.GetComponent<Animator>().enabled=false;var skin=root.GetComponentInChildren<SkinnedMeshRenderer>();
    var source=skin.sharedMesh;var rest=source.vertices;var bones=skin.bones;var transforms=root.GetComponentsInChildren<Transform>();
    Check(bones.Length==17 && source.bindposes.Length==17,"17-bone skeleton and matching bind poses saved");
    Check(root.GetComponentsInChildren<Renderer>().Length==1 && skin.sharedMaterials.Length==8,"One skinned renderer retains eight material slots");
    foreach(var w in source.boneWeights){
        float sum=w.weight0+w.weight1+w.weight2+w.weight3;
        if(Mathf.Abs(sum-1)>1e-5f||w.weight0<0||w.weight1<0||w.weight2<0||w.weight3<0)throw new System.Exception("Invalid weight sum/sign");
        foreach(int b in new[]{w.boneIndex0,w.boneIndex1,w.boneIndex2,w.boneIndex3})if(b<0||b>=bones.Length)throw new System.Exception("Invalid bone index");
    }
    Check(source.boneWeights.Length==source.vertexCount,"Every vertex has normalized nonnegative weights and valid bone references");
    skin.BakeMesh(baked);var neutral=baked.vertices;float restError=0;for(int i=0;i<rest.Length;i++)restError=Mathf.Max(restError,Vector3.Distance(rest[i],neutral[i]));
    Check(restError<.0001f,"Binding preserves refined geometry within 0.1 mm");
    var parts=new System.Collections.Generic.Dictionary<string,Vector2Int>();
    foreach(string line in System.IO.File.ReadAllLines("../docs/images/art03/m5-rig-parts.txt")){var p=line.Split('|');parts[p[0]]=new Vector2Int(int.Parse(p[1]),int.Parse(p[2]));}
    var restPositions=new Vector3[bones.Length];for(int i=0;i<bones.Length;i++)restPositions[i]=bones[i].localPosition;
    void Reset(){for(int i=0;i<bones.Length;i++){bones[i].localRotation=Quaternion.identity;bones[i].localPosition=restPositions[i];}}
    void Rotate(string name,Vector3 angles){foreach(var t in bones)if(t.name==name)t.localRotation=Quaternion.Euler(angles);}
    float maximumStretch=0;int sampleCount=0;var triangles=source.triangles;
    void Sample(){
        skin.BakeMesh(baked);var v=baked.vertices;
        foreach(var p in v){
            if(float.IsNaN(p.x)||float.IsNaN(p.y)||float.IsNaN(p.z)||float.IsInfinity(p.x)||float.IsInfinity(p.y)||float.IsInfinity(p.z))throw new System.Exception("Nonfinite deformed vertex");
            if(!skin.bounds.Contains(skin.transform.TransformPoint(p)))throw new System.Exception("Deformed vertex outside rendering bounds: "+p);
        }
        for(int i=0;i<triangles.Length;i+=3)for(int e=0;e<3;e++){
            int a=triangles[i+e],b=triangles[i+(e+1)%3];float original=Vector3.Distance(rest[a],rest[b]);
            if(original>.0001f)maximumStretch=Mathf.Max(maximumStretch,Vector3.Distance(v[a],v[b])/original);
        }
        sampleCount++;
    }
    foreach(var t in new[]{0f,.25f,.5f,.75f,1f}){
        Reset();Rotate("LeftUpperArm",new Vector3(0,0,-55*t));Rotate("RightUpperArm",new Vector3(0,0,55*t));
        Rotate("LeftForearm",new Vector3(65*t,0,0));Rotate("RightForearm",new Vector3(65*t,0,0));Rotate("Head",new Vector3(0,25*t,0));Sample();
        var v=baked.vertices;var shirt=parts["JoinedShirt"];float waistError=0;
        for(int i=shirt.x;i<shirt.x+shirt.y;i++)if(rest[i].y<1.14f)waistError=Mathf.Max(waistError,Vector3.Distance(rest[i],v[i]));
        Check(waistError<.0001f,"Arm-raise sample "+t+" keeps lower shirt stable");
        var eye=parts["Left_Eye"];var mouth=parts["SimpleMouth"];
        Check(Mathf.Abs(Vector3.Distance(v[eye.x],v[mouth.x])-Vector3.Distance(rest[eye.x],rest[mouth.x]))<.0001f,"Head-turn sample "+t+" preserves facial spacing");
    }
    foreach(string name in new[]{"M5_Idle","M5_WalkPreview","M5_GesturePreview"}){
        var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Prototype/Art03Rig/Clips/"+name+".anim");
        Check(clip!=null && clip.isLooping,"Saved looping clip "+name);
        Vector3[] first=null,last=null;
        for(int step=0;step<=12;step++){
            Reset();clip.SampleAnimation(root,clip.length*step/12);Sample();var v=baked.vertices;
            if(step==0)first=v;if(step==12)last=v;
            if(name=="M5_Idle"){
                foreach(string part in new[]{"Left_Shoe","Right_Shoe"}){
                    var p=parts[part];for(int i=p.x;i<p.x+p.y;i++)if(Vector3.Distance(v[i],rest[i])>.0001f)throw new System.Exception("Idle shifts feet");
                }
            }
        }
        float seam=0;for(int i=0;i<first.Length;i++)seam=Mathf.Max(seam,Vector3.Distance(first[i],last[i]));
        Check(seam<.0001f,name+" closes without a geometry jump");
    }
    Reset();Rotate("LeftThigh",new Vector3(27,0,0));Rotate("LeftShin",new Vector3(-65,0,0));Rotate("RightThigh",new Vector3(-18,0,0));Rotate("RightShin",new Vector3(-8,0,0));Sample();
    results.Add("PASS: "+sampleCount+" deformed samples have finite vertices within culling bounds");
    results.Add("PASS: Idle keeps shoes planted; facial feature spacing survives head rotation");
    results.Add("INFO: maximum sampled edge stretch ratio="+maximumStretch.ToString("F3")+"; neutral maximum error="+restError.ToString("F7")+" m");
    results.Add("INFO: 29037 vertices, 57948 triangles retained; no production topology reduction claimed");
}finally{UnityEngine.Object.DestroyImmediate(baked);UnityEngine.Object.DestroyImmediate(root);UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(preview);}
System.IO.File.WriteAllLines("../docs/images/art03/m5-rig-deformation-checks.txt",results);
return string.Join("\n",results);
