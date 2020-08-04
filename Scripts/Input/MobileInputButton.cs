using UnityEngine;
using UnityEngine.EventSystems;

public class MobileInputButton : MonoBehaviour, IMobileInputArea, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private string keyName;

    public void OnPointerDown(PointerEventData eventData)
    {
        InputManager.SetButtonDown(keyName);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        InputManager.SetButtonUp(keyName);
    }
}
