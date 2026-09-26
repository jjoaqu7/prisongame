using PrisonGame.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PrisonGame.PlayerCombat
{
    // Only this adapter reads combat bindings. Menus/inventory can gate world actions explicitly.
    // The sandbox uses the existing movement controller's cursor capture without referencing it.
    [RequireComponent(typeof(Fighter))]
    [DefaultExecutionOrder(40)]
    public sealed class LocalCombatInput : MonoBehaviour
    {
        private Fighter _fighter;
        private bool _wasAvailable;
        private bool _waitForRelease;
        public bool WorldActionsEnabled { get; set; } = true;

        private void Awake() => _fighter = GetComponent<Fighter>();

        private void OnDisable()
        {
            _wasAvailable = false;
            _waitForRelease = true;
            if (_fighter != null && _fighter.Actions != null)
            {
                _fighter.Actions.Cancel();
                _fighter.SubmitIntent(default);
            }
        }

        private void Update()
        {
            Mouse mouse = Mouse.current;
            Keyboard keyboard = Keyboard.current;
            bool available = WorldActionsEnabled && Cursor.lockState == CursorLockMode.Locked &&
                (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame);
            bool anyHeld = (mouse != null && (mouse.leftButton.isPressed || mouse.rightButton.isPressed)) ||
                (keyboard != null && keyboard.fKey.isPressed);
            if (!available || !_wasAvailable)
            {
                _waitForRelease = anyHeld;
            }
            _wasAvailable = available;
            if (!anyHeld)
            {
                _waitForRelease = false;
            }
            if (!available || _waitForRelease)
            {
                _fighter.SubmitIntent(default);
                return;
            }
            _fighter.SubmitIntent(new CombatIntent(true,
                mouse != null && mouse.leftButton.wasPressedThisFrame,
                mouse != null && mouse.leftButton.isPressed,
                mouse != null && mouse.rightButton.isPressed,
                keyboard != null && keyboard.fKey.wasPressedThisFrame));
        }
    }
}
