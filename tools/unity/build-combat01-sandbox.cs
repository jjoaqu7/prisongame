// Pipeline eval_file, outside Play mode. Creates the ENCOUNTER-01 greybox fight room; never overwrites.
// Room interior matches the neighboring cell (4 x 5 m). The player is copied from Room01 so
// movement settings match the accepted controller; Room01 itself is not modified.
const string scenePath="Assets/Scenes/Combat01_Sandbox.unity";
if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)throw new System.Exception("Editor must be idle and outside Play mode");
if(System.IO.File.Exists(scenePath))throw new System.Exception("Sandbox already exists; edit it in Unity instead of rebuilding.");
for(int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)throw new System.Exception("Save pending scene edits first.");
string previous=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
Material Mat(string n){var m=AssetDatabase.LoadAssetAtPath<Material>("Assets/Prototype/Materials/"+n+".mat");if(m==null)throw new System.Exception("Missing material "+n);return m;}
var floorMat=Mat("Floor_Cell");var lowerMat=Mat("Institutional_Green");var upperMat=Mat("Concrete_Upper");var metal=Mat("Dark_Metal");var mattress=Mat("Placeholder_Mattress");var ochre=Mat("Doorway_Ochre");

var scene=UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,UnityEditor.SceneManagement.NewSceneMode.Single);
var room=new GameObject("Greybox cell 4x5 (matches neighboring cell)");
GameObject Box(string n,Vector3 center,Vector3 size,Material m,Transform parent){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(parent,false);g.transform.position=center;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=m;g.isStatic=true;return g;}
const float w=4f,d=5f,h=2.8f,t=.2f,split=1.2f;
Box("Floor",new Vector3(0,-.05f,0),new Vector3(w+2*t,.1f,d+2*t),floorMat,room.transform);
Box("Ceiling",new Vector3(0,h+.05f,0),new Vector3(w+2*t,.1f,d+2*t),upperMat,room.transform);
foreach(var (side,c,s) in new[]{("North",new Vector3(0,0,d/2+t/2),new Vector3(w+2*t,1,t)),("South",new Vector3(0,0,-d/2-t/2),new Vector3(w+2*t,1,t)),("East",new Vector3(w/2+t/2,0,0),new Vector3(t,1,d)),("West",new Vector3(-w/2-t/2,0,0),new Vector3(t,1,d))})
{
    Box(side+" wall lower",new Vector3(c.x,split/2,c.z),new Vector3(s.x,split,s.z),lowerMat,room.transform);
    Box(side+" wall upper",new Vector3(c.x,split+(h-split)/2,c.z),new Vector3(s.x,h-split,s.z),upperMat,room.transform);
}
// Bunk against the west wall, where the real cell has one; later ragdoll tests need it.
var bunk=new GameObject("Bunk");bunk.transform.SetParent(room.transform,false);
Box("Bunk frame",new Vector3(-1.55f,.2f,1.2f),new Vector3(.9f,.4f,2f),metal,bunk.transform);
Box("Bunk mattress",new Vector3(-1.55f,.47f,1.2f),new Vector3(.85f,.14f,1.95f),mattress,bunk.transform);

var sun=new GameObject("Directional light").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=.35f;sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(55,-25,0);
var lamp=new GameObject("Ceiling light").AddComponent<Light>();lamp.type=LightType.Point;lamp.range=7f;lamp.intensity=2.2f;lamp.color=new Color(1f,.86f,.68f);lamp.shadows=LightShadows.Soft;lamp.transform.position=new Vector3(0,h-.25f,0);
RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.42f,.42f,.45f);

// Copy the accepted first-person player.
var source=UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Room01_Blockout.unity",UnityEditor.SceneManagement.OpenSceneMode.Additive);
var sourcePlayer=source.GetRootGameObjects().FirstOrDefault(g=>g.name=="Player");
if(sourcePlayer==null)throw new System.Exception("Room01 Player not found");
float playerHeight=sourcePlayer.transform.position.y;
var player=UnityEngine.Object.Instantiate(sourcePlayer);player.name="Player";
UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(player,scene);
UnityEditor.SceneManagement.EditorSceneManager.CloseScene(source,true);
player.transform.SetPositionAndRotation(new Vector3(0,playerHeight,-1.9f),Quaternion.identity);
var cam=player.GetComponentInChildren<Camera>();if(cam==null)throw new System.Exception("Player camera missing");
cam.fieldOfView=70f;cam.gameObject.tag="MainCamera";
if(player.GetComponent<PrisonGame.Prototype.FirstPersonController>()==null)throw new System.Exception("Player controller missing");

// Capsule dummy: 1.8 m tall, facing the player (-Z), with the combat rules component.
var dummy=GameObject.CreatePrimitive(PrimitiveType.Capsule);dummy.name="Sandbox dummy";
dummy.transform.SetPositionAndRotation(new Vector3(.4f,.9f,1.3f),Quaternion.Euler(0,180,0));dummy.transform.localScale=new Vector3(.8f,.9f,.8f);
dummy.GetComponent<Renderer>().sharedMaterial=ochre;
var nose=GameObject.CreatePrimitive(PrimitiveType.Cube);nose.name="Facing marker";UnityEngine.Object.DestroyImmediate(nose.GetComponent<Collider>());
nose.transform.SetParent(dummy.transform,false);nose.transform.localPosition=new Vector3(0,.55f,.5f);nose.transform.localScale=new Vector3(.25f,.12f,.2f);nose.GetComponent<Renderer>().sharedMaterial=metal;
var fighter=dummy.AddComponent<PrisonGame.Combat.Fighter>();
var so=new SerializedObject(fighter);so.FindProperty("stableId").stringValue="sandbox-dummy";so.FindProperty("maxHealth").floatValue=100f;so.ApplyModifiedPropertiesWithoutUndo();
dummy.AddComponent<PrisonGame.Combat.FighterDebugLog>();

UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene,scenePath);
if(!string.IsNullOrEmpty(previous))UnityEditor.SceneManagement.EditorSceneManager.OpenScene(previous,UnityEditor.SceneManagement.OpenSceneMode.Single);
return "Created "+scenePath+": 4x5 m greybox cell, bunk, Room01 player at FOV 70, dummy Fighter 'sandbox-dummy' (100 health). Reopened "+previous+".";
