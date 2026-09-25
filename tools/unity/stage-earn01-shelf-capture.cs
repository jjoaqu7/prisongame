if(!EditorApplication.isPlaying)throw new System.Exception("Enter Play mode.");
var p=GameObject.Find("Player");var actor=p.GetComponent<PrisonGame.Prototype.PlayerInteraction>();var stock=p.GetComponent<PrisonGame.Prototype.SnackSupplies>();var tray=UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.SnackAssembly>();var supply=UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.SnackSupplyBox>();var npc=UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.PrototypeInmate>();var shelf=UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.SnackShelf>();
if(!stock.StarterCollected){supply.Interact(actor);for(int i=0;i<3;i++){if(i==2)supply.Interact(actor);for(int j=0;j<3;j++)tray.Interact(actor);npc.Interact(actor);}shelf.Interact(actor);}
var cc=p.GetComponent<CharacterController>();cc.enabled=false;p.transform.SetPositionAndRotation(new Vector3(-3.85f,.05f,-1.2f),Quaternion.Euler(0,180,0));cc.enabled=true;
var movement=p.GetComponent<PrisonGame.Prototype.FirstPersonController>();movement.enabled=false;
var cam=p.GetComponentInChildren<Camera>();cam.transform.rotation=Quaternion.LookRotation(new Vector3(-3.85f,1.27f,-2.65f)-cam.transform.position);
Application.runInBackground=true;var gameView=EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));gameView.Show();gameView.Focus();
typeof(PrisonGame.Prototype.FirstPersonController).GetMethod("SetCursorCaptured",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(movement,new object[]{true});
Physics.SyncTransforms();EditorApplication.QueuePlayerLoopUpdate();gameView.Repaint();UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
return "Shelf purchased through production rules; camera staged at cell shelf, HUD enabled. Frame="+Time.frameCount;
