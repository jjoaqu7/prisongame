if(EditorApplication.isPlayingOrWillChangePlaymode)throw new System.Exception("Stop Play first.");
var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.name!="Save01_Progress" || scene.isDirty)throw new System.Exception("Open clean Save01 first.");
const string folder="Assets/Prototype/Audio01/";
foreach(var guid in AssetDatabase.FindAssets("t:AudioClip",new[]{folder.TrimEnd('/')}))
{
 var path=AssetDatabase.GUIDToAssetPath(guid);var importer=(AudioImporter)AssetImporter.GetAtPath(path);
 importer.forceToMono=true;var settings=importer.defaultSampleSettings;settings.preloadAudioData=true;settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;settings.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;importer.defaultSampleSettings=settings;importer.SaveAndReimport();
}
var player=GameObject.Find("Player");var audio=player.GetComponent<PrisonGame.Prototype.SampleAudio>();if(audio==null)audio=player.AddComponent<PrisonGame.Prototype.SampleAudio>();
var so=new SerializedObject(audio);var steps=so.FindProperty("footsteps");steps.arraySize=4;for(int i=0;i<4;i++)steps.GetArrayElementAtIndex(i).objectReferenceValue=AssetDatabase.LoadAssetAtPath<AudioClip>(folder+"footstep_"+(i+1)+".wav");
string[] names={"pickup","place","crackers","fruit","wrap","sale","door","shelf","restock"};var cues=so.FindProperty("cues");cues.arraySize=names.Length;for(int i=0;i<names.Length;i++)cues.GetArrayElementAtIndex(i).objectReferenceValue=AssetDatabase.LoadAssetAtPath<AudioClip>(folder+names[i]+".wav");
so.FindProperty("roomTone").objectReferenceValue=AssetDatabase.LoadAssetAtPath<AudioClip>(folder+"room_tone.wav");so.ApplyModifiedPropertiesWithoutUndo();
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();return "Save01 audio configured with four footsteps, nine action cues and room tone; save scene identity retained.";
