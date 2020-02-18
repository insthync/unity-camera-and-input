using System;
using System.Collections.Generic;
using UnityEngine;

public static class InputManager
{
    private static Dictionary<string, SimulateButton> simulateInputs = new Dictionary<string, SimulateButton>();
    private static Dictionary<string, SimulateAxis> simulateAxis = new Dictionary<string, SimulateAxis>();
    public static bool useMobileInputOnNonMobile = false;

    public static bool HasInputSetting(string keyName)
    {
        return InputSettingManager.Singleton != null && InputSettingManager.Singleton.Settings.ContainsKey(keyName);
    }

    public static float GetAxis(string name, bool raw)
    {
        float axis = 0;
        if (useMobileInputOnNonMobile || Application.isMobilePlatform)
        {
            SimulateAxis foundSimulateAxis;
            if (axis == 0 && simulateAxis.TryGetValue(name, out foundSimulateAxis))
                axis = foundSimulateAxis.Value;
            if (raw)
            {
                if (axis > 0)
                    axis = 1;
                if (axis < 0)
                    axis = -1;
            }
        }
        else
        {
            try
            {
                axis = raw ? Input.GetAxisRaw(name) : Input.GetAxis(name);
            }
            catch
            {
                axis = 0;
            }
        }
        return axis;
    }

    public static bool GetButton(string name)
    {
        if (useMobileInputOnNonMobile || Application.isMobilePlatform)
        {
            SimulateButton foundSimulateButton;
            return simulateInputs.TryGetValue(name, out foundSimulateButton) && foundSimulateButton.Pressed;
        }
        if (HasInputSetting(name))
        {
            HashSet<KeyCode> keyCodes = InputSettingManager.Singleton.Settings[name];
            foreach (KeyCode keyCode in keyCodes)
            {
                if (Input.GetKey(keyCode))
                    return true;
            }
        }
        try
        {
            return Input.GetButton(name);
        }
        catch
        {
            return false;
        }
    }

    public static bool GetButtonDown(string name)
    {
        if (useMobileInputOnNonMobile || Application.isMobilePlatform)
        {
            SimulateButton foundSimulateButton;
            return simulateInputs.TryGetValue(name, out foundSimulateButton) && foundSimulateButton.ButtonDown;
        }
        if (HasInputSetting(name))
        {
            HashSet<KeyCode> keyCodes = InputSettingManager.Singleton.Settings[name];
            foreach (KeyCode keyCode in keyCodes)
            {
                if (Input.GetKeyDown(keyCode))
                    return true;
            }
        }
        try
        {
            return Input.GetButtonDown(name);
        }
        catch
        {
            return false;
        }
    }

    public static bool GetButtonUp(string name)
    {
        if (useMobileInputOnNonMobile || Application.isMobilePlatform)
        {
            SimulateButton foundSimulateButton;
            return simulateInputs.TryGetValue(name, out foundSimulateButton) && foundSimulateButton.ButtonUp;
        }
        if (HasInputSetting(name))
        {
            HashSet<KeyCode> keyCodes = InputSettingManager.Singleton.Settings[name];
            foreach (KeyCode keyCode in keyCodes)
            {
                if (Input.GetKeyUp(keyCode))
                    return true;
            }
        }
        try
        {
            return Input.GetButtonUp(name);
        }
        catch
        {
            return false;
        }
    }

    public static void SetButtonDown(string name)
    {
        if (!simulateInputs.ContainsKey(name))
        {
            simulateInputs.Add(name, new SimulateButton());
        }
        simulateInputs[name].Press();
    }

    public static void SetButtonUp(string name)
    {
        if (!simulateInputs.ContainsKey(name))
        {
            simulateInputs.Add(name, new SimulateButton());
        }
        simulateInputs[name].Release();
    }

    public static void SetAxisPositive(string name)
    {
        if (!simulateAxis.ContainsKey(name))
        {
            simulateAxis.Add(name, new SimulateAxis());
        }
        simulateAxis[name].Update(1f);
    }

    public static void SetAxisNegative(string name)
    {
        if (!simulateAxis.ContainsKey(name))
        {
            simulateAxis.Add(name, new SimulateAxis());
        }
        simulateAxis[name].Update(-1f);
    }

    public static void SetAxisZero(string name)
    {
        if (!simulateAxis.ContainsKey(name))
        {
            simulateAxis.Add(name, new SimulateAxis());
        }
        simulateAxis[name].Update(0);
    }

    public static void SetAxis(string name, float value)
    {
        if (!simulateAxis.ContainsKey(name))
        {
            simulateAxis.Add(name, new SimulateAxis());
        }
        simulateAxis[name].Update(value);
    }

    public static Vector3 MousePosition()
    {
        return Input.mousePosition;
    }

    public class SimulateButton
    {
        private int lastPressedFrame = -5;
        private int releasedFrame = -5;

        public bool Pressed { get; private set; }

        public void Press()
        {
            if (Pressed)
                return;
            Pressed = true;
            lastPressedFrame = Time.frameCount;
        }

        public void Release()
        {
            Pressed = false;
            releasedFrame = Time.frameCount;
        }

        public bool ButtonDown
        {
            get { return lastPressedFrame - Time.frameCount == -1; }
        }

        public bool ButtonUp
        {
            get { return releasedFrame == Time.frameCount - 1; }
        }
    }

    public class SimulateAxis
    {
        public float Value { get; private set; }
        public void Update(float value)
        {
            Value = value;
        }
    }
}
