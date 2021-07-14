using UnityEngine;

public class MobileInputToggleAxis : BaseMobileInputToggle
{
    [SerializeField]
    private string axisName = string.Empty;
    [SerializeField]
    private float axisValueWhenOff = 0f;
    [SerializeField]
    private float axisValueWhileOn = 0f;

    private void Update()
    {
        if (IsOn)
            InputManager.SetAxis(axisName, axisValueWhileOn);
    }

    protected override void OnToggle(bool isOn)
    {
        InputManager.SetAxis(axisName, isOn ? axisValueWhileOn : axisValueWhenOff);
    }
}
