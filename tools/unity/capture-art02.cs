// Capture matching player-height views without saving camera/door changes.
if(EditorApplication.isPlayingOrWillChangePlaymode) throw new System.Exception("Stop Play mode first.");
var active=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if(active.isDirty)throw new System.Exception("Save pending scene changes first.");
var original=active.path;
System.IO.Directory.CreateDirectory("../docs/images/art02");
try {
var variants=new System.Collections.Generic.List<string>{"A_Amber","B_SoftCool"};
if(System.IO.File.Exists("Assets/Scenes/Art02_Combined.unity"))variants.Add("Combined");
foreach(var variant in variants) {
    UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Art02_"+variant+".unity");
    var cam=Camera.main;
    var door=GameObject.Find("Cell sliding door");door.transform.position+=new Vector3(0,0,1.5f);
    Vector3[] positions={new Vector3(-2.8f,1.7f,-1.65f),new Vector3(-3.9f,1.7f,-.95f),new Vector3(.1f,1.7f,1.1f)};
    Vector3[] targets={new Vector3(-4.9f,1.2f,.6f),new Vector3(1f,1.4f,.45f),new Vector3(4.7f,1.35f,3.8f)};
    string[] views={"cell","corridor","common"};
    for(int i=0;i<views.Length;i++) {
        cam.transform.position=positions[i];cam.transform.LookAt(targets[i]);
        var rt=new RenderTexture(1280,720,24);var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);
        var old=RenderTexture.active;
        try {cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();
            System.IO.File.WriteAllBytes("../docs/images/art02/"+variant+"-"+views[i]+".png",tex.EncodeToPNG());
        } finally {cam.targetTexture=null;RenderTexture.active=old;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
    }
}
} finally {UnityEditor.SceneManagement.EditorSceneManager.OpenScene(original);}
return "Matching camera captures written to docs/images/art02; original scene reopened without capture changes.";
