// Play-mode integration: real movement/collision, talk events, animation and world-space soles.
if(!EditorApplication.isPlaying)throw new System.Exception("Enter Play mode first.");
var motion=UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.InmateMotion>();
var inmate=motion.GetComponent<PrisonGame.Prototype.PrototypeInmate>();var root=motion.transform;
var presentation=motion.GetComponentInChildren<PrisonGame.Prototype.InmateAnimation>();var animator=presentation.GetComponent<Animator>();
var clearance=presentation.GetComponent<PrisonGame.Prototype.RigFloorClearance>();var skin=presentation.GetComponentInChildren<SkinnedMeshRenderer>();
var player=GameObject.Find("Player");var interaction=player.GetComponent<PrisonGame.Prototype.PlayerInteraction>();var cc=player.GetComponent<CharacterController>();
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var step=motion.GetType().GetMethod("Step",flags);var talk=motion.GetType().GetField("talkRemaining",flags);
var start=root.position;var rotation=root.rotation;var playerPosition=player.transform.position;var playerRotation=player.transform.rotation;
var mode=animator.cullingMode;var results=new System.Collections.Generic.List<string>();var temporary=new System.Collections.Generic.List<GameObject>();var baked=new Mesh();
void Check(bool value,string message){if(!value)throw new System.Exception("FAIL: "+message);results.Add("PASS: "+message);}
void Tick(float dt){step.Invoke(motion,new object[]{dt});presentation.RefreshAnimation();animator.Update(dt);clearance.ApplyFloorClearance();Physics.SyncTransforms();}
void Reset(Vector3 position){motion.Stop();talk.SetValue(motion,0f);root.SetPositionAndRotation(position,Quaternion.identity);animator.Rebind();animator.SetInteger("Preview",0);animator.Update(0);clearance.ApplyFloorClearance();Physics.SyncTransforms();}
void PlacePlayer(Vector3 position){cc.enabled=false;player.transform.position=position;cc.enabled=true;Physics.SyncTransforms();}
try{
    animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;PlacePlayer(new Vector3(-3.4f,.05f,-.5f));Reset(start);
    Check(!motion.SetDestination(new Vector3(float.NaN,0,0))&&!motion.SetDestination(start+Vector3.up),"Invalid and off-level destinations are rejected");
    Check(motion.SetDestination(start+Vector3.back*1.2f),"Level destination accepted");
    var soles=new System.Collections.Generic.Dictionary<string,Vector2Int>();
    foreach(string line in System.IO.File.ReadAllLines("../docs/images/art03/m5-rig-parts.txt")){var p=line.Split('|');if(p[0].EndsWith("_Sole"))soles[p[0]]=new Vector2Int(int.Parse(p[1]),int.Parse(p[2]));}
    var previous=new Vector3[2];var contacting=new bool[2];float drift=0,penetration=0;int measured=0;bool sawWalk=false;
    for(int frame=0;frame<180;frame++){
        Tick(1f/60);var state=animator.GetCurrentAnimatorStateInfo(0);
        if(!state.IsName("WalkPreview")||animator.IsInTransition(0)||motion.Velocity.magnitude<PrisonGame.Prototype.InmateMotion.WalkSpeed-.001f){contacting[0]=contacting[1]=false;continue;}
        sawWalk=true;skin.BakeMesh(baked);var v=baked.vertices;
        for(int side=0;side<2;side++){
            float phase=Mathf.Repeat(state.normalizedTime+side*.5f,1);var part=soles[side==0?"Left_Sole":"Right_Sole"];Vector3 center=Vector3.zero;
            for(int i=part.x;i<part.x+part.y;i++){var world=skin.transform.TransformPoint(v[i]);penetration=Mathf.Max(penetration,-world.y);center+=world;}center/=part.y;
            bool contact=phase>.035f&&phase<.465f;
            if(contact&&contacting[side]){drift=Mathf.Max(drift,Vector3.Distance(center,previous[side]));measured++;}contacting[side]=contact;previous[side]=center;
        }
    }
    Check(sawWalk&&Vector3.Distance(root.position,start+Vector3.back*1.2f)<.001f&&!motion.HasDestination,"Inmate walks 1.2 m and arrives without overshoot");
    Check(measured>60&&drift<.001f&&penetration<.001f,"Actual traveling soles stay planted within 1 mm per sampled frame and above the floor");
    Tick(.3f);Tick(.3f);Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"),"Arrival returns animation to idle");
    results.Add("INFO: moving contact samples="+measured+"; max per-frame world-space drift="+(drift*1000).ToString("F4")+" mm; penetration="+(penetration*1000).ToString("F4")+" mm");
    foreach(int fps in new[]{30,60,144}){Reset(start);motion.SetDestination(start+Vector3.back*1.2f);for(int i=0;i<fps;i++)Tick(1f/fps);Check(Mathf.Abs(Vector3.Distance(start,root.position)-PrisonGame.Prototype.InmateMotion.WalkSpeed)<.001f,"Matched travel speed at "+fps+" FPS");}

    Reset(start);var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);temporary.Add(wall);wall.transform.position=start+new Vector3(0,1,-.65f);wall.transform.localScale=new Vector3(1,2,.1f);Physics.SyncTransforms();
    motion.SetDestination(start+Vector3.back*1.2f);for(int i=0;i<180;i++)Tick(1f/60);
    Check(motion.IsBlocked&&root.position.z>start.z-.38f&&motion.Velocity==Vector3.zero,"Solid obstacle stops inmate and walking animation input");
    wall.SetActive(false);Physics.SyncTransforms();for(int i=0;i<180;i++)Tick(1f/60);
    Check(!motion.HasDestination&&Vector3.Distance(root.position,start+Vector3.back*1.2f)<.001f,"Removing obstacle resumes the retained destination");
    Reset(start);PlacePlayer(start+new Vector3(0,.05f,-.8f));motion.SetDestination(start+Vector3.back*1.2f);for(int i=0;i<180;i++)Tick(1f/60);
    var body=motion.GetComponent<CapsuleCollider>();Vector3 separation;float overlap;
    bool touching=Physics.ComputePenetration(body,root.position,root.rotation,cc,player.transform.position,player.transform.rotation,out separation,out overlap);
    Check(motion.IsBlocked&&Vector3.Distance(root.position,start)<.5f&&(!touching||overlap<.001f),"Player capsule blocks inmate movement without overlap (travel="+Vector3.Distance(root.position,start).ToString("F3")+", overlap="+overlap.ToString("F5")+")");PlacePlayer(playerPosition);

    var platform=GameObject.CreatePrimitive(PrimitiveType.Cube);temporary.Add(platform);platform.transform.position=new Vector3(30,1.9f,30);platform.transform.localScale=new Vector3(2,.2f,1.2f);Physics.SyncTransforms();
    var ledgeStart=new Vector3(30,2,30);Reset(ledgeStart);motion.SetDestination(ledgeStart+Vector3.back*2);for(int i=0;i<240;i++)Tick(1f/60);
    Check(motion.IsBlocked&&root.position.z>29.6f&&Mathf.Abs(root.position.y-2)<.001f,"Missing floor support stops movement before a ledge");

    Reset(start);motion.SetDestination(start+Vector3.back*1.2f);for(int i=0;i<30;i++)Tick(1f/60);var beforeTalk=root.position;
    int sequence=motion.TalkSequence;PrisonGame.Prototype.PlayerInteraction observed=null;System.Action<PrisonGame.Prototype.PlayerInteraction> listener=actor=>observed=actor;
    inmate.Talked+=listener;try{inmate.Interact(interaction);}finally{inmate.Talked-=listener;}
    Check(observed==interaction&&motion.IsTalking&&motion.TalkSequence==sequence+1,"Talk receives the acting player and starts shared talking state");
    for(int i=0;i<30;i++)Tick(1f/60);
    Check(animator.GetCurrentAnimatorStateInfo(0).IsName("GesturePreview")&&Vector3.Distance(root.position,beforeTalk)<.0001f,"Talking plays gesture and pauses actual travel");
    inmate.Interact(interaction);Tick(.02f);Tick(.2f);
    Check(motion.TalkSequence==sequence+2&&animator.GetCurrentAnimatorStateInfo(0).normalizedTime<.2f,"Repeated talk restarts the gesture");
    for(int i=0;i<180;i++)Tick(1f/60);
    Check(motion.IsTalking&&!motion.IsGesturing&&animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"),"Gesture plays once, then idle holds for the rest of the dialogue");
    Check(Vector3.Distance(root.position,beforeTalk)<.0001f,"Inmate stays in place throughout dialogue hold");
    for(int i=0;i<420;i++)Tick(1f/60);
    Check(!motion.IsTalking&&!motion.HasDestination&&Vector3.Distance(root.position,start+Vector3.back*1.2f)<.001f,"After talking, inmate turns back and completes the interrupted walk");
    Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"),"Completed walk settles to idle");
    Check(motion.GetComponentsInChildren<Collider>().Length==1,"Behavior keeps one gameplay collider");
}finally{
    foreach(var o in temporary)UnityEngine.Object.DestroyImmediate(o);Reset(start);root.rotation=rotation;PlacePlayer(playerPosition);player.transform.rotation=playerRotation;animator.cullingMode=mode;UnityEngine.Object.DestroyImmediate(baked);Physics.SyncTransforms();
}
System.IO.File.WriteAllLines("../docs/images/art03/m5-behavior-checks.txt",results);return string.Join("\n",results);
