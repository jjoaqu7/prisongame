if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)throw new System.Exception("Editor must be idle.");
for(int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)throw new System.Exception("Save pending edits.");
var scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Earn01_SnackPacks.unity");
if(UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.SnackShelf>()!=null)throw new System.Exception("Shelf already installed in scene; edit normally.");
var player=GameObject.Find("Player");var inmate=UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.PrototypeInmate>();inmate.gameObject.AddComponent<PrisonGame.Prototype.SnackPackBuyer>();
var parent=GameObject.Find("EARN-01 - Snack packs").transform;
var shelf=new GameObject("Cell shelf - buy and store");shelf.transform.SetParent(parent);shelf.transform.position=new Vector3(-3.85f,0,-2.65f);
var behavior=shelf.AddComponent<PrisonGame.Prototype.SnackShelf>();
Material Load(string name)=>AssetDatabase.LoadAssetAtPath<Material>("Assets/Prototype/Earn01/"+name+".mat");
GameObject Box(string name,Transform owner,Vector3 position,Vector3 size,Material mat){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(owner,false);g.transform.localPosition=position;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=mat;return g;}
var visual=new GameObject("Purchased shelf");visual.transform.SetParent(shelf.transform,false);
foreach(float y in new[]{1.1f,1.53f})Box("Shelf board",visual.transform,new Vector3(0,y,0),new Vector3(1.12f,.05f,.36f),Load("Supply box"));
foreach(float x in new[]{-.44f,.44f})Box("Wall bracket",visual.transform,new Vector3(x,1.27f,-.15f),new Vector3(.055f,.68f,.055f),Load("Blue paper band"));
var sign=Box("Shelf purchase plaque",shelf.transform,new Vector3(.77f,1.47f,-.20f),new Vector3(.34f,.28f,.055f),Load("Blue paper band"));
var label=new GameObject("Shelf label");label.transform.SetParent(sign.transform,false);label.transform.localPosition=new Vector3(0,0,.56f);label.transform.localRotation=Quaternion.Euler(0,180,0);label.transform.localScale=new Vector3(1/.34f,1/.28f,1/.055f);
var text=label.AddComponent<TextMesh>();text.text="CELL SHELF\n$8";text.fontSize=48;text.characterSize=.012f;text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.color=new Color(.94f,.91f,.80f);
var slots=new System.Collections.Generic.List<Transform>();foreach(float x in new[]{-.28f,.28f}){var slot=new GameObject("Storage point").transform;slot.SetParent(shelf.transform,false);slot.localPosition=new Vector3(x,1.125f,0);slots.Add(slot);}
visual.SetActive(false);var so=new SerializedObject(behavior);so.FindProperty("shelfVisual").objectReferenceValue=visual;var array=so.FindProperty("slots");array.arraySize=slots.Count;for(int i=0;i<slots.Count;i++)array.GetArrayElementAtIndex(i).objectReferenceValue=slots[i];so.ApplyModifiedPropertiesWithoutUndo();
var hud=player.AddComponent<PrisonGame.Prototype.SnackLoopHud>();so=new SerializedObject(hud);so.FindProperty("assembly").objectReferenceValue=UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.SnackAssembly>();so.FindProperty("shelf").objectReferenceValue=behavior;so.ApplyModifiedPropertiesWithoutUndo();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
return "Saved complete snack-loop scene: M5 buyer, paid restock and recovery, objective HUD, purchasable two-slot physical cell shelf. Prices: sell $3, restock $1, shelf $8.";
