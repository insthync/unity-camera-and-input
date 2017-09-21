using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MobileInputButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public string keyName;

    public void OnPointerDown(PointerEventData eventData)
    {
        InputManager.SetButtonDown(keyName);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        InputManager.SetButtonUp(keyName);
    }
}
