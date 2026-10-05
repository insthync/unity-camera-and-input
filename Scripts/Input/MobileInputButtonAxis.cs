using UnityEngine;

namespace Insthync.CameraAndInput
{
    public class MobileInputButtonAxis : BaseMobileInputButton
    {
        [SerializeField]
        private string axisName = string.Empty;
        [SerializeField]
        private float axisValue = 1f;

        protected override void OnButtonDown()
        {
            InputManager.SetAxis(axisName, axisValue);
        }

        protected override void OnButtonUp()
        {
            InputManager.SetAxis(axisName, 0f);
        }
    }
}
