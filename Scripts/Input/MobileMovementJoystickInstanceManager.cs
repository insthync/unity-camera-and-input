using System.Collections.Generic;

namespace Insthync.CameraAndInput
{
    public static class MobileMovementJoystickInstanceManager
    {
        private static List<MobileMovementJoystick> _joystickInstances = new();
        public static void Add(MobileMovementJoystick joystick)
        {
            if (joystick == null)
                return;

            if (!_joystickInstances.Contains(joystick))
                _joystickInstances.Add(joystick);
        }

        public static void Remove(MobileMovementJoystick joystick)
        {
            if (joystick == null)
                return;

            if (_joystickInstances.Contains(joystick))
                _joystickInstances.Remove(joystick);
        }

        public static void Clear()
        {
            _joystickInstances.Clear();
        }

        public static void UnToggleAll()
        {
            foreach (var joyInstance in _joystickInstances)
                joyInstance.UnToggle();
        }
    }
}