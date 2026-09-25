if(!EditorApplication.isPlaying)throw new System.Exception("Enter fresh Duty01 Play first");
var actor=UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.PlayerInteraction>();
var driver=actor.GetComponent<PrisonGame.Prototype.SoloClockDriver>();driver.enabled=false;
var checks=new System.Collections.Generic.List<string>();
try {PrisonGame.Prototype.InventoryPrototypeCheck.Run(actor,(ok,name)=>{if(!ok)throw new System.Exception("FAIL: "+name);checks.Add("PASS: "+name);});}
finally {driver.enabled=true;System.IO.File.WriteAllLines("../docs/evidence/inventory01-checks.txt",checks);}
return string.Join("\n",checks);
