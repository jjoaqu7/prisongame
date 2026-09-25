// Check actual deformed soles at 240 Hz, including between the 60 Hz baked keys.
if(EditorApplication.isPlayingOrWillChangePlaymode)throw new System.Exception("Run outside Play mode.");
var preview=UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
var root=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prototype/Art03Rig/M5_Rigged.prefab"));
UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root,preview);var baked=new Mesh();
var results=new System.Collections.Generic.List<string>();
void Check(bool value,string message){if(!value)throw new System.Exception("FAIL: "+message);results.Add("PASS: "+message);}
try{
    root.GetComponent<Animator>().enabled=false;var skin=root.GetComponentInChildren<SkinnedMeshRenderer>();
    var parts=new System.Collections.Generic.Dictionary<string,Vector2Int>();
    foreach(string line in System.IO.File.ReadAllLines("../docs/images/art03/m5-rig-parts.txt")){var p=line.Split('|');parts[p[0]]=new Vector2Int(int.Parse(p[1]),int.Parse(p[2]));}
    skin.BakeMesh(baked);var rest=baked.vertices;float minRest=float.PositiveInfinity;
    foreach(string side in new[]{"Left","Right"}){var p=parts[side+"_Sole"];for(int i=p.x;i<p.x+p.y;i++)minRest=Mathf.Min(minRest,rest[i].y);}
    var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Prototype/Art03Rig/Clips/M5_WalkPreview.anim");
    float maximumPenetration=0,maximumContactHeightError=0,maximumSlip=0,minKnee=180,maxKnee=0;
    float[] peakLift={0,0};int[] contacts={0,0};var previous=new Vector3[2];var wasContact=new bool[2];
    int steps=288;float dt=clip.length/steps;float nominalSpeed=.26f/.6f;
    for(int frame=0;frame<=steps;frame++){
        float time=clip.length*frame/steps;clip.SampleAnimation(root,time);skin.BakeMesh(baked);var v=baked.vertices;
        foreach(var point in v)if(!float.IsFinite(point.x)||!float.IsFinite(point.y)||!float.IsFinite(point.z)||!skin.bounds.Contains(skin.transform.TransformPoint(point)))throw new System.Exception("Invalid or culled sampled geometry");
        for(int sideIndex=0;sideIndex<2;sideIndex++){
            string side=sideIndex==0?"Left":"Right";float phase=Mathf.Repeat((float)frame/steps+sideIndex*.5f,1);
            var part=parts[side+"_Sole"];float min=float.PositiveInfinity;var center=Vector3.zero;
            for(int i=part.x;i<part.x+part.y;i++){min=Mathf.Min(min,v[i].y);center+=v[i];}center/=part.y;
            float height=min-minRest;peakLift[sideIndex]=Mathf.Max(peakLift[sideIndex],height);maximumPenetration=Mathf.Max(maximumPenetration,-height);
            // Exclude phase boundaries when measuring within a single stance interval.
            bool contact=phase>.015f&&phase<.485f;
            var worldCenter=center+Vector3.back*nominalSpeed*time;
            if(contact){contacts[sideIndex]++;maximumContactHeightError=Mathf.Max(maximumContactHeightError,Mathf.Abs(height));if(wasContact[sideIndex])maximumSlip=Mathf.Max(maximumSlip,Vector3.Distance(worldCenter,previous[sideIndex]));}
            wasContact[sideIndex]=contact;previous[sideIndex]=worldCenter;
            Transform hip=null,knee=null,foot=null;foreach(var bone in skin.bones){if(bone.name==side+"Thigh")hip=bone;if(bone.name==side+"Shin")knee=bone;if(bone.name==side+"Foot")foot=bone;}
            float bend=180-Vector3.Angle(hip.position-knee.position,foot.position-knee.position);minKnee=Mathf.Min(minKnee,bend);maxKnee=Mathf.Max(maxKnee,bend);
        }
    }
    Check(maximumPenetration<.001f,"289 walk samples keep deformed soles within 1 mm of the neutral floor or above");
    Check(maximumContactHeightError<.001f&&contacts[0]>100&&contacts[1]>100,"Both feet hold level contact throughout sampled stance intervals");
    Check(maximumSlip<.0001f,"Contact drift below 0.1 mm per 240 Hz sample with matched 0.433333 m/s virtual forward travel");
    Check(peakLift[0]>.06f&&peakLift[1]>.06f,"Both swinging shoes clear the floor by at least 6 cm");
    Check(minKnee>2&&maxKnee<65,"Walking knees remain bent without locking straight or exceeding 65 degrees");
    results.Add("INFO: max sole penetration="+(maximumPenetration*1000).ToString("F4")+" mm; contact height error="+(maximumContactHeightError*1000).ToString("F4")+" mm; per-sample matched-travel drift="+(maximumSlip*1000).ToString("F4")+" mm");
    results.Add("INFO: swing clearance L/R="+peakLift[0].ToString("F4")+" / "+peakLift[1].ToString("F4")+" m; knee range="+minKnee.ToString("F2")+" to "+maxKnee.ToString("F2")+" degrees; neutral sole height="+minRest.ToString("F5")+" m");
    results.Add("LIMIT: Level-ground clip checks with virtual matched travel; no terrain adaptation, navigation, or actual root movement. In-place preview intentionally moves contact feet backward.");
}finally{UnityEngine.Object.DestroyImmediate(baked);UnityEngine.Object.DestroyImmediate(root);UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(preview);}
System.IO.File.WriteAllLines("../docs/images/art03/m5-foot-contact-checks.txt",results);return string.Join("\n",results);
