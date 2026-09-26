using System.Collections.Generic;

namespace Insthync.CameraAndInput
{
    public static class MobileInputToggleInstanceManager
    {
        private static HashSet<IMobileInputToggle> _instances = new HashSet<IMobileInputToggle>();

        public static void Add(IMobileInputToggle instance)
        {
            if (instance == null)
                return;
            _instances.Add(instance);
        }

        public static void Remove(IMobileInputToggle instance)
        {
            if (instance == null)
                return;
            _instances.Remove(instance);
        }

        public static void Clear()
        {
            _instances.Clear();
        }

        public static void UnToggleAll()
        {
            foreach (var instance in _instances)
            {
                instance.UnToggle();
            }
        }

        public static void Toggle(IMobileInputToggle toggle)
        {
            if (toggle == null)
                return;
            string groupName = toggle.ToggleGroupName;
            foreach (var instance in _instances)
            {
                if (instance != toggle && string.Equals(groupName, instance.ToggleGroupName))
                    instance.Toggle();
            }
        }

        public static void UnToggle(IMobileInputToggle toggle)
        {
            if (toggle == null)
                return;
            string groupName = toggle.ToggleGroupName;
            foreach (var instance in _instances)
            {
                if (instance != toggle && string.Equals(groupName, instance.ToggleGroupName))
                    instance.UnToggle();
            }
        }
    }
}
