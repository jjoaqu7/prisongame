if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)throw new System.Exception("Editor must be idle.");
const string path="Assets/Prototype/Art03Rig/M5_Rigged.prefab";
var root=PrefabUtility.LoadPrefabContents(path);
try{
    var skin=root.GetComponentInChildren<SkinnedMeshRenderer>();
    var clearance=root.GetComponent<PrisonGame.Prototype.RigFloorClearance>();if(clearance==null)clearance=root.AddComponent<PrisonGame.Prototype.RigFloorClearance>();
    var serialized=new SerializedObject(clearance);var vertices=skin.sharedMesh.vertices;
    foreach(string side in new[]{"Left","Right"}){
        Transform foot=null;foreach(var bone in skin.bones)if(bone.name==side+"Foot")foot=bone;
        var bounds=new Bounds();bool first=true;
        foreach(string line in System.IO.File.ReadAllLines("../docs/images/art03/m5-rig-parts.txt")){
            var p=line.Split('|');if(p[0]!=side+"_Sole")continue;int start=int.Parse(p[1]),count=int.Parse(p[2]);
            for(int i=start;i<start+count;i++){
                var point=foot.InverseTransformPoint(skin.transform.TransformPoint(vertices[i]));
                if(first){bounds=new Bounds(point,Vector3.zero);first=false;}else bounds.Encapsulate(point);
            }
        }
        if(first||foot==null)throw new System.Exception("Missing foot/sole");
        string prefix=side.ToLowerInvariant();serialized.FindProperty(prefix+"Foot").objectReferenceValue=foot;serialized.FindProperty(prefix+"SoleBounds").boundsValue=bounds;
    }
    serialized.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SaveAsPrefabAsset(root,path);AssetDatabase.SaveAssets();
}finally{PrefabUtility.UnloadPrefabContents(root);}
return "Configured two foot-local sole bounds for visual-only floor clearance. Gameplay parent/collider untouched.";
