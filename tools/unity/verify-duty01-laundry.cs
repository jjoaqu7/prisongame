if(!EditorApplication.isPlaying)throw new System.Exception("Enter Duty01 Play");
var p=GameObject.Find("Player");var a=p.GetComponent<PrisonGame.Prototype.PlayerInteraction>();var d=p.GetComponent<PrisonGame.Prototype.LaundryDuty>();var room=d.room;var clock=p.GetComponent<PrisonGame.Prototype.SnackRequest>().Clock;var save=p.GetComponent<PrisonGame.Prototype.SampleSaveGame>();
var result=new System.Collections.Generic.List<string>();void Check(bool ok,string name){if(!ok)throw new System.Exception("FAIL: "+name);result.Add("PASS: "+name);}
var fresh=save.Capture(a);p.GetComponent<PrisonGame.Prototype.SoloClockDriver>().enabled=false;clock.SetPaused(false);Time.timeScale=0;
void Place(Vector3 local){var cc=p.GetComponent<CharacterController>();cc.enabled=false;p.transform.position=room.transform.TransformPoint(local);cc.enabled=true;Physics.SyncTransforms();}
var work=new Vector3(3.7f,.05f,5.4f);var away=new Vector3(4,.05f,3.9f);var supply=new Vector3(11.4f,.05f,4.3f);
void Fresh(){d.Restore(new PrisonGame.Prototype.LaundryDuty.State());clock.SetPaused(false);Place(work);}
Check(save.SavePath.EndsWith("laundry-save-v1.json"),"Laundry uses its own manual slot");
Fresh();Check(!d.Report(null),"Wrong actor cannot start duty");Check(d.Report(a),"Reporting starts assigned duty");Check(!d.Report(a),"Reporting twice cannot reset active shift");
d.Step(a,30);Check(d.Data.worked==30&&d.Data.remaining==90,"Nearby work advances the exact real-second counters");
d.TalkToRue(a);d.Step(a,10);Check(d.Data.favorAsked&&d.Data.worked==40,"Conversation accepts favor while work continues");
clock.SetPaused(true);d.Step(a,30);Check(d.Data.worked==40&&d.Data.remaining==80,"Paused duty does not advance");clock.SetPaused(false);
d.Step(null,5);Check(d.Data.worked==40,"Other actor cannot advance duty");
Place(supply);Check(!room.CanSee(a),"Supply wall blocks guard line of sight");d.Step(a,10);Check(d.Data.worked==40&&d.Data.remaining==70&&!d.Data.warned,"Unseen absence pauses work without erasing it");
Check(d.TakeTool(a)&&d.Data.hasTool&&!room.toolVisual.activeSelf,"Requested tool can be pocketed in the supply room");Check(!d.TakeTool(a),"Tool cannot be collected twice");
Place(work);d.TalkToRue(a);Check(d.Data.favorDone&&!d.Data.hasTool,"Delivery consumes pocketed tool and earns introduction");d.TalkToRue(a);Check(!room.toolVisual.activeSelf,"Repeated dialogue does not respawn delivered tool");d.TalkToDex(a);Check(d.Data.contactMet,"Introduction unlocks second contact");
Fresh();d.TalkToDex(a);Check(!d.Data.contactMet,"Dex contact remains locked before favor");Place(supply);Check(!d.TakeTool(a),"Tool requires the favor first");
Fresh();d.Report(a);Place(away);room.guard.rotation=Quaternion.LookRotation((p.transform.position-room.guard.position).normalized);Physics.SyncTransforms();Check(room.CanSee(a),"Guard sees a nearby player in its facing direction");
d.Step(a,.5f);Check(!d.Data.warned,"Brief visibility below threshold does not warn");d.Step(a,.6f);Check(d.Data.warned&&d.Data.grace==8,"Observed absence gives first warning and eight seconds");
Place(work);d.Step(a,.1f);Check(d.Data.grace==0&&d.Data.warned,"Returning clears order but preserves warning history");
Place(away);d.Step(a,1.1f);Check(d.Data.phase==PrisonGame.Prototype.LaundryDuty.Phase.Failed,"Second observed absence fails the shift");
Fresh();d.Report(a);Place(away);d.Step(a,1.1f);d.Step(a,8);Check(d.Data.phase==PrisonGame.Prototype.LaundryDuty.Phase.Failed,"Ignoring warning fails after return deadline");
Check(d.Report(a)&&d.Data.worked==0&&!d.Data.warned,"Board can restart a failed shift");
Fresh();d.Report(a);d.Step(a,60);Check(d.Data.phase==PrisonGame.Prototype.LaundryDuty.Phase.Active&&d.Data.worked==60,"Work quota does not end shift early");d.Step(a,60);Check(d.Data.phase==PrisonGame.Prototype.LaundryDuty.Phase.Completed,"Full shift with quota completes");
Fresh();d.Report(a);Place(supply);d.Step(a,240);Check(d.Data.phase==PrisonGame.Prototype.LaundryDuty.Phase.Failed,"Missing quota fails at shift end");
Fresh();d.Report(a);d.Step(a,35);d.TalkToRue(a);Place(supply);d.TakeTool(a);d.Data.warned=true;room.Advance(10);
clock.SetPaused(true);var saved=save.Capture(a);const string path="Temp/Duty01Checks/roundtrip.json";Check(save.SaveTo(a,path),"Active duty saves to isolated fixture");
d.Restore(new PrisonGame.Prototype.LaundryDuty.State());room.Advance(12);Check(save.LoadFrom(a,path),"Duty fixture loads");Check(d.Data.worked==35&&d.Data.warned&&d.Data.hasTool&&d.Data.favorAsked&&!d.Data.favorDone,"Load restores progress, warning and pocketed tool");Check(room.Capture().segment==saved.world.laundryRoom.segment&&room.Capture().elapsed==saved.world.laundryRoom.elapsed,"Load restores guard attention instead of resetting it");Check(clock.Paused&&Time.timeScale==0,"Load remains paused");
var bad=JsonUtility.FromJson<PrisonGame.Prototype.SampleSaveData>(JsonUtility.ToJson(saved));bad.player.laundry.worked=121;bool rejected=false;try{save.Restore(a,bad);}catch(System.IO.InvalidDataException){rejected=true;}Check(rejected&&d.Data.worked==35,"Invalid duty save rejected without replacing state");
var legacy=d.Capture();legacy.worked=120;legacy.remaining=200;Check(PrisonGame.Prototype.LaundryDuty.Valid(legacy),"Original two-minute work progress remains loadable");d.Restore(legacy);Check(d.Data.worked==60&&d.Data.remaining==120,"Older work progress clamps to the one-minute quota");
var oldPos=room.transform.position;var oldRot=room.transform.rotation;room.transform.SetPositionAndRotation(oldPos+new Vector3(20,0,0),Quaternion.Euler(0,90,0));Place(work);Check(room.AtWork(a),"Translated and rotated module retains local work zone");room.transform.SetPositionAndRotation(oldPos,oldRot);
save.Restore(a,fresh);p.GetComponent<PrisonGame.Prototype.SoloClockDriver>().enabled=true;Physics.SyncTransforms();
System.IO.Directory.CreateDirectory("../docs/evidence");System.IO.File.WriteAllLines("../docs/evidence/duty01-checks.txt",result);return string.Join("\n",result);
