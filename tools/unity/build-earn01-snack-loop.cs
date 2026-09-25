// Build the new earning study from the reviewed M5 behavior scene.
if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)throw new System.Exception("Editor must be idle.");
for(int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)throw new System.Exception("Save pending edits.");
const string folder="Assets/Prototype/Earn01";const string destination="Assets/Scenes/Earn01_SnackPacks.unity";
if(AssetDatabase.IsValidFolder(folder)||System.IO.File.Exists(destination))throw new System.Exception("Earning study exists; edit saved assets.");
AssetDatabase.CreateFolder("Assets/Prototype","Earn01");
Material Mat(string name,Color color){var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.name=name;m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",.2f);AssetDatabase.CreateAsset(m,folder+"/"+name+".mat");return m;}
var paper=Mat("Cream wrapper",new Color(.83f,.78f,.62f));var band=Mat("Blue paper band",new Color(.26f,.42f,.47f));var cracker=Mat("Cracker packet",new Color(.66f,.43f,.22f));var fruit=Mat("Fruit packet",new Color(.49f,.26f,.20f));var trayMat=Mat("Assembly tray",new Color(.34f,.43f,.41f));var crateMat=Mat("Supply box",new Color(.41f,.33f,.24f));
GameObject Shape(string name,Transform parent,Vector3 position,Vector3 scale,Material material,bool collider=false){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=position;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=material;if(!collider)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
var pack=Shape("Snack pack",null,Vector3.zero,new Vector3(.24f,.10f,.18f),paper,true);
var rb=pack.AddComponent<Rigidbody>();rb.mass=.15f;rb.isKinematic=true;rb.useGravity=true;
var pickup=pack.AddComponent<PrisonGame.Prototype.PrototypePickup>();var so=new SerializedObject(pickup);so.FindProperty("itemName").stringValue="Snack pack";so.ApplyModifiedPropertiesWithoutUndo();pack.AddComponent<PrisonGame.Prototype.SnackPackItem>();
Shape("Paper band",pack.transform,new Vector3(0,.01f,0),new Vector3(.25f,1.02f,1.02f),band);
var packPrefab=PrefabUtility.SaveAsPrefabAsset(pack,folder+"/SnackPack.prefab");UnityEngine.Object.DestroyImmediate(pack);
var scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Art03_M5_Behavior.unity");UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene,destination);
var root=new GameObject("EARN-01 - Snack packs");
var player=GameObject.Find("Player");player.AddComponent<PrisonGame.Prototype.SnackSupplies>();
var station=new GameObject("Snack assembly tray");station.transform.SetParent(root.transform);station.transform.position=new Vector3(3.65f,.87f,3.8f);
var sc=station.AddComponent<BoxCollider>();sc.size=new Vector3(.58f,.1f,.55f);
Shape("Tray base",station.transform,Vector3.zero,new Vector3(.58f,.04f,.55f),trayMat);
var crackersVisual=Shape("Crackers on tray",station.transform,new Vector3(-.13f,.055f,0),new Vector3(.2f,.07f,.24f),cracker);crackersVisual.SetActive(false);
var fruitVisual=Shape("Fruit on tray",station.transform,new Vector3(.13f,.055f,0),new Vector3(.18f,.07f,.2f),fruit);fruitVisual.SetActive(false);
var assembly=station.AddComponent<PrisonGame.Prototype.SnackAssembly>();so=new SerializedObject(assembly);so.FindProperty("packPrefab").objectReferenceValue=packPrefab;so.FindProperty("crackersVisual").objectReferenceValue=crackersVisual;so.FindProperty("fruitVisual").objectReferenceValue=fruitVisual;so.ApplyModifiedPropertiesWithoutUndo();
var box=Shape("Snack supply box",root.transform,new Vector3(4.45f,1f,3.8f),new Vector3(.55f,.32f,.5f),crateMat,true);box.AddComponent<PrisonGame.Prototype.SnackSupplyBox>();
// Labels face the open west end of the table, beside the inmate's clear route.
void Label(string text,Vector3 position){var g=new GameObject(text);g.transform.SetParent(root.transform);g.transform.position=position;g.transform.rotation=Quaternion.Euler(0,90,0);var tm=g.AddComponent<TextMesh>();tm.text=text;tm.fontSize=48;tm.characterSize=.018f;tm.anchor=TextAnchor.MiddleCenter;tm.alignment=TextAlignment.Center;tm.color=new Color(.93f,.91f,.78f);}
Label("ASSEMBLY",new Vector3(3.34f,1.12f,3.8f));Label("SUPPLIES",new Vector3(4.15f,1.23f,3.8f));
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
return "Created Earn01_SnackPacks with starter supplies, three-step assembly, physical carryable pack and table props. Sale/restock/upgrade integration follows the exchange-method decision.";
