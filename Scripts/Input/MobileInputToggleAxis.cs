using UnityEngine;

public class MobileInputToggleAxis : BaseMobileInputToggle
{
    [SerializeField]
    private string axisName;
    [SerializeField]
    private float axisValueWhenOff;
    [SerializeField]
    private float axisValueWhileOn;

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
