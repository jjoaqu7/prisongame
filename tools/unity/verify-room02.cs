// Pipeline eval_file snippet. Run in Play mode in Room01_Blockout.
// Exercises the actual controller/physics and input actions, restoring runtime state afterward.
if (!EditorApplication.isPlaying) throw new System.Exception("Enter Play mode first.");
var player = GameObject.Find("Player");
var script = player.GetComponent<PrisonGame.Prototype.FirstPersonController>();
var cc = player.GetComponent<CharacterController>();
var camera = player.GetComponentInChildren<Camera>();
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
var type = script.GetType();
var step = type.GetMethod("StepMovement", flags);
var look = type.GetMethod("ApplyLook", flags);
var capture = type.GetMethod("SetCursorCaptured", flags);
var originalPosition = player.transform.position;
var originalRotation = player.transform.rotation;
var originalCameraRotation = camera.transform.localRotation;
var originalPitch = type.GetField("pitch", flags).GetValue(script);
var originalVertical = type.GetField("verticalSpeed", flags).GetValue(script);
bool wasEnabled = script.enabled;
// ROOM-03 adds a closed door. These movement-only checks need a clear doorway;
// verify-room03.cs separately checks movement against the closed and open door.
var door = UnityEngine.Object.FindFirstObjectByType<PrisonGame.Prototype.PrototypeDoor>();
var doorCollider = door != null ? door.GetComponent<BoxCollider>() : null;
bool doorWasEnabled = doorCollider != null && doorCollider.enabled;
var inputSettings = UnityEngine.InputSystem.InputSystem.settings;
var previousBackground = inputSettings.backgroundBehavior;
var previousEditorInput = inputSettings.editorInputBehaviorInPlayMode;
UnityEngine.InputSystem.Keyboard keyboard = null;
var results = new System.Collections.Generic.List<string>();
void Check(bool ok, string label) { if (!ok) throw new System.Exception("FAIL: " + label); results.Add("PASS: " + label); }
void Place(Vector3 p, float yaw) {
    cc.enabled = false; player.transform.SetPositionAndRotation(p, Quaternion.Euler(0,yaw,0)); cc.enabled=true;
    type.GetField("verticalSpeed",flags).SetValue(script,0f); Physics.SyncTransforms();
}
void Walk(Vector2 input, int frames, float dt) {
    for (int i=0;i<frames;i++) step.Invoke(script,new object[]{input,dt});
}
try {
    if (doorCollider != null) doorCollider.enabled = false;
    // Synthetic device tests must not depend on whether the user is focused on VS Code.
    // These temporary test settings are restored below; production focus behavior is unchanged.
    inputSettings.backgroundBehavior = UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
    inputSettings.editorInputBehaviorInPlayMode = UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
    script.enabled=false;
    Place(new Vector3(-3.4f,.05f,-.5f),90);
    Walk(Vector2.up,58,1f/60);
    Check(Mathf.Abs(player.transform.position.x + .5f)<.12f,"Cross cell doorway using controller");
    player.transform.rotation=Quaternion.identity;
    Walk(Vector2.up,50,1f/60);
    Check(Mathf.Abs(player.transform.position.z-2)<.12f,"Walk and turn in corridor");
    player.transform.rotation=Quaternion.Euler(0,90,0);
    Walk(Vector2.up,50,1f/60);
    Check(Mathf.Abs(player.transform.position.x-2)<.12f,"Cross common-area entrance");

    Place(new Vector3(-.5f,.05f,-3),90);
    Walk(Vector2.up,120,1f/60);
    Check(player.transform.position.x>.4f && player.transform.position.x<.65f,"Solid wall stops sustained movement");
    Check(player.transform.position.y>-.1f && player.transform.position.y<.15f,"Gravity settles on floor");

    float[] distances = new float[3]; int[] rates = {30,60,144};
    for(int i=0;i<rates.Length;i++) {
        Place(new Vector3(-.5f,.05f,-5),0); Walk(Vector2.up,rates[i],1f/rates[i]);
        distances[i]=player.transform.position.z+5;
    }
    Check(System.Linq.Enumerable.All(distances,d=>Mathf.Abs(d-3.02f)<.005f),"Walk speed is 3.02m/s at 30, 60, 144 FPS");
    Place(new Vector3(6.5f,.05f,4),0);
    Walk(new Vector2(1,1),15,1f/60);
    var offset=player.transform.position-new Vector3(6.5f,0,4); offset.y=0;
    Check(Mathf.Abs(offset.magnitude-.755f)<.005f,"Diagonal movement has no speed bonus");

    Place(new Vector3(-.5f,.05f,-4),0);
    type.GetField("pitch",flags).SetValue(script,0f);
    look.Invoke(script,new object[]{new Vector2(0,100000)});
    Check(Mathf.Abs((float)type.GetField("pitch",flags).GetValue(script)+85)<.01f,"Look up is clamped");
    look.Invoke(script,new object[]{new Vector2(0,-100000)});
    Check(Mathf.Abs((float)type.GetField("pitch",flags).GetValue(script)-85)<.01f,"Look down is clamped");

    Place(new Vector3(-.5f,-11,-4),0); Walk(Vector2.zero,1,1f/60);
    var spawn=(Vector3)type.GetField("spawnPosition",flags).GetValue(script);
    Check(Vector3.Distance(player.transform.position,spawn)<.01f,"Accidental fall returns to spawn");

    var action=(UnityEngine.InputSystem.InputAction)type.GetField("moveAction",flags).GetValue(script);
    action.Enable();
    keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
    // Complete action enable/device binding before sending the first key event.
    UnityEngine.InputSystem.InputSystem.Update();
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.W));
    UnityEngine.InputSystem.InputSystem.Update();
    Check(action.ReadValue<Vector2>().y>.99f,"W binding produces forward input");
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.A));
    UnityEngine.InputSystem.InputSystem.Update();
    Check(action.ReadValue<Vector2>().x<-.99f,"A binding produces left input");

    capture.Invoke(script,new object[]{true});
    type.GetMethod("OnApplicationFocus",flags).Invoke(script,new object[]{false});
    Check(Cursor.lockState==CursorLockMode.None && Cursor.visible && !(bool)type.GetField("cursorCaptured",flags).GetValue(script),"Focus loss releases cursor");
    capture.Invoke(script,new object[]{true});
    UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.Escape));
    UnityEngine.InputSystem.InputSystem.Update();
    type.GetMethod("Update",flags).Invoke(script,null);
    Check(Cursor.lockState==CursorLockMode.None && Cursor.visible,"Escape releases cursor");
    return string.Join("\n",results);
} finally {
    if (doorCollider != null) doorCollider.enabled = doorWasEnabled;
    if(keyboard!=null) UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);
    inputSettings.backgroundBehavior = previousBackground;
    inputSettings.editorInputBehaviorInPlayMode = previousEditorInput;
    cc.enabled=false; player.transform.SetPositionAndRotation(originalPosition,originalRotation); cc.enabled=true;
    camera.transform.localRotation=originalCameraRotation;
    type.GetField("pitch",flags).SetValue(script,originalPitch);
    type.GetField("verticalSpeed",flags).SetValue(script,originalVertical);
    script.enabled=wasEnabled; capture.Invoke(script,new object[]{false}); Physics.SyncTransforms();
}
