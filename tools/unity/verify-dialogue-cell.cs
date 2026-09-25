if(!EditorApplication.isPlaying)throw new System.Exception("Enter Duty01 Play");
var actor=UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.PlayerInteraction>();
var checks=new System.Collections.Generic.List<string>();
try{PrisonGame.Prototype.DialogueCellCheck.Run(actor,(ok,name)=>{if(!ok)throw new System.Exception("FAIL: "+name);checks.Add("PASS: "+name);});}
finally{System.IO.File.WriteAllLines("../docs/evidence/dialogue-cell-checks.txt",checks);}
return string.Join("\n",checks);
