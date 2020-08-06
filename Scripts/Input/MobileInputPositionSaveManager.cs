using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileInputPositionSaveManager : MonoBehaviour
{
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
}
