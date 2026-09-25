if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)throw new System.Exception("Editor must be idle.");
for(int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)throw new System.Exception("Save pending scene edits first.");
const string folder="Assets/Prototype/Art03Behavior";
const string scenePath="Assets/Scenes/Art03_M5_Behavior.unity";
if(AssetDatabase.IsValidFolder(folder)||System.IO.File.Exists(scenePath))throw new System.Exception("Behavior study exists; edit saved assets.");
AssetDatabase.CreateFolder("Assets/Prototype","Art03Behavior");
AssetDatabase.CopyAsset("Assets/Prototype/Art03Rig/M5_Study.controller",folder+"/M5_Behavior.controller");
var controller=AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(folder+"/M5_Behavior.controller");
controller.AddParameter("MoveRate",AnimatorControllerParameterType.Float);
foreach(var state in controller.layers[0].stateMachine.states)if(state.state.name=="WalkPreview"){state.state.speedParameter="MoveRate";state.state.speedParameterActive=true;EditorUtility.SetDirty(state.state);}
EditorUtility.SetDirty(controller);
var visual=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prototype/Art03Rig/M5_Rigged.prefab"));
try{
    visual.name="M5_Behavior";visual.GetComponent<Animator>().runtimeAnimatorController=controller;
    visual.AddComponent<PrisonGame.Prototype.InmateAnimation>();PrefabUtility.SaveAsPrefabAsset(visual,folder+"/M5_Behavior.prefab");
}finally{UnityEngine.Object.DestroyImmediate(visual);}
var scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Art03_M5_Rig.unity");
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene,scenePath);
var inmate=GameObject.Find("Inmate - M5 static study");
foreach(Transform child in inmate.transform)child.gameObject.SetActive(false);
inmate.AddComponent<PrisonGame.Prototype.InmateMotion>();
PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(folder+"/M5_Behavior.prefab"),inmate.transform);
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
EditorApplication.ExecuteMenuItem("Prison Game/Art/M5 Rig Preview");
return "Created Art03_M5_Behavior with shared motion state, dialogue event hookup, separate presentation and walk-rate controller. Previous study retained.";
