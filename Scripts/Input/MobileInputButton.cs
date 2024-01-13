using UnityEngine;

public class MobileInputButton : BaseMobileInputButton
{
    [Header("Key")]
    public string keyName = string.Empty;

    protected override void OnButtonDown()
    {
        InputManager.SetButtonDown(keyName);
    }

    protected override void OnButtonUp()
    {
        InputManager.SetButtonUp(keyName);
    }
}
