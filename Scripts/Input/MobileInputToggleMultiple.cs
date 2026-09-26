using UnityEngine;

namespace Insthync.CameraAndInput
{
    public class MobileInputToggleMultiple : BaseMobileInputToggle
    {
        [System.Serializable]
        public struct Setting
        {
            public string axisName;
            public float axisValueWhenOff;
            public float axisValueWhileOn;
            public string keyName;
            public KeyCode keyCode;
        }

        [SerializeField]
        private Setting[] settings = new Setting[0];

        protected override void OnToggle(bool isOn)
        {
            for (int i = 0; i < settings.Length; ++i)
            {
                Setting setting = settings[i];
                if (!string.IsNullOrEmpty(setting.axisName))
                    InputManager.SetAxis(setting.axisName, isOn ? setting.axisValueWhileOn : setting.axisValueWhenOff);
                if (!string.IsNullOrEmpty(setting.keyName))
                {
                    if (isOn)
                        InputManager.SetButtonDown(setting.keyName);
                    else
                        InputManager.SetButtonUp(setting.keyName);
                }
                if (setting.keyCode != KeyCode.None)
                {
                    if (isOn)
                        InputManager.SetKeyDown(setting.keyCode);
                    else
                        InputManager.SetKeyUp(setting.keyCode);
                }
            }
        }
    }
}
