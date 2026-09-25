// Actual Unity clip poses; no generated/repainted imagery.
if(EditorApplication.isPlayingOrWillChangePlaymode)throw new System.Exception("Stop Play first.");
var original=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(original.isDirty)throw new System.Exception("Save pending changes.");
string saved=original.path;var transient=new System.Collections.Generic.List<UnityEngine.Object>();
try{
    UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,UnityEditor.SceneManagement.NewSceneMode.Single);
    RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.55f,.55f,.55f);RenderSettings.fog=false;
    foreach(int side in new[]{-1,1}){var light=new GameObject("Studio light").AddComponent<Light>();light.type=LightType.Directional;light.intensity=side<0?1.6f:.6f;light.transform.rotation=Quaternion.Euler(side<0?35:20,side<0?35:-45,0);light.shadows=LightShadows.Soft;}
    var plane=GameObject.CreatePrimitive(PrimitiveType.Plane);plane.transform.position=new Vector3(0,-.004f,0);
    var mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));mat.SetColor("_BaseColor",new Color(.65f,.66f,.65f));plane.GetComponent<Renderer>().sharedMaterial=mat;transient.Add(mat);
    var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prototype/Art03Rig/M5_Rigged.prefab");
    var walk=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Prototype/Art03Rig/Clips/M5_WalkPreview.anim");
    var gesture=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Prototype/Art03Rig/Clips/M5_GesturePreview.anim");
    for(int pose=0;pose<4;pose++){
        var root=(GameObject)PrefabUtility.InstantiatePrefab(prefab);root.GetComponent<Animator>().enabled=false;
        if(pose<3)walk.SampleAnimation(root,walk.length*pose*.25f);else gesture.SampleAnimation(root,gesture.length*.5f);
        var skin=root.GetComponentInChildren<SkinnedMeshRenderer>();var mesh=new Mesh();skin.BakeMesh(mesh);transient.Add(mesh);
        var display=new GameObject("Baked clip pose");display.transform.SetParent(root.transform,false);display.AddComponent<MeshFilter>().sharedMesh=mesh;display.AddComponent<MeshRenderer>().sharedMaterials=skin.sharedMaterials;skin.enabled=false;
        root.transform.rotation=Quaternion.Euler(0,pose<3?-65:-25,0);root.transform.position=new Vector3((pose-1.5f)*.85f,0,0);
    }
    var cam=new GameObject("Motion camera").AddComponent<Camera>();cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.67f,.69f,.70f);
    cam.transform.position=new Vector3(0,1.20f,-6);cam.transform.LookAt(new Vector3(0,1.02f,0));cam.orthographic=true;cam.orthographicSize=1.15f;
    var rt=new RenderTexture(1800,1000,24);var tex=new Texture2D(1800,1000,TextureFormat.RGB24,false);var old=RenderTexture.active;
    try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1800,1000),0,0);tex.Apply();System.IO.File.WriteAllBytes("../docs/images/art03/m5-polished-motion.png",tex.EncodeToPNG());}
    finally{cam.targetTexture=null;RenderTexture.active=old;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
}finally{UnityEditor.SceneManagement.EditorSceneManager.OpenScene(saved);foreach(var o in transient)if(o!=null)UnityEngine.Object.DestroyImmediate(o);}
return "Captured walk at 0%, 25%, 50%, and gesture at 50%. Saved scene restored.";
