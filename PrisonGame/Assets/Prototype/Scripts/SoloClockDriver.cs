using UnityEngine;
using UnityEngine.InputSystem;

namespace PrisonGame.Prototype
{
    // Solo-only pause policy. World rules do not depend on the local controller or cursor.
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(FirstPersonController))]
    public sealed class SoloClockDriver : MonoBehaviour
    {
        [SerializeField] private PrisonClock clock;
        private FirstPersonController movement;
        private bool focused;
        private float originalTimeScale;
        private bool wasRunning;
        private double lastSample;

        private void OnEnable()
        {
            movement = GetComponent<FirstPersonController>();
            originalTimeScale = Time.timeScale;
            focused = Application.isFocused;
            wasRunning = false;
            lastSample = Time.realtimeSinceStartupAsDouble;
            ApplyPause(true);
        }

        private void Update()
        {
            bool escape = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
            bool running = focused && movement.ControlsActive && !escape;
            double now = Time.realtimeSinceStartupAsDouble;
            ApplyPause(!running);
            // On the first resumed frame, discard the interval that belonged to the pause.
            if (running && wasRunning && clock != null) clock.Advance(now - lastSample);
            lastSample = now;
            wasRunning = running;
        }

        private void OnApplicationFocus(bool focus)
        {
            focused = focus;
            if (!focus) { wasRunning = false; ApplyPause(true); }
        }

        private void ApplyPause(bool paused)
        {
            if (clock != null) clock.SetPaused(paused);
            Time.timeScale = paused ? 0 : originalTimeScale;
        }

        private void OnDisable()
        {
            if (clock != null) clock.SetPaused(true);
            Time.timeScale = originalTimeScale;
        }
    }
}
