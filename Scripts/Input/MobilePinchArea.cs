using UnityEngine;
using UnityEngine.EventSystems;

public class MobilePinchArea : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public string axisName = "Mouse ScrollWheel";
    public float sensitivity = 1f;
    public bool isZooming
    {
        get; private set;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isZooming = Input.touchCount >= 2;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isZooming = Input.touchCount >= 2;
    }

    private void Update()
    {
        if (!isZooming || Input.touchCount < 2)
        {
            InputManager.SetAxis(axisName, 0f);
            return;
        }

        // Store both touches.
        Touch touch1 = Input.touches[0];
        Touch touch2 = Input.touches[1];

        // Find the position in the previous frame of each touch.
        Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
        Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

        // Find the magnitude of the vector (the distance) between the touches in each frame.
        float prevTouchDeltaMag = (touch1PrevPos - touch2PrevPos).magnitude;
        float touchDeltaMag = (touch1.position - touch2.position).magnitude;

        // Find the difference in the distances between each frame.
        float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
        
        InputManager.SetAxis(axisName, deltaMagnitudeDiff * sensitivity);
    }
}
