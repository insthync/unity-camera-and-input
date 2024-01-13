public class MobileInputToggle : BaseMobileInputToggle
{
    public string keyName = string.Empty;

    protected override void OnToggle(bool isOn)
    {
        if (isOn)
            InputManager.SetButtonDown(keyName);
        else
            InputManager.SetButtonUp(keyName);
    }
}
