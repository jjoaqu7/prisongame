// One-time static M5 study. Run through Pipeline eval_file outside Play mode.
// Native mesh assets, no runtime mesh generation. No rig or animation implied.
if(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)throw new System.Exception("Editor must be idle.");
for(int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)
    if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)throw new System.Exception("Save pending edits first.");
const string folder="Assets/Prototype/Art03";
const string scenePath="Assets/Scenes/Art03_M5_Study.unity";
if(AssetDatabase.IsValidFolder(folder)||System.IO.File.Exists(scenePath))throw new System.Exception("Study already exists; edit saved assets instead.");
var shader=Shader.Find("Universal Render Pipeline/Lit");
if(shader==null)throw new System.Exception("URP Lit unavailable.");
AssetDatabase.CreateFolder("Assets/Prototype","Art03");
AssetDatabase.CreateFolder(folder,"Meshes");
AssetDatabase.CreateFolder(folder,"Materials");
Material Mat(string name,Color color) {
    var m=new Material(shader);m.name=name;m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",.12f);m.SetFloat("_Metallic",0);
    AssetDatabase.CreateAsset(m,folder+"/Materials/"+name+".mat");return m;
}
var skin=Mat("M5_Skin",new Color(.72f,.49f,.29f));
var shirt=Mat("M5_Ochre",new Color(.57f,.36f,.105f));
var pants=Mat("M5_BlueGrey",new Color(.23f,.30f,.36f));
var hair=Mat("M5_HairBrow",new Color(.16f,.105f,.065f));
var white=Mat("M5_Eyes",new Color(.88f,.86f,.79f));
var pupil=Mat("M5_Pupil",new Color(.018f,.022f,.02f));
var shoes=Mat("M5_Shoes",new Color(.65f,.63f,.55f));
var soles=Mat("M5_Soles",new Color(.19f,.20f,.19f));
var root=new GameObject("M5 - static visual study");
GameObject Surface(string name,Vector3[] vertices,int[] triangles,Material mat) {
    var mesh=new Mesh();mesh.name=name;mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();
    AssetDatabase.CreateAsset(mesh,folder+"/Meshes/"+name+".asset");
    var g=new GameObject(name);g.transform.SetParent(root.transform,false);g.AddComponent<MeshFilter>().sharedMesh=mesh;
    g.AddComponent<MeshRenderer>().sharedMaterial=mat;return g;
}
// Each ring: y, horizontal radius, depth radius. Circular seam is shared.
GameObject Rings(string name,Vector3[] rings,Material mat,float x=0,float z=0,int sides=32) {
    var v=new System.Collections.Generic.List<Vector3>();var t=new System.Collections.Generic.List<int>();
    foreach(var r in rings)for(int j=0;j<sides;j++){float a=j*Mathf.PI*2/sides;v.Add(new Vector3(x+Mathf.Cos(a)*r.y,r.x,z+Mathf.Sin(a)*r.z));}
    for(int i=0;i<rings.Length-1;i++)for(int j=0;j<sides;j++){
        int a=i*sides+j,b=i*sides+(j+1)%sides,c=a+sides,d=b+sides;
        t.Add(a);t.Add(c);t.Add(b);t.Add(b);t.Add(c);t.Add(d);
    }
    int bottom=v.Count;v.Add(new Vector3(x,rings[0].x,z));int top=v.Count;v.Add(new Vector3(x,rings[rings.Length-1].x,z));
    for(int j=0;j<sides;j++){int next=(j+1)%sides;t.Add(bottom);t.Add(j);t.Add(next);
        int offset=(rings.Length-1)*sides;t.Add(top);t.Add(offset+next);t.Add(offset+j);}
    return Surface(name,v.ToArray(),t.ToArray(),mat);
}
GameObject Ellipsoid(string name,Vector3 center,Vector3 size,Material mat) {
    var rs=new Vector3[17];for(int i=0;i<rs.Length;i++){float a=-Mathf.PI/2+Mathf.PI*i/(rs.Length-1);rs[i]=new Vector3(center.y+Mathf.Sin(a)*size.y/2,Mathf.Max(.0001f,Mathf.Cos(a)*size.x/2),Mathf.Max(.0001f,Mathf.Cos(a)*size.z/2));}
    return Rings(name,rs,mat,center.x,center.z,32);
}
GameObject Link(string name,Vector3 a,Vector3 b,float width,float depth,Material mat) {
    float length=Vector3.Distance(a,b);var obj=Ellipsoid(name,Vector3.zero,new Vector3(width,length+width*.45f,depth),mat);
    obj.transform.localPosition=(a+b)*.5f;obj.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);return obj;
}
Rings("Shirt",new[]{new Vector3(.94f,.103f,.065f),new Vector3(.96f,.108f,.069f),new Vector3(1.09f,.105f,.067f),new Vector3(1.26f,.112f,.072f),new Vector3(1.34f,.119f,.074f),new Vector3(1.39f,.085f,.060f),new Vector3(1.405f,.042f,.038f)},shirt);
Ellipsoid("Neck",new Vector3(0,1.43f,0),new Vector3(.075f,.15f,.072f),skin);
Ellipsoid("Hips",new Vector3(0,.92f,0),new Vector3(.208f,.20f,.126f),pants);
foreach(int s in new[]{-1,1}) {
    string side=s<0?"Left":"Right";
    Rings(side+"_Trouser",new[]{new Vector3(.095f,.039f,.041f),new Vector3(.13f,.042f,.043f),new Vector3(.47f,.034f,.037f),new Vector3(.67f,.040f,.043f),new Vector3(.94f,.052f,.060f)},pants,s*.063f);
    Ellipsoid(side+"_Shoe",new Vector3(s*.068f,.055f,-.022f),new Vector3(.10f,.085f,.18f),shoes);
    Ellipsoid(side+"_Sole",new Vector3(s*.068f,.017f,-.022f),new Vector3(.102f,.034f,.182f),soles);
    Link(side+"_Sleeve",new Vector3(s*.11f,1.34f,0),new Vector3(s*.144f,1.225f,0),.087f,.092f,shirt);
    Link(side+"_Arm",new Vector3(s*.145f,1.23f,0),new Vector3(s*.172f,.84f,-.01f),.047f,.048f,skin);
    Ellipsoid(side+"_Mitten",new Vector3(s*.174f,.804f,-.009f),new Vector3(.055f,.105f,.048f),skin);
    Ellipsoid(side+"_Thumb",new Vector3(s*.151f,.817f,-.029f),new Vector3(.027f,.053f,.025f),skin);
}
var headRings=new[]{new Vector3(1.46f,.018f,.025f),new Vector3(1.48f,.039f,.041f),new Vector3(1.53f,.065f,.061f),new Vector3(1.61f,.084f,.079f),new Vector3(1.72f,.103f,.091f),new Vector3(1.83f,.116f,.097f),new Vector3(1.9f,.106f,.089f),new Vector3(1.94f,.073f,.063f),new Vector3(1.955f,.004f,.004f)};
Rings("PebbleHead",headRings,skin,0,0,48);
Vector2 Radius(float y) {
    for(int i=0;i<headRings.Length-1;i++)if(y<=headRings[i+1].x){float u=Mathf.InverseLerp(headRings[i].x,headRings[i+1].x,y);return Vector2.Lerp(new Vector2(headRings[i].y,headRings[i].z),new Vector2(headRings[i+1].y,headRings[i+1].z),u);}
    return new Vector2(.004f,.004f);
}
var hv=new System.Collections.Generic.List<Vector3>();var ht=new System.Collections.Generic.List<int>();
for(int row=0;row<=16;row++)for(int j=0;j<48;j++){
    float a=j*Mathf.PI*2/48;
    float bottom=1.803f-.06f*Mathf.Sin(a)+.014f*Mathf.Cos(a);
    float y=Mathf.Lerp(bottom,1.956f,row/16f);var r=Radius(y);
    hv.Add(new Vector3(Mathf.Cos(a)*(r.x+.002f),y,Mathf.Sin(a)*(r.y+.002f)));
}
for(int row=0;row<16;row++)for(int j=0;j<48;j++){int a=row*48+j,b=row*48+(j+1)%48,c=a+48,d=b+48;ht.Add(a);ht.Add(c);ht.Add(b);ht.Add(b);ht.Add(c);ht.Add(d);}
Surface("HairCap",hv.ToArray(),ht.ToArray(),hair);
foreach(int s in new[]{-1,1}) {
    string side=s<0?"Left":"Right";
    Ellipsoid(side+"_Eye",new Vector3(s*.048f,1.785f,-.084f),new Vector3(.066f,.065f,.041f),white);
    Ellipsoid(side+"_Pupil",new Vector3(s*.048f+.012f,1.789f,-.104f),new Vector3(.025f,.037f,.009f),pupil);
}
// A gently curved solid brow bar hides the top of the eyes.
var bv=new System.Collections.Generic.List<Vector3>();var bt=new System.Collections.Generic.List<int>();
for(int i=0;i<=16;i++){
    float x=Mathf.Lerp(-.106f,.106f,i/16f);float z=-.11f+.035f*Mathf.Pow(x/.106f,2);
    bv.Add(new Vector3(x,1.801f,z));bv.Add(new Vector3(x,1.831f,z));bv.Add(new Vector3(x,1.831f,z+.023f));bv.Add(new Vector3(x,1.801f,z+.023f));
}
for(int i=0;i<16;i++)for(int k=0;k<4;k++){int a=i*4+k,b=i*4+(k+1)%4,c=a+4,d=b+4;bt.Add(a);bt.Add(b);bt.Add(c);bt.Add(b);bt.Add(d);bt.Add(c);}
bt.AddRange(new[]{0,2,1,0,3,2,64,65,66,64,66,67});
Surface("ConnectedBrow",bv.ToArray(),bt.ToArray(),hair);
for(int i=0;i<8;i++){
    float xa=Mathf.Lerp(-.022f,.022f,i/8f),xb=Mathf.Lerp(-.022f,.022f,(i+1)/8f);
    float ya=1.572f-.009f*Mathf.Pow(xa/.022f,2),yb=1.572f-.009f*Mathf.Pow(xb/.022f,2);
    Vector2 ra=Radius(ya),rb=Radius(yb);
    float za=-ra.y*Mathf.Sqrt(1-xa*xa/(ra.x*ra.x))-.002f,zb=-rb.y*Mathf.Sqrt(1-xb*xb/(rb.x*rb.x))-.002f;
    Link("Mouth_"+i,new Vector3(xa,ya,za),new Vector3(xb,yb,zb),.004f,.004f,hair);
}
var prefab=PrefabUtility.SaveAsPrefabAsset(root,folder+"/M5_Static.prefab");
UnityEngine.Object.DestroyImmediate(root);
var scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Art02_Combined.unity");
if(!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene,scenePath))throw new System.Exception("Study copy save failed.");
var inmate=GameObject.Find("Inmate - temporary character");
foreach(Transform child in inmate.transform)child.gameObject.SetActive(false);
var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,inmate.transform);instance.transform.localPosition=Vector3.zero;instance.transform.localRotation=Quaternion.identity;
var collider=inmate.GetComponent<CapsuleCollider>();collider.height=1.96f;collider.center=new Vector3(0,.98f,0);collider.radius=.23f;
inmate.name="Inmate - M5 static study";
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
int triangles=0;foreach(var mf in instance.GetComponentsInChildren<MeshFilter>())triangles+=mf.sharedMesh.triangles.Length/3;
return "Saved M5_Static prefab and Art03_M5_Study scene; "+triangles+" triangles, "+instance.GetComponentsInChildren<Renderer>().Length+" renderers. Existing inmate interaction retained. Static study only.";
