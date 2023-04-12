using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class InputSettingManager : MonoBehaviour
{
    [System.Serializable]
    public struct InputSetting
    {
        public string keyName;
        public KeyCode keyCode;
    }

    public static InputSettingManager Singleton { get; protected set; }

    [Tooltip("These settings will override Unity's input manager axes settings")]
    public InputSetting[] settings;
    public string settingsSaveKeyPrefix = "SETTING_KEY_BIND";
#if ENABLE_INPUT_SYSTEM
    public InputActionAsset inputActionAsset;
#endif

    internal readonly Dictionary<string, List<KeyCode>> Settings = new Dictionary<string, List<KeyCode>>();

    private void Awake()
    {
        if (Singleton != null)
        {
            Destroy(gameObject);
            return;
        }
        Singleton = GetComponent<InputSettingManager>();
        DontDestroyOnLoad(gameObject);
        Setup();
    }

    public void Setup()
    {
        if (settings != null && settings.Length > 0)
        {
            Settings.Clear();
            foreach (InputSetting setting in settings)
            {
                if (!Settings.ContainsKey(setting.keyName))
                    Settings[setting.keyName] = new List<KeyCode>();
                if (!Settings[setting.keyName].Contains(setting.keyCode))
                    Settings[setting.keyName].Add(LoadKeyBinding(setting.keyName, Settings[setting.keyName].Count - 1, setting.keyCode));
            }
        }
    }

    public void ResetSettings()
    {
        if (settings != null && settings.Length > 0)
        {
            Settings.Clear();
            foreach (InputSetting setting in settings)
            {
                if (!Settings.ContainsKey(setting.keyName))
                    Settings[setting.keyName] = new List<KeyCode>();
                if (!Settings[setting.keyName].Contains(setting.keyCode))
                    Settings[setting.keyName].Add(SaveKeyBinding(setting.keyName, Settings[setting.keyName].Count - 1, setting.keyCode));
            }
        }
    }

    public string GetSaveKey(string keyName, int index)
    {
        return $"{settingsSaveKeyPrefix}_{keyName}_{index}";
    }

    public KeyCode LoadKeyBinding(string keyName, int index, KeyCode defaultKey)
    {
        return (KeyCode)PlayerPrefs.GetInt(GetSaveKey(keyName, index), (int)defaultKey);
    }

    public KeyCode SaveKeyBinding(string keyName, int index, KeyCode key)
    {
        PlayerPrefs.SetInt(GetSaveKey(keyName, index), (int)key);
        PlayerPrefs.Save();
        return key;
    }

    public void SetKeySetting(string keyName, int index, KeyCode key)
    {
        if (!Settings.ContainsKey(keyName) || index >= Settings[keyName].Count)
            return;
        Settings[keyName][index] = SaveKeyBinding(keyName, index, key);
    }

    public KeyCode GetKeySetting(string keyName, int index)
    {
        if (!Settings.ContainsKey(keyName) || index >= Settings[keyName].Count)
            return KeyCode.None;
        return Settings[keyName][index];
    }
}
