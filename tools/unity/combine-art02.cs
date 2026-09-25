// Pipeline eval_file outside Play mode. Combine approved regions without overwriting A/B.
if(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
    throw new System.Exception("Editor must be idle.");
for(int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)
    if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
        throw new System.Exception("Save pending scene edits first.");
const string destination="Assets/Scenes/Art02_Combined.unity";
if(System.IO.File.Exists(destination))throw new System.Exception("Combined scene exists; edit it normally.");
var scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Art02_B_SoftCool.unity");
if(!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene,destination))
    throw new System.Exception("Could not save combined scene.");
int cellMaterials=0;
foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)) {
    // Cell's east wall and door sit at x=-2; the corridor starts east of that boundary.
    if(r.transform.position.x > -1.99f || r.transform.position.z < -3.01f || r.transform.position.z > 2.01f || r.sharedMaterial==null)continue;
    string path=AssetDatabase.GetAssetPath(r.sharedMaterial).Replace("Art02_B_SoftCool_","Art02_A_Amber_");
    var m=AssetDatabase.LoadAssetAtPath<Material>(path);
    if(m==null)throw new System.Exception("Missing amber material: "+path);
    r.sharedMaterial=m;cellMaterials++;
}
foreach(var light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None)) {
    if(light.name=="Cell warm ceiling light" || light.name=="Bunk reading lamp light") {
        light.color=new Color(1f,.69f,.37f);
        light.intensity=light.name=="Cell warm ceiling light"?2.2f:1.1f;
    }
}
// Retain B's ambient fill throughout: a fixed shared level avoids camera-triggered lighting changes.
// Actual warm/cool transitions come from local lights and their wall occlusion.
GameObject.Find("ART-02 - Proposed atmosphere B soft cool").name="ART-02 - Amber cell and cool shared areas";
if(!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene))throw new System.Exception("Save failed.");
return "Saved combined scene; "+cellMaterials+" cell renderers use A materials; shared areas retain B lighting/materials.";
