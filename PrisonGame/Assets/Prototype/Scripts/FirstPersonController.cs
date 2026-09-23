using UnityEngine;
using UnityEngine.InputSystem;

namespace PrisonGame.Prototype
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonController : MonoBehaviour
    {
        private const string SensitivityKey = "PrisonGame.MouseSensitivity";

        [SerializeField, Min(0.1f)] private float walkSpeed = 3f;
        [SerializeField, Range(0.03f, 0.4f)] private float mouseSensitivity = 0.12f;
        [SerializeField] private Camera playerCamera;

        private CharacterController controller;
        private InputAction moveAction;
        private InputAction lookAction;
        private float verticalSpeed;
        private float pitch;
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;
        private bool cursorCaptured;
        private bool skipLookFrame;
        private GUIStyle hintStyle;
        private GUIStyle labelStyle;
        private GUIStyle buttonStyle;
        public bool ControlsActive => cursorCaptured && Cursor.lockState == CursorLockMode.Locked;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null)
            {
                Debug.LogError("FirstPersonController needs a child camera.", this);
                enabled = false;
                return;
            }

            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
            mouseSensitivity = Mathf.Clamp(PlayerPrefs.GetFloat(SensitivityKey, mouseSensitivity), 0.03f, 0.4f);
            moveAction = new InputAction("Walk", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            lookAction = new InputAction("Look", InputActionType.PassThrough, "<Mouse>/delta");
        }

        private void OnEnable()
        {
            moveAction?.Enable();
            lookAction?.Enable();
            SetCursorCaptured(false);
        }

        private void OnDisable()
        {
            moveAction?.Disable();
            lookAction?.Disable();
            SetCursorCaptured(false);
        }

        private void OnDestroy()
        {
            moveAction?.Dispose();
            lookAction?.Dispose();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) SetCursorCaptured(false);
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                SetCursorCaptured(false);
            // The Editor or OS can release the cursor independently of this component.
            if (cursorCaptured && Cursor.lockState != CursorLockMode.Locked)
                SetCursorCaptured(false);

            if (cursorCaptured)
            {
                if (!skipLookFrame) ApplyLook(lookAction.ReadValue<Vector2>());
                skipLookFrame = false;
            }
            StepMovement(cursorCaptured ? moveAction.ReadValue<Vector2>() : Vector2.zero, Time.deltaTime);
        }

        private void ApplyLook(Vector2 mouseDelta)
        {
            // Mouse delta is already distance accumulated this frame, not a rate.
            transform.Rotate(0f, mouseDelta.x * mouseSensitivity, 0f);
            pitch = Mathf.Clamp(pitch - mouseDelta.y * mouseSensitivity, -85f, 85f);
            playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void StepMovement(Vector2 input, float deltaTime)
        {
            if (deltaTime <= 0f || !controller.enabled) return;
            input = Vector2.ClampMagnitude(input, 1f);
            if (controller.isGrounded && verticalSpeed < 0f) verticalSpeed = -2f;
            verticalSpeed = Mathf.Max(verticalSpeed - 20f * deltaTime, -30f);
            Vector3 planar = transform.right * input.x + transform.forward * input.y;
            controller.Move((planar * walkSpeed + Vector3.up * verticalSpeed) * deltaTime);

            // Recover from accidental falls while the prototype geometry is being edited.
            if (transform.position.y < -10f)
            {
                controller.enabled = false;
                transform.SetPositionAndRotation(spawnPosition, spawnRotation);
                controller.enabled = true;
                verticalSpeed = 0f;
                pitch = 0f;
                playerCamera.transform.localRotation = Quaternion.identity;
            }
        }

        private void SetCursorCaptured(bool captured)
        {
            cursorCaptured = captured;
            skipLookFrame = captured;
            Cursor.lockState = captured ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !captured;
        }

        private void OnGUI()
        {
            if (playerCamera == null) return;
            if (hintStyle == null)
            {
                hintStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
                labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true };
                buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 18 };
            }
            Matrix4x4 previousMatrix = GUI.matrix;
            float scale = Mathf.Clamp(Screen.height / 900f, 0.75f, 2.5f);
            GUI.matrix = Matrix4x4.Scale(Vector3.one * scale);
            float viewWidth = Screen.width / scale;
            float viewHeight = Screen.height / scale;

            if (cursorCaptured)
            {
                GUI.Label(new Rect(viewWidth / 2f - 12, viewHeight / 2f - 12, 24, 24), "+", hintStyle);
                GUI.Box(new Rect(viewWidth / 2f - 300, viewHeight - 42, 600, 32), GUIContent.none);
                GUI.Label(new Rect(viewWidth / 2f - 300, viewHeight - 42, 600, 32), "WASD: walk   E: interact   Q: put down   Esc: settings", hintStyle);
                GUI.matrix = previousMatrix;
                return;
            }

            float width = Mathf.Min(440, viewWidth - 20);
            float panelHeight = Application.isEditor ? 270 : 320;
            Rect panel = new Rect((viewWidth - width) / 2f, Mathf.Max(10, (viewHeight - panelHeight) / 2f), width, panelHeight);
            GUI.Box(panel, "Prison prototype - settings");
            GUILayout.BeginArea(new Rect(panel.x + 20, panel.y + 30, panel.width - 40, panel.height - 40));
            GUILayout.Label("WASD to walk. Move the mouse to look.", labelStyle);
            GUILayout.Label("E interacts. Q puts down a carried item.", labelStyle);
            GUILayout.Label("Escape releases the mouse and opens these settings.", labelStyle);
            GUILayout.Space(10);
            GUILayout.Label("Mouse sensitivity: " + mouseSensitivity.ToString("0.00"), labelStyle);
            float sensitivity = GUILayout.HorizontalSlider(mouseSensitivity, 0.03f, 0.4f);
            if (!Mathf.Approximately(sensitivity, mouseSensitivity))
            {
                mouseSensitivity = sensitivity;
                PlayerPrefs.SetFloat(SensitivityKey, sensitivity);
                PlayerPrefs.Save();
            }
            GUILayout.Space(12);
            if (GUILayout.Button("Resume walking", buttonStyle, GUILayout.Height(40))) SetCursorCaptured(true);
            if (!Application.isEditor && GUILayout.Button("Quit game", buttonStyle, GUILayout.Height(36))) Application.Quit();
            GUILayout.EndArea();
            GUI.matrix = previousMatrix;
        }
    }
}
