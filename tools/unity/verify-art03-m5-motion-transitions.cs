if(!EditorApplication.isPlaying)throw new System.Exception("Enter Play mode first.");
var skin=UnityEngine.Object.FindFirstObjectByType<SkinnedMeshRenderer>();var animator=skin.GetComponentInParent<Animator>();
var mode=animator.cullingMode;int previous=animator.GetInteger("Preview");var baked=new Mesh();
var clearance=animator.GetComponent<PrisonGame.Prototype.RigFloorClearance>();
if(clearance==null||!clearance.enabled)throw new System.Exception("Missing floor-clearance component");
var gameplay=animator.transform.parent;var originalPosition=gameplay.position;var originalRotation=gameplay.rotation;
var results=new System.Collections.Generic.List<string>();
try{
    animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
    var soles=new System.Collections.Generic.List<int>();
    foreach(string line in System.IO.File.ReadAllLines("../docs/images/art03/m5-rig-parts.txt")){var p=line.Split('|');if(p[0].EndsWith("_Sole")){int start=int.Parse(p[1]),partCount=int.Parse(p[2]);for(int i=start;i<start+partCount;i++)soles.Add(i);}}
    var names=new[]{"Idle","WalkPreview","GesturePreview"};float penetration=0,maxIdleOffset=0,maxCorrection=0;int count=0;
    var rest=skin.sharedMesh.vertices;
    for(int from=0;from<3;from++)for(int to=0;to<3;to++)if(from!=to)foreach(float phase in new[]{0f,.25f,.5f,.75f}){
        animator.Rebind();animator.SetInteger("Preview",from);animator.Play(names[from],0,phase);animator.Update(0);
        animator.SetInteger("Preview",to);
        for(int frame=0;frame<24;frame++){
            animator.Update(1f/120);clearance.ApplyFloorClearance();skin.BakeMesh(baked);var v=baked.vertices;
            maxCorrection=Mathf.Max(maxCorrection,animator.transform.localPosition.y);
            foreach(int i in soles)penetration=Mathf.Max(penetration,rest[i].y-gameplay.InverseTransformPoint(skin.transform.TransformPoint(v[i])).y);
            foreach(var point in v)if(!float.IsFinite(point.x)||!float.IsFinite(point.y)||!float.IsFinite(point.z)||!skin.bounds.Contains(skin.transform.TransformPoint(point)))throw new System.Exception("Invalid transition geometry");count++;
        }
        if(!animator.GetCurrentAnimatorStateInfo(0).IsName(names[to]))throw new System.Exception("Transition did not reach "+names[to]);
        if(to==0){skin.BakeMesh(baked);var v=baked.vertices;foreach(int i in soles)maxIdleOffset=Mathf.Max(maxIdleOffset,Vector3.Distance(gameplay.InverseTransformPoint(skin.transform.TransformPoint(v[i])),rest[i]));}
    }
    results.Add("INFO: maximum sole penetration through crossfades="+(penetration*1000).ToString("F3")+" mm; idle return sole offset="+(maxIdleOffset*1000).ToString("F3")+" mm; largest visual lift="+(maxCorrection*1000).ToString("F3")+" mm");
    if(penetration>.001f)throw new System.Exception("Crossfade sole penetration exceeds 1 mm: "+(penetration*1000).ToString("F3"));
    if(maxIdleOffset>.0001f)throw new System.Exception("Idle did not restore planted feet");
    if(Vector3.Distance(gameplay.position,originalPosition)>.00001f||Quaternion.Angle(gameplay.rotation,originalRotation)>.0001f)throw new System.Exception("Correction moved gameplay root");
    results.Add("PASS: All six directed clip transitions at four starting phases complete; "+count+" sampled poses stay finite and inside rendering bounds.");
    results.Add("PASS: Sole penetration stays below 1 mm during crossfades; returning to idle restores neutral foot positions within 0.1 mm; gameplay root position/rotation unchanged.");
}finally{animator.SetInteger("Preview",previous);animator.Play(previous==1?"WalkPreview":previous==2?"GesturePreview":"Idle",0,0);animator.Update(0);clearance.ApplyFloorClearance();animator.cullingMode=mode;UnityEngine.Object.DestroyImmediate(baked);}
System.IO.File.WriteAllLines("../docs/images/art03/m5-motion-transition-checks.txt",results);return string.Join("\n",results);
