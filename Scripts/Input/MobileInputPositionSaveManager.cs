using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobileInputPositionSaveManager : MonoBehaviour
{
    [Header("Editing UI Element")]
    public GameObject uiRoot;
    public Slider scaleSlider;
    public Slider alphaSlider;

    public void TurnOnEditMode()
    {
        var comps = GetComponentsInChildren<MobileInputPositionSave>();
        foreach (var comp in comps)
        {
            comp.isEditMode = true;
        }
    }

    public void TurnOffEditMode()
    {
        var comps = GetComponentsInChildren<MobileInputPositionSave>();
        foreach (var comp in comps)
        {
            comp.isEditMode = false;
        }
    }

    public void LoadPositions()
    {
        var comps = GetComponentsInChildren<MobileInputPositionSave>();
        foreach (var comp in comps)
        {
            comp.LoadPosition();
        }
    }

    public void SavePositions()
    {
        var comps = GetComponentsInChildren<MobileInputPositionSave>();
        foreach (var comp in comps)
        {
            comp.SavePosition();
        }
    }

    public void ResetPositions()
    {
        var comps = GetComponentsInChildren<MobileInputPositionSave>();
        foreach (var comp in comps)
        {
            comp.ResetPosition();
        }
    }

    public void SelectMobileInput(MobileInputPositionSave input)
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
            scaleSlider.minValue = input.minScale;
            scaleSlider.maxValue = input.maxScale;
            scaleSlider.value = input.CurrentScale;
            scaleSlider.onValueChanged.RemoveAllListeners();
            scaleSlider.onValueChanged.AddListener(input.SetScale);
        }
        if (alphaSlider)
        {
            alphaSlider.minValue = input.minAlpha;
            alphaSlider.maxValue = input.maxAlpha;
            alphaSlider.value = input.CurrentAlpha;
            alphaSlider.onValueChanged.RemoveAllListeners();
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
