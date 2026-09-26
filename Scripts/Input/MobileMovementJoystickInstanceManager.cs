using System.Collections.Generic;

namespace Insthync.CameraAndInput
{
    public static class MobileMovementJoystickInstanceManager
    {
        private static HashSet<MobileMovementJoystick> _instances = new HashSet<MobileMovementJoystick>();

        public static void Add(MobileMovementJoystick instance)
        {
            if (instance == null)
                return;
            _instances.Add(instance);
        }

        public static void Remove(MobileMovementJoystick instance)
        {
            if (instance == null)
                return;
            _instances.Remove(instance);
        }

        public static void Clear()
        {
            _instances.Clear();
        }
    }
}
