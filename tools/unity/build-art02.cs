// Pipeline eval_file, outside Play mode. Creates two review scenes; never overwrites.
if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
    throw new System.Exception("Editor must be idle.");
for (int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)
    if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty) throw new System.Exception("Save existing scene changes first.");
string[] names={"Art02_A_Amber","Art02_B_SoftCool"};
foreach(var name in names) if(System.IO.File.Exists("Assets/Scenes/"+name+".unity")) throw new System.Exception("Review scene already exists.");
if(!AssetDatabase.IsValidFolder("Assets/Prototype/Art02")) AssetDatabase.CreateFolder("Assets/Prototype","Art02");
for(int variant=0;variant<2;variant++) {
    bool a=variant==0;
    var scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Room01_Blockout.unity");
    // Save as first so subsequent edits belong to the review scene.
    var path="Assets/Scenes/"+names[variant]+".unity";
    if(!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene,path)) throw new System.Exception("Copy save failed.");
    var mats=new System.Collections.Generic.Dictionary<string,Material>();
    foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)) {
        var old=r.sharedMaterial; if(old==null) continue;
        if(!mats.TryGetValue(old.name,out var m)) {
            m=new Material(old); m.name=names[variant]+"_"+old.name;
            Color c=old.GetColor("_BaseColor");
            switch(old.name) {
                case "Concrete_Upper": c=a?new Color(.60f,.57f,.47f):new Color(.63f,.65f,.62f);break;
                case "Institutional_Green": c=a?new Color(.26f,.34f,.29f):new Color(.28f,.37f,.40f);break;
                case "Floor_Cell": c=new Color(.34f,.32f,.27f);break;
                case "Floor_Corridor": c=new Color(.25f,.28f,.28f);break;
                case "Floor_Common": c=new Color(.38f,.40f,.35f);break;
                case "Placeholder_Mattress": c=a?new Color(.34f,.38f,.26f):new Color(.32f,.39f,.43f);break;
                case "Dark_Metal": c=new Color(.21f,.24f,.23f);break;
            }
            m.SetColor("_BaseColor",c);m.SetFloat("_Smoothness",old.name.Contains("Metal")?.32f:.12f);
            AssetDatabase.CreateAsset(m,"Assets/Prototype/Art02/"+m.name+".mat");mats.Add(old.name,m);
        }
        r.sharedMaterial=m;
    }
    foreach(var light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None)) light.enabled=false;
    RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
    RenderSettings.ambientLight=a?new Color(.19f,.22f,.21f):new Color(.23f,.27f,.30f);
    RenderSettings.fog=false;
    var root=new GameObject("ART-02 - Proposed atmosphere "+(a?"A amber":"B soft cool"));
    GameObject Box(string name,Vector3 p,Vector3 s,Material m) {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(root.transform);
        g.transform.position=p;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=m;return g;
    }
    // Enclose the test space using the existing wall height.
    Box("Cell ceiling",new Vector3(-4,3.3f,-.5f),new Vector3(4,.2f,5),mats["Concrete_Upper"]);
    Box("Corridor ceiling",new Vector3(-.5f,3.3f,0),new Vector3(3,.2f,14),mats["Concrete_Upper"]);
    Box("Common ceiling",new Vector3(4.5f,3.3f,3.5f),new Vector3(7,.2f,7),mats["Concrete_Upper"]);
    var glow=new Material(Shader.Find("Universal Render Pipeline/Lit"));glow.name=names[variant]+"_Fixture";
    glow.SetColor("_BaseColor",new Color(.85f,.80f,.64f));glow.EnableKeyword("_EMISSION");glow.SetColor("_EmissionColor",new Color(1.2f,1.05f,.75f));
    AssetDatabase.CreateAsset(glow,"Assets/Prototype/Art02/"+glow.name+".mat");
    void Lamp(string name,Vector3 pos,Color color,float power,float range,Vector3 size) {
        Box(name+" housing",pos+Vector3.up*.12f,size+new Vector3(.08f,.08f,.08f),mats["Dark_Metal"]).GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        Box(name+" diffuser",pos+Vector3.up*.075f,size,glow).GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        var g=new GameObject(name+" light");g.transform.SetParent(root.transform);g.transform.position=pos;
        var l=g.AddComponent<Light>();l.type=LightType.Point;l.color=color;l.intensity=power;l.range=range;
        l.shadows=LightShadows.Soft;l.shadowBias=.025f;l.shadowNormalBias=.15f;
    }
    var warm=a?new Color(1f,.69f,.37f):new Color(1f,.85f,.64f);
    var cool=a?new Color(.68f,.82f,.75f):new Color(.64f,.78f,1f);
    Lamp("Cell warm ceiling",new Vector3(-4,2.95f,-.5f),warm,a?2.2f:2.5f,5,new Vector3(.6f,.07f,.35f));
    Lamp("Bunk reading lamp",new Vector3(-5.65f,2.15f,.8f),warm,1.1f,2.8f,new Vector3(.18f,.08f,.4f));
    foreach(float z in new[]{-4.5f,0f,4.5f}) Lamp("Corridor strip "+z,new Vector3(-.5f,2.95f,z),cool,a?1.7f:2.1f,5,new Vector3(.25f,.06f,1.1f));
    foreach(float x in new[]{3f,6.3f}) Lamp("Common pendant "+x,new Vector3(x,2.9f,3.5f),a?new Color(1f,.82f,.57f):new Color(.86f,.91f,1f),2.7f,6,new Vector3(.6f,.08f,.6f));
    // Identical props in both samples, all outside the walking route.
    Box("Folded blanket",new Vector3(-5.15f,.60f,-.12f),new Vector3(1.08f,.05f,.65f),mats["Placeholder_Mattress"]);
    Box("Pillow",new Vector3(-5.15f,.64f,1.08f),new Vector3(.75f,.12f,.32f),mats["Concrete_Upper"]);
    Box("Book on desk",new Vector3(-3.48f,.85f,1.48f),new Vector3(.22f,.04f,.28f),mats["Institutional_Green"]);
    AssetDatabase.SaveAssets();UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
}
UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Art02_A_Amber.unity");
return "Created two playable ART-02 scenes with independent materials. Baseline/build list unchanged.";
