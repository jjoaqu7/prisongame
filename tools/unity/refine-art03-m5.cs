// One-time refinement construction record, already applied. Edit saved assets normally.
if(EditorApplication.isPlayingOrWillChangePlaymode)throw new System.Exception("Stop Play first.");
const string folder="Assets/Prototype/Art03";
var existingEye=AssetDatabase.LoadAssetAtPath<Mesh>(folder+"/Meshes/Left_Eye.asset");
if(existingEye==null || existingEye.bounds.size.z<.03f)throw new System.Exception("Expected initial study; refinement already applied or source absent.");
void Colorize(string name,Color c){var m=AssetDatabase.LoadAssetAtPath<Material>(folder+"/Materials/"+name+".mat");m.SetColor("_BaseColor",c);EditorUtility.SetDirty(m);}
Colorize("M5_Skin",new Color(.86f,.64f,.41f));
Colorize("M5_Ochre",new Color(.78f,.54f,.20f));
Colorize("M5_BlueGrey",new Color(.32f,.41f,.48f));
Colorize("M5_HairBrow",new Color(.23f,.16f,.11f));
Colorize("M5_Shoes",new Color(.80f,.78f,.70f));
void WriteMesh(string name,Vector3[] vertices,int[] triangles) {
    var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(folder+"/Meshes/"+name+".asset");
    mesh.Clear();mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
}
// Extend the hair's front hairline upward to restore the concept's forehead.
var headRings=new[]{new Vector3(1.46f,.018f,.025f),new Vector3(1.48f,.039f,.041f),new Vector3(1.53f,.065f,.061f),new Vector3(1.61f,.084f,.079f),new Vector3(1.72f,.103f,.091f),new Vector3(1.83f,.116f,.097f),new Vector3(1.9f,.106f,.089f),new Vector3(1.94f,.073f,.063f),new Vector3(1.955f,.004f,.004f)};
Vector2 Radius(float y) {
    for(int i=0;i<headRings.Length-1;i++)if(y<=headRings[i+1].x){float u=Mathf.InverseLerp(headRings[i].x,headRings[i+1].x,y);return Vector2.Lerp(new Vector2(headRings[i].y,headRings[i].z),new Vector2(headRings[i+1].y,headRings[i+1].z),u);}
    return new Vector2(.004f,.004f);
}
var hv=new System.Collections.Generic.List<Vector3>();var ht=new System.Collections.Generic.List<int>();
for(int row=0;row<=16;row++)for(int j=0;j<48;j++){
    float a=j*Mathf.PI*2/48;float bottom=1.841f-.058f*Mathf.Sin(a)+.013f*Mathf.Cos(a);
    float y=Mathf.Lerp(bottom,1.957f,row/16f);var r=Radius(y);
    hv.Add(new Vector3(Mathf.Cos(a)*(r.x+.002f),y,Mathf.Sin(a)*(r.y+.002f)));
}
for(int row=0;row<16;row++)for(int j=0;j<48;j++){int a=row*48+j,b=row*48+(j+1)%48,c=a+48,d=b+48;ht.Add(a);ht.Add(c);ht.Add(b);ht.Add(b);ht.Add(c);ht.Add(d);}
int cap=hv.Count;hv.Add(new Vector3(0,1.959f,0));
for(int j=0;j<48;j++){ht.Add(cap);ht.Add(16*48+(j+1)%48);ht.Add(16*48+j);}
WriteMesh("HairCap",hv.ToArray(),ht.ToArray());
var prefab=PrefabUtility.LoadPrefabContents(folder+"/M5_Static.prefab");
try {
foreach(int s in new[]{-1,1}) {
    string side=s<0?"Left":"Right";
    var eye=AssetDatabase.LoadAssetAtPath<Mesh>(folder+"/Meshes/"+side+"_Eye.asset");var ev=eye.vertices;
    for(int i=0;i<ev.Length;i++){ev[i].y=1.793f+(ev[i].y-1.785f)*.88f;ev[i].z=-.084f+(ev[i].z+.084f)*.5f;}
    eye.vertices=ev;eye.RecalculateNormals();eye.RecalculateBounds();EditorUtility.SetDirty(eye);
    var pupil=AssetDatabase.LoadAssetAtPath<Mesh>(folder+"/Meshes/"+side+"_Pupil.asset");var pv=pupil.vertices;
    for(int i=0;i<pv.Length;i++){pv[i].y+=.004f;pv[i].z+=.010f;}
    pupil.vertices=pv;pupil.RecalculateBounds();EditorUtility.SetDirty(pupil);
    var obj=prefab.transform.Find(side+"_Sleeve");obj.localPosition=Vector3.zero;obj.localRotation=Quaternion.identity;
    Vector3[] rings={new Vector3(1.20f,.044f,.045f),new Vector3(1.22f,.045f,.047f),new Vector3(1.32f,.047f,.052f),new Vector3(1.36f,.030f,.04f),new Vector3(1.375f,.008f,.012f)};
    var v=new System.Collections.Generic.List<Vector3>();var t=new System.Collections.Generic.List<int>();
    foreach(var r in rings)for(int j=0;j<24;j++){float a=j*Mathf.PI*2/24;float x=s*(.133f+(1.32f-r.x)*.12f);v.Add(new Vector3(x+Mathf.Cos(a)*r.y,r.x,Mathf.Sin(a)*r.z));}
    for(int row=0;row<rings.Length-1;row++)for(int j=0;j<24;j++){int a=row*24+j,b=row*24+(j+1)%24,c=a+24,d=b+24;t.Add(a);t.Add(c);t.Add(b);t.Add(b);t.Add(c);t.Add(d);}
    int bottom=v.Count;v.Add(new Vector3(s*.1474f,1.2f,0));int top=v.Count;v.Add(new Vector3(s*.1264f,1.375f,0));
    for(int j=0;j<24;j++){t.Add(bottom);t.Add(j);t.Add((j+1)%24);t.Add(top);t.Add(96+(j+1)%24);t.Add(96+j);}
    WriteMesh(side+"_Sleeve",v.ToArray(),t.ToArray());
}
PrefabUtility.SaveAsPrefabAsset(prefab,folder+"/M5_Static.prefab");
}finally{PrefabUtility.UnloadPrefabContents(prefab);}
AssetDatabase.SaveAssets();
return "Refined sleeve silhouettes, restored forehead, sealed hair cap, reduced eye projection and lightened materials. Room lighting untouched.";
