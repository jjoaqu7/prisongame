// Pipeline eval_file in Play mode with Combat01_Sandbox loaded. Run before verify-combat01-step1.cs:
// starts one dummy attack so the second script can confirm Play mode advanced it through every phase.
if(!EditorApplication.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="Combat01_Sandbox")throw new System.Exception("Enter Play mode in Combat01_Sandbox first");
// An unfocused Editor stops game frames; allow them for this check only. Unity resets this on leaving Play,
// and verify-combat01-step1.cs restores it from Player Settings.
Application.runInBackground=true;
var fighter=UnityEngine.Object.FindFirstObjectByType<PrisonGame.Combat.Fighter>();
if(fighter==null)throw new System.Exception("No fighter in the sandbox");
if(!fighter.State.TryStartAttack(new PrisonGame.Combat.AttackTiming(.2f,.1f,.2f)))throw new System.Exception("Dummy could not start an attack from "+fighter.State.State);
return "Started a 0.5 s attack at t="+Time.time.ToString("0.00")+"; wait at least 1 s of Play time, then run verify-combat01-step1.cs.";
