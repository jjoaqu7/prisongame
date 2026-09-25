// Read-only checks against saved meshes and unchanged room lighting.
if(EditorApplication.isPlayingOrWillChangePlaymode)throw new System.Exception("Stop Play first.");
var active=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if(active.isDirty)throw new System.Exception("Save pending edits.");
string original=active.path;
var results=new System.Collections.Generic.List<string>();
void Check(bool good,string message){if(!good)throw new System.Exception("FAIL: "+message);results.Add("PASS: "+message);}
string Lights(){
    var lines=new System.Collections.Generic.List<string>();
    foreach(var l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
        lines.Add(l.name+"|"+l.type+"|"+l.transform.position+"|"+l.transform.rotation+"|"+l.color+"|"+l.intensity+"|"+l.range+"|"+l.spotAngle+"|"+l.shadows+"|"+l.cullingMask);
    lines.Sort();return string.Join("\n",lines)+"\n"+RenderSettings.ambientMode+"|"+RenderSettings.ambientSkyColor+"|"+RenderSettings.ambientEquatorColor+"|"+RenderSettings.ambientGroundColor+"|"+RenderSettings.ambientIntensity;
}
try{
    UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Art03_M5_Study.unity");string before=Lights();
    UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Art03_M5_Refined.unity");Check(before==Lights(),"Room light transforms, colors, intensity, ranges, shadows and ambient settings match the original study");
    var root=GameObject.Find("Inmate - M5 static study");
    Check(root.GetComponent<PrisonGame.Prototype.PrototypeInmate>()!=null,"Original inmate interaction retained");
    Check(root.GetComponentsInChildren<Collider>().Length==1,"Only the root gameplay collider is active");
    var visual=root.transform.Find("M5_Refined");
    Check(visual!=null && PrefabUtility.IsPartOfPrefabInstance(visual.gameObject),"Saved refined prefab instance is connected");
    int triangles=0;var bounds=new Bounds(root.transform.position,Vector3.zero);
    foreach(var mf in visual.GetComponentsInChildren<MeshFilter>()){
        var mesh=mf.sharedMesh;Check(mesh!=null && AssetDatabase.Contains(mesh),mf.name+" is a saved mesh asset");
        foreach(var p in mesh.vertices)if(float.IsNaN(p.x)||float.IsNaN(p.y)||float.IsNaN(p.z)||float.IsInfinity(p.x)||float.IsInfinity(p.y)||float.IsInfinity(p.z))throw new System.Exception("Nonfinite vertex");
        Check(mesh.normals.Length==mesh.vertexCount,mf.name+" has vertex normals");triangles+=mesh.triangles.Length/3;
    }
    foreach(var r in visual.GetComponentsInChildren<Renderer>()){
        Check(r.sharedMaterial!=null && !ShaderUtil.ShaderHasError(r.sharedMaterial.shader),r.name+" has a valid shader/material");
        bounds.Encapsulate(r.bounds);
        Check(!GameObjectUtility.AreStaticEditorFlagsSet(r.gameObject,StaticEditorFlags.ContributeGI),r.name+" does not contribute baked light");
    }
    var skin=AssetDatabase.LoadAssetAtPath<Material>("Assets/Prototype/Art03Refined/Materials/M5_Skin.mat");
    Check(skin.IsKeywordEnabled("_EMISSION"),"Character material fill persists after saving and scene reload");
    var capsule=root.GetComponent<CapsuleCollider>();
    Check(bounds.max.y<=root.transform.position.y+capsule.center.y+capsule.height*.5f+.005f,"Visual head remains within interaction collider height");
    Check(bounds.size.x<.46f && bounds.size.y>1.9f && bounds.size.y<2f,"M5 remains within the original narrow size envelope");
    results.Add("INFO: triangles="+triangles+"; renderers="+visual.GetComponentsInChildren<Renderer>().Length+"; bounds="+bounds.size);
}finally{UnityEditor.SceneManagement.EditorSceneManager.OpenScene(original);}
System.IO.File.WriteAllLines("../docs/images/art03/m5-refined-verification.txt",results);
return string.Join("\n",results);
