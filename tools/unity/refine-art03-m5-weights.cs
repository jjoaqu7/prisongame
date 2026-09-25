// Deterministic weight-only correction on the saved rig.
if(EditorApplication.isPlayingOrWillChangePlaymode)throw new System.Exception("Stop Play first.");
var prefab=PrefabUtility.LoadPrefabContents("Assets/Prototype/Art03Rig/M5_Rigged.prefab");
try{
    var skin=prefab.GetComponentInChildren<SkinnedMeshRenderer>();var mesh=skin.sharedMesh;var vertices=mesh.vertices;var weights=mesh.boneWeights;
    var indices=new System.Collections.Generic.Dictionary<string,int>();for(int i=0;i<skin.bones.Length;i++)indices[skin.bones[i].name]=i;
    float S(float a,float b,float v){float t=Mathf.InverseLerp(a,b,v);return t*t*(3-2*t);}
    foreach(string line in System.IO.File.ReadAllLines("../docs/images/art03/m5-rig-parts.txt")){
        var p=line.Split('|');if(p[0]!="JoinedShirt"&&p[0]!="JoinedTrousers")continue;
        int start=int.Parse(p[1]),count=int.Parse(p[2]);
        for(int i=start;i<start+count;i++){
            var v=vertices[i];string side=v.x<0?"Left":"Right";var w=new float[skin.bones.Length];
            void Add(string name,float value){w[indices[name]]+=Mathf.Max(0,value);}
            if(p[0]=="JoinedShirt"){
                float arm=S(.075f,.156f,Mathf.Abs(v.x))*S(1.145f,1.25f,v.y)*(1-S(1.332f,1.395f,v.y));
                float spine=S(.99f,1.14f,v.y),chest=S(1.17f,1.31f,v.y);
                Add("Hips",(1-arm)*(1-spine));Add("Spine",(1-arm)*spine*(1-chest));Add("Chest",(1-arm)*spine*chest);Add(side+"UpperArm",arm);
            }else{
                float thigh=(1-S(.82f,.93f,v.y))*Mathf.Lerp(1,S(.015f,.05f,Mathf.Abs(v.x)),S(.80f,.86f,v.y));
                float shin=1-S(.465f,.565f,v.y),foot=1-S(.105f,.175f,v.y);
                Add("Hips",1-thigh);Add(side+"Thigh",thigh*(1-shin));Add(side+"Shin",thigh*shin*(1-foot));Add(side+"Foot",thigh*shin*foot);
            }
            var ids=new System.Collections.Generic.List<int>();for(int b=0;b<w.Length;b++)if(w[b]>0)ids.Add(b);ids.Sort((a,b)=>w[b].CompareTo(w[a]));
            float sum=0;for(int b=0;b<Mathf.Min(4,ids.Count);b++)sum+=w[ids[b]];
            var bw=new BoneWeight();bw.boneIndex0=ids[0];bw.weight0=w[ids[0]]/sum;
            if(ids.Count>1){bw.boneIndex1=ids[1];bw.weight1=w[ids[1]]/sum;}
            if(ids.Count>2){bw.boneIndex2=ids[2];bw.weight2=w[ids[2]]/sum;}
            if(ids.Count>3){bw.boneIndex3=ids[3];bw.weight3=w[ids[3]]/sum;}weights[i]=bw;
        }
    }
    mesh.boneWeights=weights;EditorUtility.SetDirty(mesh);AssetDatabase.SaveAssets();
}finally{PrefabUtility.UnloadPrefabContents(prefab);}
return "Confined shirt arm influence to shoulder/armpit; stabilized pelvis and crotch weights.";
