// Run via Unity Pipeline eval_file while the Editor is idle.
// Creates a new scene only; refuses to overwrite an existing prototype.

var scenePath = "Assets/Scenes/Room01_Blockout.unity";
if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
    throw new System.Exception("Run while the Editor is idle and outside Play mode.");
if (System.IO.File.Exists(scenePath))
    throw new System.Exception("Prototype already exists. Edit it in Unity; do not overwrite it.");
for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
    if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
        throw new System.Exception("Save your existing scene changes first.");
var shader = Shader.Find("Universal Render Pipeline/Lit");
if (shader == null) throw new System.Exception("URP Lit shader is required.");
if (!AssetDatabase.IsValidFolder("Assets/Prototype")) AssetDatabase.CreateFolder("Assets", "Prototype");
if (!AssetDatabase.IsValidFolder("Assets/Prototype/Materials")) AssetDatabase.CreateFolder("Assets/Prototype", "Materials");
var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects, UnityEditor.SceneManagement.NewSceneMode.Single);
var root = new GameObject("ROOM-01 - Temporary layout");
GameObject Group(string name) { var g = new GameObject(name); g.transform.SetParent(root.transform); return g; }
var shell = Group("Architecture");
var props = Group("Scale references - static placeholders");
var markers = Group("Markers");
Material Mat(string name, Color color) {
    var path = "Assets/Prototype/Materials/" + name + ".mat";
    var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
    if (existing != null) return existing;
    var m = new Material(shader); m.name = name; m.SetColor("_BaseColor", color);
    m.SetFloat("_Smoothness", .15f); AssetDatabase.CreateAsset(m, path); return m;
}
var wall = Mat("Concrete_Upper", new Color(.64f,.65f,.61f));
var baseWall = Mat("Institutional_Green", new Color(.27f,.38f,.36f));
var floorCell = Mat("Floor_Cell", new Color(.47f,.46f,.40f));
var floorHall = Mat("Floor_Corridor", new Color(.34f,.38f,.39f));
var floorCommon = Mat("Floor_Common", new Color(.48f,.51f,.47f));
var metal = Mat("Dark_Metal", new Color(.16f,.21f,.23f));
var wood = Mat("Placeholder_Wood", new Color(.49f,.34f,.21f));
var cloth = Mat("Placeholder_Mattress", new Color(.34f,.43f,.48f));
var accent = Mat("Doorway_Ochre", new Color(.78f,.57f,.24f));
GameObject Box(string name, Vector3 position, Vector3 size, Material material, GameObject parent) {
    var g = GameObject.CreatePrimitive(PrimitiveType.Cube); g.name = name;
    g.transform.SetParent(parent.transform); g.transform.position=position; g.transform.localScale=size;
    g.GetComponent<Renderer>().sharedMaterial=material; g.isStatic=true; return g;
}
void Wall(string name, float x, float z, float sx, float sz) {
    Box(name+" lower",new Vector3(x,.6f,z),new Vector3(sx,1.2f,sz),baseWall,shell);
    Box(name+" upper",new Vector3(x,2.2f,z),new Vector3(sx,2f,sz),wall,shell);
}
// Coordinates are wall centre lines. Floor top is y=0; wall top is y=3.2.
Box("Cell floor 4x5",new Vector3(-4,-.1f,-.5f),new Vector3(4,.2f,5),floorCell,shell);
Box("Corridor floor 3x14",new Vector3(-.5f,-.1f,0),new Vector3(3,.2f,14),floorHall,shell);
Box("Common floor 7x7",new Vector3(4.5f,-.1f,3.5f),new Vector3(7,.2f,7),floorCommon,shell);
Wall("Cell west",-6,-.5f,.2f,5.2f);
Wall("Cell south",-4,-3,4,.2f); Wall("Cell north",-4,2,4,.2f);
Wall("Hall west south end",-2,-5,.2f,4);
Wall("Cell doorway south",-2,-2.1f,.2f,1.8f);
Wall("Cell doorway north",-2,1.1f,.2f,1.8f);
Wall("Hall west north end",-2,4.5f,.2f,5);
// Cell doorway z=-1.2..0.2, clear width 1.4m, height 2.3m.
Box("Cell door lintel",new Vector3(-2,2.75f,-.5f),new Vector3(.2f,.9f,1.4f),accent,shell);
Wall("Hall south end",-.5f,-7,3,.2f); Wall("Hall north end",-.5f,7,3,.2f);
Wall("Hall east south end",1,-3.5f,.2f,7);
Wall("Common entry south",1,.4f,.2f,.8f);
Wall("Common entry north",1,5.1f,.2f,3.8f);
// Common doorway z=0.8..3.2, clear width 2.4m, height 2.5m.
Box("Common entry lintel",new Vector3(1,2.85f,2),new Vector3(.2f,.7f,2.4f),accent,shell);
Wall("Common south",4.5f,0,7,.2f); Wall("Common north",4.5f,7,7,.2f); Wall("Common east",8,3.5f,.2f,7.2f);
// Furniture is intentionally built from plain boxes with colliders.
Box("Bunk frame",new Vector3(-5.15f,.35f,.4f),new Vector3(1.3f,.15f,2.1f),metal,props);
Box("Bunk mattress",new Vector3(-5.15f,.5f,.4f),new Vector3(1.2f,.15f,2),cloth,props);
Box("Bunk upper frame",new Vector3(-5.15f,1.55f,.4f),new Vector3(1.3f,.12f,2.1f),metal,props);
Box("Bunk upper mattress",new Vector3(-5.15f,1.68f,.4f),new Vector3(1.2f,.14f,2),cloth,props);
foreach(float x in new[]{-5.75f,-4.55f}) foreach(float z in new[]{-.6f,1.4f})
    Box("Bunk post",new Vector3(x,.95f,z),new Vector3(.07f,1.9f,.07f),metal,props);
