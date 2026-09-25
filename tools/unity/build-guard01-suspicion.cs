if(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) throw new System.Exception("Editor must be idle.");
for(int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++) if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty) throw new System.Exception("Save pending edits first.");
const string path="Assets/Scenes/Guard01_Suspicion.unity",folder="Assets/Prototype/Guard01";
if(System.IO.File.Exists(path)||AssetDatabase.IsValidFolder(folder)) throw new System.Exception("Guard study exists; edit saved assets.");
var scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Earn03_Deadline.unity");
AssetDatabase.CreateFolder("Assets/Prototype","Guard01");
Material Mat(string name,Color color){var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.name=name;m.color=color;m.SetFloat("_Smoothness",.15f);AssetDatabase.CreateAsset(m,folder+"/"+name+".mat");return m;}
var navy=Mat("Guard navy",new Color(.12f,.18f,.23f));var mark=Mat("Staff boundary",new Color(.85f,.62f,.25f));
GameObject Box(string name,Transform parent,Vector3 pos,Vector3 size,Material mat,bool solid){var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=name;o.transform.SetParent(parent,false);o.transform.localPosition=pos;o.transform.localScale=size;o.GetComponent<Renderer>().sharedMaterial=mat;if(!solid)UnityEngine.Object.DestroyImmediate(o.GetComponent<Collider>());return o;}
var root=new GameObject("GUARD-01 - Staff corner");
var zone=new GameObject("Staff-only corner");zone.transform.SetParent(root.transform);zone.transform.position=new Vector3(6.8f,0,6.1f);
var zoneBox=zone.AddComponent<BoxCollider>();zoneBox.isTrigger=true;zoneBox.center=new Vector3(0,1.1f,0);zoneBox.size=new Vector3(1.8f,2.2f,1.5f);var area=zone.AddComponent<PrisonGame.Prototype.RestrictedArea>();
Box("Staff-only front line",zone.transform,new Vector3(0,.008f,-.75f),new Vector3(1.8f,.01f,.07f),mark,false);
foreach(float x in new[]{-.9f,.9f})Box("Staff-only side line",zone.transform,new Vector3(x,.008f,0),new Vector3(.07f,.01f,1.5f),mark,false);
Box("Staff-only back line",zone.transform,new Vector3(0,.008f,.75f),new Vector3(1.8f,.01f,.07f),mark,false);
var plaque=Box("Staff-only wall sign",root.transform,new Vector3(6.8f,2.35f,6.78f),new Vector3(1.5f,.42f,.06f),navy,false);
var label=new GameObject("Staff-only sign text");label.transform.SetParent(root.transform);label.transform.position=new Vector3(6.8f,2.35f,6.74f);
var text=label.AddComponent<TextMesh>();text.text="STAFF ONLY\nOFFICER HARRIS";text.fontSize=48;text.characterSize=.022f;text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.color=new Color(.95f,.89f,.72f);
var source=GameObject.Find("Inmate - M5 static study");var guard=new GameObject("Officer Harris - temporary M5-based guard");guard.transform.SetParent(root.transform);guard.transform.SetPositionAndRotation(new Vector3(6.8f,0,6.48f),Quaternion.Euler(0,180,0));
var sourceBody=source.GetComponent<CapsuleCollider>();var body=guard.AddComponent<CapsuleCollider>();body.center=sourceBody.center;body.height=sourceBody.height;body.radius=sourceBody.radius;
var rig=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prototype/Art03Rig/M5_Rigged.prefab"),guard.transform);
// The M5 mesh faces local -Z; the observer uses conventional local +Z.
rig.transform.localRotation=Quaternion.Euler(0,180,0);
foreach(var renderer in rig.GetComponentsInChildren<SkinnedMeshRenderer>()){var mats=renderer.sharedMaterials;for(int i=0;i<mats.Length;i++)if(mats[i].name=="M5_Ochre"||mats[i].name=="M5_BlueGrey")mats[i]=navy;renderer.sharedMaterials=mats;}
Box("Guard cap",guard.transform,new Vector3(0,1.965f,0),new Vector3(.34f,.065f,.30f),navy,false);
Box("Guard cap brim",guard.transform,new Vector3(0,1.93f,.16f),new Vector3(.34f,.025f,.16f),navy,false);
Box("Guard badge",guard.transform,new Vector3(-.09f,1.15f,.15f),new Vector3(.065f,.08f,.025f),mark,false);
var head=rig.GetComponentsInChildren<Transform>().First(t=>t.name=="Head");
guard.transform.Find("Guard cap").SetParent(head,true);guard.transform.Find("Guard cap brim").SetParent(head,true);
var eyes=new GameObject("Guard eyes").transform;eyes.SetParent(guard.transform,false);eyes.localPosition=new Vector3(0,1.72f,.10f);
var observer=guard.AddComponent<PrisonGame.Prototype.GuardObserver>();var so=new SerializedObject(observer);so.FindProperty("area").objectReferenceValue=area;so.FindProperty("eyes").objectReferenceValue=eyes;so.ApplyModifiedPropertiesWithoutUndo();
var player=GameObject.Find("Player");var suspicion=player.AddComponent<PrisonGame.Prototype.GuardSuspicion>();so=new SerializedObject(suspicion);so.FindProperty("observer").objectReferenceValue=observer;so.FindProperty("clock").objectReferenceValue=UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.PrisonClock>();so.ApplyModifiedPropertiesWithoutUndo();player.AddComponent<PrisonGame.Prototype.GuardSuspicionHud>();
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene,path);AssetDatabase.SaveAssets();return "Saved Guard01_Suspicion: marked staff corner, temporary navy M5-based guard, sight-based personal suspicion and separate HUD. Deadline and earning loop retained.";
