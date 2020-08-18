using UnityEngine;

public class MobileInputToggle : BaseMobileInputToggle
{
    [SerializeField]
    private string keyName;

    protected override void OnToggle(bool isOn)
    {
        if (isOn)
            InputManager.SetButtonDown(keyName);
        else
            InputManager.SetButtonUp(keyName);
    }
}
