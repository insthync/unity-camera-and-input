using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

public static class InputManager
{
    private static Dictionary<string, SimulateButton> simulateInputs = new Dictionary<string, SimulateButton>();
    private static Dictionary<KeyCode, SimulateButton> simulateKeys = new Dictionary<KeyCode, SimulateButton>();
    private static Dictionary<string, SimulateAxis> simulateAxis = new Dictionary<string, SimulateAxis>();
    public static bool useMobileInputOnNonMobile = false;
    public static bool useNonMobileInput = false;
    internal static readonly Dictionary<int, GameObject> touchedPointerIds = new Dictionary<int, GameObject>();

    private static int mobileInputLastDragFrame;
    public static bool IsDraggingMobileInput
    {
        get
        {
            return Time.frameCount - mobileInputLastDragFrame <= 1;
        }
    }

#if ENABLE_INPUT_SYSTEM
    private static HashSet<string> alreadyFindInputActionNames = new HashSet<string>();
    private static Dictionary<string, InputAction> foundInputActions = new Dictionary<string, InputAction>();
#endif

    public static void UpdateMobileInputDragging()
    {
        mobileInputLastDragFrame = Time.frameCount;
    }

    public static bool HasInputSetting(string keyName)
    {
        return !string.IsNullOrEmpty(keyName) && InputSettingManager.Singleton != null && InputSettingManager.Singleton.Settings.ContainsKey(keyName);
    }

    public static bool UseMobileInput()
    {
#if VR_BUILD
        return false;
#else
        return Application.isMobilePlatform || useMobileInputOnNonMobile;
#endif
    }

