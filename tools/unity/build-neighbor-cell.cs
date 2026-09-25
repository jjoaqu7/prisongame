if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)throw new System.Exception("Editor must be idle");
var scene=UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
if(scene.name!="Duty01_Laundry"||scene.isDirty)throw new System.Exception("Open saved Duty01_Laundry first");
if(GameObject.Find("Neighboring cell module")!=null)throw new System.Exception("Neighboring cell already exists");
string[] names={"Cell floor 4x5","Cell ceiling","Cell west lower","Cell west upper","Cell north lower","Cell north upper","Cell doorway north lower","Cell doorway north upper","Cell doorway south lower","Cell doorway south upper","Cell door lintel","Cell sliding door","Hall west north end lower","Hall west north end upper"};
var sources=names.ToDictionary(n=>n,n=>GameObject.Find(n));
if(sources.Any(p=>p.Value==null))throw new System.Exception("Missing geometry source");
var all=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).ToArray();
var root=new GameObject("Neighboring cell module");root.transform.position=new Vector3(-4,0,4.5f);
GameObject Copy(GameObject source){var clone=UnityEngine.Object.Instantiate(source,root.transform);clone.name="Neighbor "+source.name;clone.transform.SetPositionAndRotation(source.transform.position+Vector3.forward*5,source.transform.rotation);return clone;}
foreach(var name in names.Where(n=>n.StartsWith("Cell")&&n!="Cell sliding door"))Copy(sources[name]);
foreach(var t in all.Where(t=>t.position.x< -2 && t.position.z<2 && t.position.z> -3 && (t.name.StartsWith("Bunk")||t.name=="Cell stool"||t.name.StartsWith("Desk")||t.name.StartsWith("Cell desk")||t.name.StartsWith("Cell warm ceiling")||t.name=="Folded blanket"||t.name=="Pillow"||t.name=="Book on desk")))Copy(t.gameObject);
var door=Copy(sources["Cell sliding door"]).GetComponent<PrisonGame.Prototype.PrototypeDoor>();
var serialized=new SerializedObject(door);serialized.FindProperty("requiresOfficerKey").boolValue=true;serialized.ApplyModifiedPropertiesWithoutUndo();
foreach(var source in all.Where(t=>t.name=="Cell door button"))
{var button=Copy(source.gameObject).GetComponent<PrisonGame.Prototype.PrototypeDoorControl>();serialized=new SerializedObject(button);serialized.FindProperty("door").objectReferenceValue=door;serialized.ApplyModifiedPropertiesWithoutUndo();}
// Replace only the corridor wall covering this new doorway; player-cell shared wall stays intact.
UnityEngine.Object.DestroyImmediate(sources["Hall west north end lower"]);UnityEngine.Object.DestroyImmediate(sources["Hall west north end upper"]);
var sign=new GameObject("Neighbor cell sign");sign.SetActive(false);sign.transform.SetParent(root.transform);sign.transform.SetPositionAndRotation(new Vector3(-1.87f,2.66f,4.5f),Quaternion.Euler(0,-90,0));
var text=sign.AddComponent<TextMesh>();text.text="NEIGHBORING CELL";text.fontSize=48;text.characterSize=.015f;text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.color=new Color(.93f,.90f,.76f);
var depth=sign.AddComponent<PrisonGame.Prototype.WorldLabelDepth>();serialized=new SerializedObject(depth);serialized.FindProperty("labelShader").objectReferenceValue=AssetDatabase.LoadAssetAtPath<Shader>("Assets/Prototype/Scripts/WorldLabel.shader");serialized.ApplyModifiedPropertiesWithoutUndo();sign.SetActive(true);
const string folder="Assets/Prototype/NeighborCell";if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/Prototype","NeighborCell");
PrefabUtility.SaveAsPrefabAssetAndConnect(root,folder+"/NeighboringCell.prefab",InteractionMode.AutomatedAction);
serialized=new SerializedObject(UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.SampleSaveGame>());serialized.FindProperty("neighboringCell").objectReferenceValue=door;serialized.ApplyModifiedPropertiesWithoutUndo();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
return "Added neighboring cell x=-6..-2 z=2..7; Harris-key door at (-2,1.1,4.5); separate backward-compatible save field.";
