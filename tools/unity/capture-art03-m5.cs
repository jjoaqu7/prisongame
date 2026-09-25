// Capture real Unity rendering, then reopen the saved study to discard capture-only changes.
if(EditorApplication.isPlayingOrWillChangePlaymode)throw new System.Exception("Stop Play first.");
var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
bool refined=scene.path=="Assets/Scenes/Art03_M5_Refined.unity";
bool rigged=scene.path=="Assets/Scenes/Art03_M5_Rig.unity";
if(scene.isDirty||(!refined&&!rigged&&scene.path!="Assets/Scenes/Art03_M5_Study.unity"))throw new System.Exception("Open a saved M5 study first.");
string savedPath=scene.path;string prefix=rigged?"m5-rig-":refined?"m5-refined-":"m5-";
System.IO.Directory.CreateDirectory("../docs/images/art03");
try {
    var cam=Camera.main;var inmate=GameObject.Find("Inmate - M5 static study");
    Vector3[] modelPositions={new Vector3(2.4f,0,5.4f),new Vector3(2.4f,0,5.4f),new Vector3(2.4f,0,5.4f),new Vector3(-3.4f,0,-.2f)};
    Vector3[] cameraPositions={new Vector3(2.4f,1.7f,3.25f),new Vector3(2.4f,1.7f,4.35f),new Vector3(1.7f,1.7f,4.45f),new Vector3(-3.4f,1.7f,-1.25f)};
    string[] names={"common-full","common-face","common-three-quarter","cell-face"};
    for(int i=0;i<names.Length;i++){
        inmate.transform.position=modelPositions[i];cam.transform.position=cameraPositions[i];cam.fieldOfView=70;
        cam.transform.LookAt(modelPositions[i]+new Vector3(0,i==0?1.05f:1.70f,0));
        var rt=new RenderTexture(1440,1080,24);var tex=new Texture2D(1440,1080,TextureFormat.RGB24,false);var previous=RenderTexture.active;
        try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1440,1080),0,0);tex.Apply();System.IO.File.WriteAllBytes("../docs/images/art03/"+prefix+names[i]+".png",tex.EncodeToPNG());}
        finally{cam.targetTexture=null;RenderTexture.active=previous;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
    }
}finally{UnityEditor.SceneManagement.EditorSceneManager.OpenScene(savedPath);}
return "Captured full body and conversation views in shared/cell lighting; saved study reopened without capture changes.";
