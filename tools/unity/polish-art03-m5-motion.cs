// Bake foot-targeted walking into ordinary Unity clips; no runtime IK dependency.
if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)throw new System.Exception("Editor must be idle.");
for(int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)throw new System.Exception("Save pending edits first.");
const string folder="Assets/Prototype/Art03Rig";
const string archive="../art-source/characters/m5/rig-before-motion-polish";
System.IO.Directory.CreateDirectory(archive);
foreach(string file in new[]{"M5_Weighted.asset","Clips/M5_Idle.anim","Clips/M5_WalkPreview.anim","Clips/M5_GesturePreview.anim"}){
    string backup=archive+"/"+System.IO.Path.GetFileName(file);
    if(!System.IO.File.Exists(backup))System.IO.File.Copy(folder+"/"+file,backup);
}
var preview=UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
var root=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(folder+"/M5_Rigged.prefab"));
UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root,preview);
var report=new System.Collections.Generic.List<string>();
try{
    root.GetComponent<Animator>().enabled=false;
    var skin=root.GetComponentInChildren<SkinnedMeshRenderer>();var mesh=skin.sharedMesh;var vertices=mesh.vertices;var weights=mesh.boneWeights;
    var bones=skin.bones;var map=new System.Collections.Generic.Dictionary<string,Transform>();var ids=new System.Collections.Generic.Dictionary<string,int>();
    var rest=new Vector3[bones.Length];for(int i=0;i<bones.Length;i++){map[bones[i].name]=bones[i];ids[bones[i].name]=i;rest[i]=bones[i].localPosition;}
    float Smooth(float a,float b,float v){float t=Mathf.InverseLerp(a,b,v);return t*t*(3-2*t);}
    var parts=new System.Collections.Generic.Dictionary<string,Vector2Int>();
    foreach(string line in System.IO.File.ReadAllLines("../docs/images/art03/m5-rig-parts.txt")){var p=line.Split('|');parts[p[0]]=new Vector2Int(int.Parse(p[1]),int.Parse(p[2]));}
    void Reset(){for(int i=0;i<bones.Length;i++){bones[i].localRotation=Quaternion.identity;bones[i].localPosition=rest[i];}}
    float JointStretch(string part,string joint,float angle){
        Reset();map[joint].localRotation=Quaternion.Euler(angle,0,0);var baked=new Mesh();
        try{skin.BakeMesh(baked);var v=baked.vertices;var p=parts[part];float maximum=0;
            var triangles=mesh.triangles;for(int i=0;i<triangles.Length;i+=3)for(int e=0;e<3;e++){
                int a=triangles[i+e],b=triangles[i+(e+1)%3];if(a<p.x||a>=p.x+p.y||b<p.x||b>=p.x+p.y)continue;
                float length=Vector3.Distance(vertices[a],vertices[b]);if(length>.0001f)maximum=Mathf.Max(maximum,Vector3.Distance(v[a],v[b])/length);
            }return maximum;
        }finally{UnityEngine.Object.DestroyImmediate(baked);Reset();}
    }
    float beforeKnee=JointStretch("JoinedTrousers","LeftShin",-65),beforeElbow=JointStretch("LeftJoinedArmHand","LeftForearm",65);
    foreach(var part in parts){
        if(part.Key!="JoinedTrousers"&&!part.Key.Contains("JoinedArmHand"))continue;
        for(int i=part.Value.x;i<part.Value.x+part.Value.y;i++){
            var v=vertices[i];string side=v.x<0?"Left":"Right";var w=new float[bones.Length];
            void Add(string name,float value){w[ids[name]]+=Mathf.Max(0,value);}
            if(part.Key=="JoinedTrousers"){
                float thigh=(1-Smooth(.82f,.93f,v.y))*Mathf.Lerp(1,Smooth(.015f,.05f,Mathf.Abs(v.x)),Smooth(.80f,.86f,v.y));
                float shin=1-Smooth(.43f,.60f,v.y),foot=1-Smooth(.105f,.175f,v.y);
                Add("Hips",1-thigh);Add(side+"Thigh",thigh*(1-shin));Add(side+"Shin",thigh*shin*(1-foot));Add(side+"Foot",thigh*shin*foot);
            }else{
                float forearm=1-Smooth(.955f,1.12f,v.y),hand=1-Smooth(.788f,.861f,v.y);
                Add(side+"UpperArm",1-forearm);Add(side+"Forearm",forearm*(1-hand));Add(side+"Hand",forearm*hand);
            }
            var order=new System.Collections.Generic.List<int>();for(int b=0;b<w.Length;b++)if(w[b]>0)order.Add(b);order.Sort((a,b)=>w[b].CompareTo(w[a]));
            float sum=0;for(int b=0;b<Mathf.Min(4,order.Count);b++)sum+=w[order[b]];
            var bw=new BoneWeight();bw.boneIndex0=order[0];bw.weight0=w[order[0]]/sum;
            if(order.Count>1){bw.boneIndex1=order[1];bw.weight1=w[order[1]]/sum;}if(order.Count>2){bw.boneIndex2=order[2];bw.weight2=w[order[2]]/sum;}if(order.Count>3){bw.boneIndex3=order[3];bw.weight3=w[order[3]]/sum;}weights[i]=bw;
        }
    }
    mesh.boneWeights=weights;EditorUtility.SetDirty(mesh);
    report.Add("65-degree isolated bend maximum edge stretch: knee "+beforeKnee.ToString("F3")+" -> "+JointStretch("JoinedTrousers","LeftShin",-65).ToString("F3")+"; elbow "+beforeElbow.ToString("F3")+" -> "+JointStretch("LeftJoinedArmHand","LeftForearm",65).ToString("F3"));
    // Forward is -Z. Contact moves backward at 0.26 / 0.6 = 0.433333 m/s,
    // to cancel a future root translation of that speed on level ground.
    void Leg(string side,float phase){
        phase=Mathf.Repeat(phase,1);float z,lift;
        if(phase<=.5f){z=Mathf.Lerp(-.13f,.13f,phase*2);lift=0;}
        else{
            float u=(phase-.5f)*2;float u2=u*u,u3=u2*u;
            z=(2*u3-3*u2+1)*.13f+(u3-2*u2+u)*.26f+(-2*u3+3*u2)*(-.13f)+(u3-u2)*.26f;
            lift=.065f*Mathf.Pow(Mathf.Sin(Mathf.PI*u),2);
        }
        var hip=map[side+"Thigh"];var knee=map[side+"Shin"];var ankle=map[side+"Foot"];
        var first=rest[ids[side+"Shin"]];var second=rest[ids[side+"Foot"]];
        var target=new Vector3(side=="Left"?-.068f:.068f,.092f+lift,-.012f+z);
        var delta=target-hip.position;float distance=delta.magnitude,a=first.magnitude,b=second.magnitude;
        if(distance>=a+b||distance<=Mathf.Abs(a-b))throw new System.Exception("Unreachable foot target");
        var direction=delta/distance;float along=(a*a-b*b+distance*distance)/(2*distance);
        var bend=Vector3.ProjectOnPlane(Vector3.back,direction).normalized;
        var joint=hip.position+direction*along+bend*Mathf.Sqrt(Mathf.Max(0,a*a-along*along));
        hip.rotation=Quaternion.FromToRotation(first,joint-hip.position);
        knee.rotation=Quaternion.FromToRotation(second,target-joint);
        ankle.rotation=Quaternion.identity; // Whole sole stays flat; toe roll is deferred.
    }
    void SaveClip(string name,float duration,System.Action<float> pose){
        var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(folder+"/Clips/"+name+".anim");
        clip.ClearCurves();clip.frameRate=60;int steps=Mathf.RoundToInt(duration*60);
        var curves=new AnimationCurve[bones.Length,7];for(int b=0;b<bones.Length;b++)for(int c=0;c<7;c++)curves[b,c]=new AnimationCurve();
        for(int frame=0;frame<=steps;frame++){
            Reset();float phase=(float)frame/steps;pose(phase);float time=duration*phase;
            for(int b=0;b<bones.Length;b++){
                var q=bones[b].localRotation;var p=bones[b].localPosition;
                float[] values={q.x,q.y,q.z,q.w,p.x,p.y,p.z};for(int c=0;c<7;c++)curves[b,c].AddKey(time,values[c]);
            }
        }
        for(int b=0;b<bones.Length;b++)for(int c=0;c<7;c++){
            var curve=curves[b,c];for(int k=0;k<curve.length;k++){AnimationUtility.SetKeyLeftTangentMode(curve,k,AnimationUtility.TangentMode.Linear);AnimationUtility.SetKeyRightTangentMode(curve,k,AnimationUtility.TangentMode.Linear);}
            string property=c<4?"m_LocalRotation."+"xyzw"[c]:"m_LocalPosition."+"xyz"[c-4];
            AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve(AnimationUtility.CalculateTransformPath(bones[b],root.transform),typeof(Transform),property),curve);
        }
        clip.EnsureQuaternionContinuity();var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=true;settings.loopBlend=false;AnimationUtility.SetAnimationClipSettings(clip,settings);EditorUtility.SetDirty(clip);
    }
    SaveClip("M5_Idle",4,t=>{
        float a=Mathf.Sin(t*Mathf.PI*2);map["Spine"].localRotation=Quaternion.Euler(.8f*a,0,.35f*a);map["Chest"].localRotation=Quaternion.Euler(-.4f*a,0,0);map["Head"].localRotation=Quaternion.Euler(0,1.2f*a,0);
    });
    SaveClip("M5_WalkPreview",1.2f,t=>{
        map["Hips"].localPosition+=new Vector3(0,-.025f+.004f*Mathf.Cos(4*Mathf.PI*t),0);
        Leg("Left",t);Leg("Right",t+.5f);
        float a=Mathf.Cos(2*Mathf.PI*t);map["Chest"].localRotation=Quaternion.Euler(0,2*a,0);
        foreach(string side in new[]{"Left","Right"}){float sign=side=="Left"?1:-1;map[side+"UpperArm"].localRotation=Quaternion.Euler(-9*sign*a,0,side=="Left"?-3:3);map[side+"Forearm"].localRotation=Quaternion.Euler(8+3*sign*a,0,0);}
    });
    SaveClip("M5_GesturePreview",2.4f,t=>{
        float a=.5f-.5f*Mathf.Cos(t*Mathf.PI*2);
        map["LeftUpperArm"].localRotation=Quaternion.Euler(10*a,0,-16*a);map["LeftForearm"].localRotation=Quaternion.Euler(52*a,0,0);
        map["Chest"].localRotation=Quaternion.Euler(0,-2*a,0);map["Head"].localRotation=Quaternion.Euler(4*Mathf.Sin(t*Mathf.PI*4)*a,-10*a,0);
    });
    AssetDatabase.SaveAssets();report.Add("Saved 60 Hz foot-targeted walk, softened gesture and explicit rest positions in all clips. Original assets archived outside Assets. No runtime IK or movement added.");
}finally{UnityEngine.Object.DestroyImmediate(root);UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(preview);}
System.IO.File.WriteAllLines("../docs/images/art03/m5-motion-polish-build.txt",report);return string.Join("\n",report);
