if(EditorApplication.isPlayingOrWillChangePlaymode)throw new System.Exception("Stop Play first.");
var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if(scene.name!="Save01_Progress" || scene.isDirty)throw new System.Exception("Open clean Save01 first.");
var shader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/Prototype/Scripts/WorldLabel.shader");
if(shader==null || ShaderUtil.ShaderHasError(shader))throw new System.Exception("Label shader must compile.");
int count=0;
foreach(var label in UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None))
{
    var component=label.GetComponent<PrisonGame.Prototype.WorldLabelDepth>();
    if(component==null)component=label.gameObject.AddComponent<PrisonGame.Prototype.WorldLabelDepth>();
    var so=new SerializedObject(component);so.FindProperty("labelShader").objectReferenceValue=shader;so.ApplyModifiedPropertiesWithoutUndo();count++;
}
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);return "Depth shader assigned to "+count+" world labels in Save01.";
