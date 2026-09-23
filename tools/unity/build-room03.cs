// Pipeline eval_file snippet; apply once to the saved Room01_Blockout scene outside Play mode.
if (EditorApplication.isPlaying || EditorApplication.isCompiling) throw new System.Exception("Wait for edit mode and compilation.");
var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if(scene.path!="Assets/Scenes/Room01_Blockout.unity" || scene.isDirty) throw new System.Exception("Open and save Room01_Blockout first.");
if(GameObject.Find("ROOM-03 - Interactions")!=null) throw new System.Exception("Interactions already exist; edit them normally.");
var root=new GameObject("ROOM-03 - Interactions");
Material Load(string name) => AssetDatabase.LoadAssetAtPath<Material>("Assets/Prototype/Materials/"+name+".mat");
var metal=Load("Dark_Metal");var parcelMat=Load("Placeholder_Wood");var accent=Load("Doorway_Ochre");
GameObject Shape(string name,PrimitiveType kind,Transform parent,Vector3 position,Vector3 scale,Material material) {
    var g=GameObject.CreatePrimitive(kind);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=position;g.transform.localScale=scale;
    g.GetComponent<Renderer>().sharedMaterial=material;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;
}
var door=new GameObject("Cell sliding door");door.transform.SetParent(root.transform);door.transform.position=new Vector3(-2,1.1f,-.5f);
var dc=door.AddComponent<BoxCollider>();dc.size=new Vector3(.12f,2.2f,1.32f);
door.AddComponent<PrisonGame.Prototype.PrototypeDoor>();
foreach(float x in new[]{-2.18f,-1.82f}) {
    var button=GameObject.CreatePrimitive(PrimitiveType.Cube);button.name="Cell door button";button.transform.SetParent(root.transform);
    button.transform.position=new Vector3(x,1.35f,.45f);button.transform.localScale=new Vector3(.12f,.24f,.2f);button.GetComponent<Renderer>().sharedMaterial=accent;
    var control=button.AddComponent<PrisonGame.Prototype.PrototypeDoorControl>();var so=new SerializedObject(control);so.FindProperty("door").objectReferenceValue=door.GetComponent<PrisonGame.Prototype.PrototypeDoor>();so.ApplyModifiedProperties();
}
foreach(float z in new[]{-.63f,-.42f,-.21f,0,.21f,.42f,.63f}) Shape("Door bar",PrimitiveType.Cube,door.transform,new Vector3(0,0,z),new Vector3(.1f,2.2f,.045f),metal);
foreach(float y in new[]{-1.06f,.2f,1.06f})Shape("Door crossbar",PrimitiveType.Cube,door.transform,new Vector3(0,y,0),new Vector3(.12f,.075f,1.32f),metal);
Shape("Door handle",PrimitiveType.Cube,door.transform,new Vector3(-.13f,.03f,-.43f),new Vector3(.13f,.18f,.08f),accent);

var parcel=GameObject.CreatePrimitive(PrimitiveType.Cube);parcel.name="Parcel - pick up with E";parcel.transform.SetParent(root.transform);
parcel.transform.position=new Vector3(-3.1f,.94f,1.5f);parcel.transform.localScale=new Vector3(.34f,.2f,.24f);
parcel.GetComponent<Renderer>().sharedMaterial=parcelMat;
var body=parcel.AddComponent<Rigidbody>();body.mass=.4f;body.isKinematic=true;body.useGravity=true;
parcel.AddComponent<PrisonGame.Prototype.PrototypePickup>();
Shape("Parcel band",PrimitiveType.Cube,parcel.transform,new Vector3(0,.01f,0),new Vector3(.2f,1.02f,1.02f),accent);

var inmate=new GameObject("Inmate - temporary character");inmate.transform.SetParent(root.transform);inmate.transform.position=new Vector3(2.4f,0,5.4f);
var capsule=inmate.AddComponent<CapsuleCollider>();capsule.height=1.8f;capsule.radius=.3f;capsule.center=new Vector3(0,.9f,0);
inmate.AddComponent<PrisonGame.Prototype.PrototypeInmate>();
Shape("Uniform",PrimitiveType.Capsule,inmate.transform,new Vector3(0,1,0),new Vector3(.6f,.55f,.45f),accent);
Shape("Head",PrimitiveType.Sphere,inmate.transform,new Vector3(0,1.63f,0),Vector3.one*.4f,Load("Concrete_Upper"));
foreach(float x in new[]{-.17f,.17f})Shape("Boot",PrimitiveType.Cube,inmate.transform,new Vector3(x,.18f,-.03f),new Vector3(.22f,.36f,.32f),metal);
var player=GameObject.Find("Player");player.layer=LayerMask.NameToLayer("Ignore Raycast");
player.AddComponent<PrisonGame.Prototype.PlayerInteraction>();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
return "Added cell door, parcel, inmate, and player interaction. Existing room dimensions retained.";