Box("Desk top",new Vector3(-3.1f,.78f,1.5f),new Vector3(1.25f,.1f,.6f),wood,props);
foreach(float x in new[]{-3.6f,-2.6f}) Box("Desk support",new Vector3(x,.36f,1.5f),new Vector3(.08f,.72f,.5f),metal,props);
Box("Cell stool",new Vector3(-3.1f,.24f,.8f),new Vector3(.42f,.48f,.42f),metal,props);
Box("Common table top",new Vector3(4.6f,.78f,3.8f),new Vector3(2.6f,.12f,1.2f),wood,props);
foreach(float x in new[]{3.6f,5.6f}) Box("Table support",new Vector3(x,.36f,3.8f),new Vector3(.12f,.72f,.85f),metal,props);
foreach(float z in new[]{2.6f,5f}) {
    Box("Common bench seat",new Vector3(4.6f,.46f,z),new Vector3(2.6f,.12f,.4f),wood,props);
    foreach(float x in new[]{3.6f,5.6f}) Box("Bench support",new Vector3(x,.2f,z),new Vector3(.12f,.4f,.32f),metal,props);
}
var spawn=new GameObject("PlayerSpawn - feet at floor, eye height 1.7m");
spawn.transform.SetParent(markers.transform); spawn.transform.position=new Vector3(-3.4f,0,-.5f); spawn.transform.rotation=Quaternion.Euler(0,90,0);
var cam=Camera.main; cam.transform.position=new Vector3(12,32,-17); cam.transform.LookAt(new Vector3(.7f,0,0));
cam.orthographic=true; cam.orthographicSize=10.5f; cam.nearClipPlane=.1f; cam.farClipPlane=100;
cam.clearFlags=CameraClearFlags.SolidColor; cam.backgroundColor=new Color(.12f,.16f,.18f);
var light=UnityEngine.Object.FindFirstObjectByType<Light>(); light.transform.rotation=Quaternion.Euler(55,-25,0); light.intensity=1.5f; light.shadows=LightShadows.Soft;
RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat; RenderSettings.ambientLight=new Color(.55f,.59f,.63f);
RenderSettings.fog=false;
AssetDatabase.SaveAssets();
if(!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene,scenePath)) throw new System.Exception("Scene save failed.");
var view=SceneView.lastActiveSceneView;
if(view!=null) { view.LookAt(new Vector3(.7f,0,0),cam.transform.rotation,10.5f,true); view.Repaint(); }
Debug.Log("ROOM-01 created: cell, corridor, common area; provisional dimensions; no movement yet.");
return "Saved " + scenePath + "; " + root.GetComponentsInChildren<BoxCollider>().Length + " box colliders.";
