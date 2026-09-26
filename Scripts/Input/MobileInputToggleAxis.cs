using UnityEngine;

namespace Insthync.CameraAndInput
{
    public class MobileInputToggleAxis : BaseMobileInputToggle
    {
        [SerializeField]
        private string axisName = string.Empty;
        [SerializeField]
        private float axisValueWhenOff = 0f;
        [SerializeField]
        private float axisValueWhileOn = 0f;

        protected override void OnToggle(bool isOn)
        {
            InputManager.SetAxis(axisName, isOn ? axisValueWhileOn : axisValueWhenOff);
        }
    }
}
