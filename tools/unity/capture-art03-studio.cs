if(EditorApplication.isPlayingOrWillChangePlaymode)throw new System.Exception("Stop Play first.");
var original=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if(original.isDirty)throw new System.Exception("Save pending scene changes.");
string path=original.path;
bool refined=path=="Assets/Scenes/Art03_M5_Refined.unity";
Material ground=null;
try {
    UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,UnityEditor.SceneManagement.NewSceneMode.Single);
    var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(refined?"Assets/Prototype/Art03Refined/M5_Refined.prefab":"Assets/Prototype/Art03/M5_Static.prefab");
    PrefabUtility.InstantiatePrefab(prefab);
    RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.55f,.55f,.55f);
    RenderSettings.fog=false;
    foreach(int side in new[]{-1,1}){var g=new GameObject("Studio light");var l=g.AddComponent<Light>();l.type=LightType.Directional;l.intensity=side<0?1.8f:.6f;g.transform.rotation=Quaternion.Euler(side<0?35:20,side<0?35:-45,0);l.shadows=LightShadows.Soft;}
    var plane=GameObject.CreatePrimitive(PrimitiveType.Plane);plane.transform.position=new Vector3(0,-.004f,0);
    ground=new Material(Shader.Find("Universal Render Pipeline/Lit"));ground.SetColor("_BaseColor",new Color(.65f,.66f,.65f));plane.GetComponent<Renderer>().sharedMaterial=ground;
    var camera=new GameObject("Studio camera").AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.66f,.68f,.69f);
    camera.transform.position=new Vector3(.85f,1.3f,-4.2f);camera.transform.LookAt(new Vector3(0,.99f,0));camera.fieldOfView=30;
    var rt=new RenderTexture(1080,1080,24);var tex=new Texture2D(1080,1080,TextureFormat.RGB24,false);var previous=RenderTexture.active;
    try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1080,1080),0,0);tex.Apply();System.IO.File.WriteAllBytes("../docs/images/art03/"+(refined?"m5-refined-studio.png":"m5-studio.png"),tex.EncodeToPNG());}
    finally{camera.targetTexture=null;RenderTexture.active=previous;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
}finally{UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);if(ground!=null)UnityEngine.Object.DestroyImmediate(ground);}
return "Neutral Unity studio capture written; original study restored.";