    public static bool UseNonMobileInput()
    {
#if VR_BUILD
        return true;
#else
        return !Application.isMobilePlatform && (!useMobileInputOnNonMobile || useNonMobileInput);
#endif
    }

#if ENABLE_INPUT_SYSTEM
    public static bool TryGetInputAction(string name, out InputAction inputAction)
    {
        if (!alreadyFindInputActionNames.Contains(name))
        {
            alreadyFindInputActionNames.Add(name);
            if (InputSettingManager.Singleton != null && InputSettingManager.Singleton.inputActionAsset != null)
            {
                inputAction = InputSettingManager.Singleton.inputActionAsset.FindAction(name);
                if (inputAction != null)
                {
                    inputAction.Enable();
                    foundInputActions.Add(name, inputAction);
                    return true;
                }
            }
        }
        else if (foundInputActions.TryGetValue(name, out inputAction))
        {
            return true;
        }
        inputAction = null;
        return false;
    }
#endif

#if USE_REWIRED
    public static float GetAxis(string name, bool raw, int playerId = 0)
#else
    public static float GetAxis(string name, bool raw)
#endif
    {
        // Try get input by rewired system
#if USE_REWIRED
        try
        {
            Rewired.Player player = Rewired.ReInput.players.GetPlayer(playerId);
            float axis = raw ? player.GetAxisRaw(name) : player.GetAxis(name);
            if (Mathf.Abs(axis) > 0.00001f)
                return axis;
        }
        catch { }
#endif

#if ENABLE_INPUT_SYSTEM
        if (TryGetInputAction(name, out InputAction inputAction))
        {
            float axis = inputAction.ReadValue<float>();
            if (raw)
            {
                if (axis > 0f)
                    axis = 1f;
                if (axis < 0f)
                    axis = -1f;
            }
            if (Mathf.Abs(axis) > 0.00001f)
                return axis;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (UseNonMobileInput())
        {
            try
            {
                float axis = raw ? Input.GetAxisRaw(name) : Input.GetAxis(name);
                if (Mathf.Abs(axis) > 0.00001f)
                    return axis;
            }
            catch { }
        }
#endif

        if (UseMobileInput())
        {
            SimulateAxis foundSimulateAxis;
            float axis = 0f;
            if (simulateAxis.TryGetValue(name, out foundSimulateAxis))
                axis = foundSimulateAxis.Value;
            if (raw)
            {
                if (axis > 0f)
                    axis = 1f;
                if (axis < 0f)
                    axis = -1f;
            }
            return axis;
        }
        return 0f;
    }

    public static bool GetKey(KeyCode key)
    {
#if ENABLE_INPUT_SYSTEM
        if (key.TryGetMouseButtonControl(out ButtonControl buttonControl) && buttonControl.isPressed)
            return true;
        if (key.TryGetKeyboardKeyControl(out KeyControl keyControl) && keyControl.isPressed)
            return true;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (UseNonMobileInput())
        {
            try
            {
                if (Input.GetKey(key))
                    return true;
            }
            catch { }
        }
#endif

        if (UseMobileInput())
        {
            SimulateButton foundSimulateButton;
            if (simulateKeys.TryGetValue(key, out foundSimulateButton) && foundSimulateButton.Pressed)
                return true;
        }
        return false;
    }

    public static bool GetKeyDown(KeyCode key)
    {
#if ENABLE_INPUT_SYSTEM
        if (key.TryGetMouseButtonControl(out ButtonControl buttonControl) && buttonControl.wasPressedThisFrame)
            return true;
        if (key.TryGetKeyboardKeyControl(out KeyControl keyControl) && keyControl.wasPressedThisFrame)
            return true;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (UseNonMobileInput())
        {
            try
            {
                if (Input.GetKeyDown(key))
                    return true;
            }
            catch { }
        }
#endif

        if (UseMobileInput())
        {
            SimulateButton foundSimulateButton;
            if (simulateKeys.TryGetValue(key, out foundSimulateButton) && foundSimulateButton.ButtonDown)
                return true;
        }
        return false;
    }

    public static bool GetKeyUp(KeyCode key)
    {
#if ENABLE_INPUT_SYSTEM
        if (key.TryGetMouseButtonControl(out ButtonControl buttonControl) && buttonControl.wasReleasedThisFrame)
            return true;
        if (key.TryGetKeyboardKeyControl(out KeyControl keyControl) && keyControl.wasReleasedThisFrame)
            return true;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (UseNonMobileInput())
        {
            try
            {
                if (Input.GetKeyUp(key))
                    return true;
            }
            catch { }
        }
#endif

        if (UseMobileInput())
        {
            SimulateButton foundSimulateButton;
            if (simulateKeys.TryGetValue(key, out foundSimulateButton) && foundSimulateButton.ButtonUp)
                return true;
        }
        return false;
    }

    private static bool IsKeyFromSettingActivated(string name, System.Func<KeyCode, bool> func)
    {
        if (HasInputSetting(name))
        {
            List<KeyCode> keyCodes = InputSettingManager.Singleton.Settings[name];
            foreach (KeyCode keyCode in keyCodes)
            {
                if (func.Invoke(keyCode))
                    return true;
            }
        }
        return false;
    }

#if USE_REWIRED
    public static bool GetButton(string name, int playerId = 0)
#else
    public static bool GetButton(string name)
#endif
    {
        if (string.IsNullOrEmpty(name))
            return false;

        // Try get input by rewired system
#if USE_REWIRED
        try
        {
            Rewired.Player player = Rewired.ReInput.players.GetPlayer(playerId);
            if (player.GetButton(name))
                return true;
        }
        catch { }
#endif

#if ENABLE_INPUT_SYSTEM
        if (TryGetInputAction(name, out InputAction inputAction))
        {
            if (inputAction.IsPressed())
                return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (UseNonMobileInput())
        {
            try
            {
                if (Input.GetButton(name))
                    return true;
            }
            catch { }
            if (IsKeyFromSettingActivated(name, GetKey))
                return true;
        }
#endif

        if (UseMobileInput())
        {
            SimulateButton foundSimulateButton;
            if (simulateInputs.TryGetValue(name, out foundSimulateButton) && foundSimulateButton.Pressed)
                return true;
            if (!UseNonMobileInput())
            {
                if (IsKeyFromSettingActivated(name, GetKey))
                    return true;
            }
        }
        return false;
    }

#if USE_REWIRED
    public static bool GetButtonDown(string name, int playerId = 0)
#else
    public static bool GetButtonDown(string name)
#endif
    {
        if (string.IsNullOrEmpty(name))
            return false;

        // Try get input by rewired system
#if USE_REWIRED
        try
        {
            Rewired.Player player = Rewired.ReInput.players.GetPlayer(playerId);
            if (player.GetButtonDown(name))
                return true;
        }
        catch { }
#endif

#if ENABLE_INPUT_SYSTEM
        if (TryGetInputAction(name, out InputAction inputAction))
        {
            if (inputAction.WasPressedThisFrame())
                return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (UseNonMobileInput())
        {
            try
            {
                if (Input.GetButtonDown(name))
                    return true;
            }
            catch { }
            if (IsKeyFromSettingActivated(name, GetKeyDown))
                return true;
        }
#endif

        if (UseMobileInput())
        {
            SimulateButton foundSimulateButton;
            if (simulateInputs.TryGetValue(name, out foundSimulateButton) && foundSimulateButton.ButtonDown)
                return true;
            if (!UseNonMobileInput())
            {
                if (IsKeyFromSettingActivated(name, GetKeyDown))
                    return true;
            }
        }
        return false;
    }

#if USE_REWIRED
    public static bool GetButtonUp(string name, int playerId = 0)
#else
    public static bool GetButtonUp(string name)
#endif
    {
        if (string.IsNullOrEmpty(name))
            return false;

        // Try get input by rewired system
#if USE_REWIRED
        try
        {
            Rewired.Player player = Rewired.ReInput.players.GetPlayer(playerId);
            if (player.GetButtonUp(name))
                return true;
        }
        catch { }
#endif

#if ENABLE_INPUT_SYSTEM
        if (TryGetInputAction(name, out InputAction inputAction))
        {
            if (inputAction.WasReleasedThisFrame())
                return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (UseNonMobileInput())
        {
            try
            {
                if (Input.GetButtonUp(name))
                    return true;
            }
            catch { }
            if (IsKeyFromSettingActivated(name, GetKeyUp))
                return true;
        }
#endif

        if (UseMobileInput())
        {
            SimulateButton foundSimulateButton;
            if (simulateInputs.TryGetValue(name, out foundSimulateButton) && foundSimulateButton.ButtonUp)
                return true;
            if (!UseNonMobileInput())
            {
                if (IsKeyFromSettingActivated(name, GetKeyUp))
                    return true;
            }
        }
        return false;
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

    public static void SetKeyDown(KeyCode key)
    {
        if (!simulateKeys.ContainsKey(key))
        {
            simulateKeys.Add(key, new SimulateButton());
        }
        simulateKeys[key].Press();
    }

    public static void SetKeyUp(KeyCode key)
    {
        if (!simulateKeys.ContainsKey(key))
        {
            simulateKeys.Add(key, new SimulateButton());
        }
        simulateKeys[key].Release();
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
#if ENABLE_INPUT_SYSTEM
        return Mouse.current.position.ReadValue();
#else
        return Input.mousePosition;
#endif
    }

    public static bool GetMouseButtonDown(int button)
    {
#if ENABLE_INPUT_SYSTEM
        switch (button)
        {
            case 0:
                return Mouse.current.leftButton.wasPressedThisFrame;
            case 1:
                return Mouse.current.rightButton.wasPressedThisFrame;
            case 2:
                return Mouse.current.middleButton.wasPressedThisFrame;
        }
        return false;
#else
        return Input.GetMouseButtonDown(button);
#endif
    }

    public static bool GetMouseButtonUp(int button)
    {
#if ENABLE_INPUT_SYSTEM
        switch (button)
        {
            case 0:
                return Mouse.current.leftButton.wasReleasedThisFrame;
            case 1:
                return Mouse.current.rightButton.wasReleasedThisFrame;
            case 2:
                return Mouse.current.middleButton.wasReleasedThisFrame;
        }
        return false;
#else
        return Input.GetMouseButtonUp(button);
#endif
    }

    public static bool GetMouseButton(int button)
    {
#if ENABLE_INPUT_SYSTEM
        switch (button)
        {
            case 0:
                return Mouse.current.leftButton.isPressed;
            case 1:
                return Mouse.current.rightButton.isPressed;
            case 2:
                return Mouse.current.middleButton.isPressed;
        }
        return false;
#else
        return Input.GetMouseButton(button);
#endif
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
