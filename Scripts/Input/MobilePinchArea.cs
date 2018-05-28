using UnityEngine;
using UnityEngine.EventSystems;

public class MobilePinchArea : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public string axisName = "Mouse ScrollWheel";
    public float sensitivity = 1f;

    private bool isDragging = false;
    private int touchId1 = -1;
    private int touchId2 = -1;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isDragging)
            return;

        var touchId = eventData.pointerId;

        if (touchId1 == -1)
            touchId1 = touchId;
        else if (touchId2 == -1)
            touchId2 = touchId;

        if (touchId1 != -1 && touchId2 != -1)
            isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        if (eventData.pointerId == touchId1)
        {
            isDragging = false;
            touchId1 = -1;
        }

        if (eventData.pointerId == touchId2)
        {
            isDragging = false;
            touchId2 = -1;
        }
    }

    private void Update()
    {
        if (!isDragging)
        {
            InputManager.SetAxis(axisName, 0f);
            return;
        }

        // Store both touches.
        Touch touch1 = Input.touches[touchId1];
        Touch touch2 = Input.touches[touchId2];

        // Find the position in the previous frame of each touch.
        Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
        Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

        // Find the magnitude of the vector (the distance) between the touches in each frame.
        float prevTouchDeltaMag = (touch1PrevPos - touch2PrevPos).sqrMagnitude;
        float touchDeltaMag = (touch1.position - touch2.position).sqrMagnitude;

        // Find the difference in the distances between each frame.
        float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
        
        InputManager.SetAxis(axisName, deltaMagnitudeDiff * sensitivity);
    }
}
