// Run after verify-save01-progress.cs, stopping and re-entering Play in Save01.
if(!EditorApplication.isPlaying)throw new System.Exception("Enter a fresh Save01 Play session.");
var p=GameObject.Find("Player");var save=p.GetComponent<PrisonGame.Prototype.SampleSaveGame>();var actor=p.GetComponent<PrisonGame.Prototype.PlayerInteraction>();
if(!save.LoadFrom(actor,"Temp/Save01Checks/roundtrip.json"))throw new System.Exception(save.Status);
var state=save.Capture(actor);
if(state.player.money!=3 || state.player.delivered!=1 || state.world.trayStage!=1 || state.player.suspicion!=55 || !state.world.parcels[0].held || state.world.minutes!=480)throw new System.Exception("Restart roundtrip mismatch");
const string result="PASS: A fresh Play session loads the prior session file, restoring money, request, partial tray, suspicion, held parcel and prison time.";
System.IO.File.WriteAllText("../docs/images/save01-restart-checks.txt",result+"\n");return result;
