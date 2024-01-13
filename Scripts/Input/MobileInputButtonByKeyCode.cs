using UnityEngine;

public class MobileInputButtonByKeyCode : BaseMobileInputButton
{
    [Header("Key")]
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
