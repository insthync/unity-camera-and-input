using UnityEngine;
using UnityEngine.UI;

public class MobileInputConfigManager : MonoBehaviour
{
    public static MobileInputConfigManager Instance { get; private set; }

    [Header("Editing UI Element")]
    public GameObject uiRoot;
    public Slider scaleSlider;
    public Slider alphaSlider;
    public bool turnOnEditModeOnEnable;

    private void OnEnable()
    {
        if (uiRoot)
        {
            uiRoot.SetActive(false);
        }
        if (turnOnEditModeOnEnable)
            TurnOnEditMode();
        else
            TurnOffEditMode();
        Instance = this;
    }

    private void OnDisable()
    {
        if (Instance == this)
            Instance = null;
    }

    public void TurnOnEditMode()
    {
        var comps = GetComponentsInChildren<MobileInputConfig>();
        foreach (var comp in comps)
        {
            comp.isEditMode = true;
        }
    }

    public void TurnOffEditMode()
    {
        var comps = GetComponentsInChildren<MobileInputConfig>();
        foreach (var comp in comps)
        {
            comp.isEditMode = false;
        }
    }

    public void LoadConfig()
    {
        var comps = FindObjectsOfType<MobileInputConfig>();
        foreach (var comp in comps)
        {
            comp.LoadPosition();
            comp.LoadScale();
            comp.LoadAlpha();
        }
    }

    public void SaveConfig()
    {
        var comps = GetComponentsInChildren<MobileInputConfig>();
        foreach (var comp in comps)
        {
            comp.SavePosition();
            comp.SaveScale();
            comp.SaveAlpha();
        }
        LoadConfig();
    }

    public void ResetConfig()
    {
        var comps = GetComponentsInChildren<MobileInputConfig>();
        foreach (var comp in comps)
        {
            comp.ResetPosition();
            comp.ResetScale();
            comp.ResetAlpha();
        }
    }

    public void SelectMobileInput(MobileInputConfig input)
    {
        if (!input)
        {
            DeselectMobileInput();
            return;
        }
        if (uiRoot)
        {
            uiRoot.SetActive(true);
        }
        if (scaleSlider)
        {
            scaleSlider.onValueChanged.RemoveAllListeners();
            scaleSlider.minValue = input.minScale;
            scaleSlider.maxValue = input.maxScale;
            scaleSlider.value = input.CurrentScale;
            scaleSlider.onValueChanged.AddListener(input.SetScale);
        }
        if (alphaSlider)
        {
            alphaSlider.onValueChanged.RemoveAllListeners();
            alphaSlider.minValue = input.minAlpha;
            alphaSlider.maxValue = input.maxAlpha;
            alphaSlider.value = input.CurrentAlpha;
            alphaSlider.onValueChanged.AddListener(input.SetAlpha);
        }
    }

    public void DeselectMobileInput()
    {
        if (uiRoot)
        {
            uiRoot.SetActive(false);
        }
    }
}
