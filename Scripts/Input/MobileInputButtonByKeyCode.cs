using UnityEngine;

namespace Insthync.CameraAndInput
{
    public class MobileInputButtonByKeyCode : BaseMobileInputButton
    {
        public KeyCode keyCode = KeyCode.None;

        protected override void OnButtonDown()
        {
            InputManager.SetKeyDown(keyCode);
        }

        protected override void OnButtonUp()
        {
            InputManager.SetKeyUp(keyCode);
        }
    }
}
