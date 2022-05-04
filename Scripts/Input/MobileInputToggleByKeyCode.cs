using UnityEngine;

public class MobileInputToggleByKeyCode : BaseMobileInputToggle
{
    [SerializeField]
    private KeyCode keyCode = KeyCode.None;

    protected override void OnToggle(bool isOn)
    {
        if (isOn)
            InputManager.SetKeyDown(keyCode);
        else
            InputManager.SetKeyUp(keyCode);
    }
}
