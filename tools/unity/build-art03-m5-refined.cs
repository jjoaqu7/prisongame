// Create a versioned study with joined surfaces; the original remains available.
if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)throw new System.Exception("Editor must be idle.");
for(int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)throw new System.Exception("Save pending edits first.");
const string folder="Assets/Prototype/Art03Refined";
const string destination="Assets/Scenes/Art03_M5_Refined.unity";
if(AssetDatabase.IsValidFolder(folder)||System.IO.File.Exists(destination))throw new System.Exception("Refined study exists; edit the saved study.");
AssetDatabase.CreateFolder("Assets/Prototype","Art03Refined");AssetDatabase.CreateFolder(folder,"Meshes");AssetDatabase.CreateFolder(folder,"Materials");
var materials=new System.Collections.Generic.Dictionary<string,Material>();
foreach(string name in new[]{"Skin","Ochre","BlueGrey","HairBrow","Eyes","Pupil","Shoes","Soles"}){
    var original=AssetDatabase.LoadAssetAtPath<Material>("Assets/Prototype/Art03/Materials/M5_"+name+".mat");
    var copy=new Material(original);copy.name="M5_"+name;
    // A small stylized material fill preserves the face in deep shadows.
    // The non-static character does not contribute to baked lighting in this study.
    float fill=name=="Skin"?.12f:name=="Eyes"?.05f:name=="Pupil"?0f:.06f;
    copy.EnableKeyword("_EMISSION");copy.SetColor("_EmissionColor",copy.GetColor("_BaseColor").linear*fill);
    copy.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;
    AssetDatabase.CreateAsset(copy,folder+"/Materials/M5_"+name+".mat");materials.Add("M5_"+name,copy);
}
Mesh LoadObj(string name){
    var v=new System.Collections.Generic.List<Vector3>();var n=new System.Collections.Generic.List<Vector3>();var tris=new System.Collections.Generic.List<int>();
    var culture=System.Globalization.CultureInfo.InvariantCulture;
    foreach(string line in System.IO.File.ReadLines("../art-source/characters/m5/refined/"+name+".obj")){
        var a=line.Split(new[]{' '},System.StringSplitOptions.RemoveEmptyEntries);
        if(a.Length==0)continue;
        if(a[0]=="v"||a[0]=="vn"){
            var vector=new Vector3(float.Parse(a[1],culture),float.Parse(a[2],culture),float.Parse(a[3],culture));if(a[0]=="v")v.Add(vector);else n.Add(vector);
        }else if(a[0]=="f"){for(int j=1;j<=3;j++)tris.Add(int.Parse(a[j].Split('/')[0])-1);}
    }
    var mesh=new Mesh();mesh.name=name;if(v.Count>65535)mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
    mesh.SetVertices(v);mesh.SetTriangles(tris,0);mesh.SetNormals(n);mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,folder+"/Meshes/"+name+".asset");return mesh;
}
var originalPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prototype/Art03/M5_Static.prefab");
var model=UnityEngine.Object.Instantiate(originalPrefab);model.name="M5 - refined static study";
try{
    foreach(string name in new[]{"Shirt","Left_Sleeve","Right_Sleeve","Hips","Left_Trouser","Right_Trouser","PebbleHead","Neck","Left_Arm","Right_Arm","Left_Mitten","Right_Mitten","Left_Thumb","Right_Thumb","HairCap"})
        UnityEngine.Object.DestroyImmediate(model.transform.Find(name).gameObject);
    foreach(var renderer in model.GetComponentsInChildren<Renderer>())renderer.sharedMaterial=materials[renderer.sharedMaterial.name];
    void Part(string name,string material){
        var g=new GameObject(name);g.transform.SetParent(model.transform,false);g.AddComponent<MeshFilter>().sharedMesh=LoadObj(name);g.AddComponent<MeshRenderer>().sharedMaterial=materials["M5_"+material];
    }
    Part("JoinedShirt","Ochre");Part("JoinedTrousers","BlueGrey");Part("JoinedHeadNeck","Skin");Part("LeftJoinedArmHand","Skin");Part("RightJoinedArmHand","Skin");
    Part("SmoothHairCap","HairBrow");
    // Replace the eight short ellipsoids of the mouth with one curved tube.
    for(int i=0;i<8;i++)UnityEngine.Object.DestroyImmediate(model.transform.Find("Mouth_"+i).gameObject);
    var mv=new System.Collections.Generic.List<Vector3>();var mt=new System.Collections.Generic.List<int>();
    for(int i=0;i<=16;i++){
        float x=Mathf.Lerp(-.022f,.022f,i/16f);float y=1.572f-.008f*Mathf.Pow(x/.022f,2);
        float z=-.0715f*Mathf.Sqrt(1-x*x/(.076f*.076f))-.0022f;
        for(int j=0;j<8;j++){float a=j*Mathf.PI*2/8;mv.Add(new Vector3(x,y+Mathf.Cos(a)*.0022f,z+Mathf.Sin(a)*.0022f));}
    }
    for(int i=0;i<16;i++)for(int j=0;j<8;j++){int a=i*8+j,b=i*8+(j+1)%8,c=a+8,d=b+8;mt.Add(a);mt.Add(b);mt.Add(c);mt.Add(b);mt.Add(d);mt.Add(c);}
    int start=mv.Count;mv.Add((mv[0]+mv[4])*.5f);int end=mv.Count;mv.Add((mv[128]+mv[132])*.5f);
    for(int j=0;j<8;j++){mt.Add(start);mt.Add((j+1)%8);mt.Add(j);mt.Add(end);mt.Add(128+j);mt.Add(128+(j+1)%8);}
    var mouthMesh=new Mesh();mouthMesh.name="SimpleMouth";mouthMesh.SetVertices(mv);mouthMesh.SetTriangles(mt,0);mouthMesh.RecalculateNormals();mouthMesh.RecalculateBounds();
    AssetDatabase.CreateAsset(mouthMesh,folder+"/Meshes/SimpleMouth.asset");
    var mouth=new GameObject("SimpleMouth");mouth.transform.SetParent(model.transform,false);mouth.AddComponent<MeshFilter>().sharedMesh=mouthMesh;mouth.AddComponent<MeshRenderer>().sharedMaterial=materials["M5_HairBrow"];
    PrefabUtility.SaveAsPrefabAsset(model,folder+"/M5_Refined.prefab");
}finally{UnityEngine.Object.DestroyImmediate(model);}
var scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Art03_M5_Study.unity");
if(!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene,destination))throw new System.Exception("Scene copy failed.");
var inmate=GameObject.Find("Inmate - M5 static study");
foreach(Transform child in inmate.transform)child.gameObject.SetActive(false);
var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(folder+"/M5_Refined.prefab");
var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,inmate.transform);instance.transform.localPosition=Vector3.zero;instance.transform.localRotation=Quaternion.identity;
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
int count=0;foreach(var mf in instance.GetComponentsInChildren<MeshFilter>())count+=mf.sharedMesh.triangles.Length/3;
return "Refined static prefab/scene saved: "+count+" triangles, "+instance.GetComponentsInChildren<Renderer>().Length+" renderers. Joined surfaces and character-only material fill; room lights retained.";
