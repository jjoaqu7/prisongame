using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace PrisonGame.PlayerCombat.Tests
{
    // Input tests queue events on simulated devices. While they run, the machine's real mouse and
    // keyboard are disabled, so a person using the computer cannot make a real device "current"
    // and steal the test's input. Restore re-enables exactly the devices that were disabled.
    internal static class InputIsolation
    {
        public static List<InputDevice> DisableRealPointerAndKeyboard()
        {
            var disabled = new List<InputDevice>();
            foreach (InputDevice device in InputSystem.devices)
            {
                if (device.enabled && (device is Mouse || device is Keyboard))
                {
                    disabled.Add(device);
                }
            }
            foreach (InputDevice device in disabled)
            {
                InputSystem.DisableDevice(device);
            }
            return disabled;
        }

        public static void Restore(List<InputDevice> disabled)
        {
            if (disabled == null)
            {
                return;
            }
            foreach (InputDevice device in disabled)
            {
                if (device.added && !device.enabled)
                {
                    InputSystem.EnableDevice(device);
                }
            }
            disabled.Clear();
        }
    }
}
