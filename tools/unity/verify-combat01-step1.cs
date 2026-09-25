// Pipeline eval_file in Play mode with Combat01_Sandbox loaded, after verify-combat01-step1-start.cs.
// ENCOUNTER-01 step 1: the scene is set up as planned, Play mode drives the fighter's timeline,
// and the dummy takes, reports and logs damage through down and kill. Writes evidence; changes vanish on exiting Play.
if(!EditorApplication.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="Combat01_Sandbox")throw new System.Exception("Enter Play mode in Combat01_Sandbox first");
var checks=new System.Collections.Generic.List<string>();
void Check(bool ok,string name){checks.Add((ok?"PASS: ":"FAIL: ")+name);if(!ok)throw new System.Exception(string.Join("\n",checks));}
PrisonGame.Combat.FighterDebugLog log=null;
try
{
    var fighters=UnityEngine.Object.FindObjectsByType<PrisonGame.Combat.Fighter>(FindObjectsSortMode.None);
    Check(fighters.Length==1,"exactly one fighter (the dummy) in the sandbox");
    var dummy=fighters[0];log=dummy.GetComponent<PrisonGame.Combat.FighterDebugLog>();
    Check(dummy.Id.Value=="sandbox-dummy","dummy has its stable ID");
    Check(log!=null,"dummy has the debug log");
    var player=UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.FirstPersonController>();
    Check(player!=null&&player.GetComponent<PrisonGame.Prototype.PlayerInteraction>()!=null,"accepted player controller and interaction present");
    var cam=player.GetComponentInChildren<Camera>();
    Check(cam!=null&&Mathf.Abs(cam.fieldOfView-70f)<.01f,"player camera field of view is 70");
    var floor=GameObject.Find("Floor").GetComponent<Collider>().bounds;
    Check(Mathf.Abs(floor.size.x-4.4f)<.01f&&Mathf.Abs(floor.size.z-5.4f)<.01f,"floor spans the 4 x 5 m interior plus walls");
    Check(Vector3.Distance(player.transform.position,dummy.transform.position)>2.5f,"player spawns out of reach of the dummy");

    var cycle=new[]{"Idle -> WindUp","WindUp -> Active","Active -> Recovery","Recovery -> Idle"};
    int at=-1;bool ordered=true;
    foreach(var step in cycle){int next=-1;for(int i=at+1;i<log.Lines.Count;i++)if(log.Lines[i].EndsWith(step)){next=i;break;}if(next<0){ordered=false;break;}at=next;}
    Check(ordered,"Play mode advanced the started attack through wind-up, active, recovery and back to idle, in order");
    Check(dummy.State.State==PrisonGame.Combat.FighterState.Idle,"dummy is idle again after its attack");
    Check(dummy.Health.Current==100f&&dummy.Health.State==PrisonGame.Combat.LifeState.Alive,"dummy starts at 100 health, alive");

    var attacker=new PrisonGame.Core.StableId("sandbox-check");
    PrisonGame.Combat.DamageInfo Hit(float amount)=>new PrisonGame.Combat.DamageInfo(attacker,"fists",PrisonGame.Combat.AttackKind.Quick,amount,dummy.transform.position,Vector3.forward,Time.timeAsDouble);
    for(int i=0;i<9;i++)dummy.ApplyHit(Hit(10f));
    Check(Mathf.Abs(dummy.Health.Current-10f)<.001f&&dummy.Health.IsAlive,"nine 10-point punches leave him at 10 health, still up");
    Check(log.Lines[log.Lines.Count-1].Contains("-10 Quick from sandbox-check (10/100)"),"each hit is logged with amount, kind, attacker and remaining health");
    var result=dummy.ApplyHit(Hit(10f));
    Check(result.WentDown&&dummy.Health.State==PrisonGame.Combat.LifeState.Downed&&dummy.State.State==PrisonGame.Combat.FighterState.Downed,"the tenth punch downs him; he is downed, not dead");
    result=dummy.ApplyHit(Hit(10f));
    Check(result.Ignored&&dummy.Health.Current==0f&&dummy.Health.State==PrisonGame.Combat.LifeState.Downed,"punches on a downed fighter change nothing");
    Check(dummy.Kill(attacker)&&dummy.Health.State==PrisonGame.Combat.LifeState.Dead&&dummy.State.State==PrisonGame.Combat.FighterState.Dead,"the kill action kills a downed fighter");
    Check(!dummy.Kill(attacker),"he can only be killed once");
    dummy.RecoverFully();
    Check(dummy.Health.State==PrisonGame.Combat.LifeState.Dead&&dummy.Health.Current==0f,"recovering does not revive the dead");
    Check(System.Linq.Enumerable.Any(log.Lines,l=>l.Contains("down (last hit by sandbox-check)"))&&System.Linq.Enumerable.Any(log.Lines,l=>l.Contains("killed by sandbox-check")),"log shows the down and the kill with the attacker's ID");
}
finally
{
    Application.runInBackground=PlayerSettings.runInBackground;
    var lines=new System.Collections.Generic.List<string>(checks);
    if(log!=null){lines.Add("");lines.Add("Dummy debug log (latest lines):");lines.AddRange(log.Lines);}
    System.IO.File.WriteAllLines("../docs/evidence/combat01-step1-checks.txt",lines);
}
return string.Join("\n",checks);
