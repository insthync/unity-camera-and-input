using UnityEngine;

public class MobileInputToggleByKeyCode : BaseMobileInputToggle
{
    public KeyCode keyCode = KeyCode.None;

    protected override void OnToggle(bool isOn)
    {
        if (isOn)
            InputManager.SetKeyDown(keyCode);
        else
            InputManager.SetKeyUp(keyCode);
    }
}
