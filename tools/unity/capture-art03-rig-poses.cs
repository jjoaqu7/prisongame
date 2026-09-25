// Render baked results of real bone deformation in a temporary neutral studio.
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
    for(int pose=0;pose<3;pose++){
        var root=(GameObject)PrefabUtility.InstantiatePrefab(prefab);root.GetComponent<Animator>().enabled=false;
        var transforms=root.GetComponentsInChildren<Transform>();
        void Rotate(string name,Vector3 euler){foreach(var t in transforms)if(t.name==name)t.localRotation=Quaternion.Euler(euler);}
        if(pose==1){
            Rotate("LeftUpperArm",new Vector3(0,0,-55));Rotate("LeftForearm",new Vector3(65,0,0));
            Rotate("RightUpperArm",new Vector3(0,0,55));Rotate("RightForearm",new Vector3(65,0,0));
            Rotate("Head",new Vector3(0,20,0));
        }else if(pose==2){
            Rotate("LeftThigh",new Vector3(27,0,0));Rotate("LeftShin",new Vector3(-65,0,0));
            Rotate("RightThigh",new Vector3(-18,0,0));Rotate("RightShin",new Vector3(-8,0,0));
            Rotate("LeftUpperArm",new Vector3(-18,0,0));Rotate("RightUpperArm",new Vector3(20,0,0));
            Rotate("RightForearm",new Vector3(30,0,0));
            root.transform.rotation=Quaternion.Euler(0,-32,0);
        }
        var skin=root.GetComponentInChildren<SkinnedMeshRenderer>();var mesh=new Mesh();skin.BakeMesh(mesh);transient.Add(mesh);
        var display=new GameObject("Baked pose");display.transform.SetParent(root.transform,false);
        display.AddComponent<MeshFilter>().sharedMesh=mesh;display.AddComponent<MeshRenderer>().sharedMaterials=skin.sharedMaterials;skin.enabled=false;
        root.transform.position=new Vector3((pose-1)*1.16f,0,0);
    }
    var cam=new GameObject("Pose camera").AddComponent<Camera>();cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.67f,.69f,.70f);
    cam.transform.position=new Vector3(0,1.20f,-6);cam.transform.LookAt(new Vector3(0,1.02f,0));cam.orthographic=true;cam.orthographicSize=1.15f;
    var rt=new RenderTexture(1800,1000,24);var tex=new Texture2D(1800,1000,TextureFormat.RGB24,false);var old=RenderTexture.active;
    try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1800,1000),0,0);tex.Apply();System.IO.File.WriteAllBytes("../docs/images/art03/m5-rig-poses.png",tex.EncodeToPNG());}
    finally{cam.targetTexture=null;RenderTexture.active=old;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
}finally{UnityEditor.SceneManagement.EditorSceneManager.OpenScene(saved);foreach(var o in transient)if(o!=null)UnityEngine.Object.DestroyImmediate(o);}
return "Captured real deformation: left neutral, center shoulder/elbow/head test, right hip/knee stride. Saved scene restored.";
