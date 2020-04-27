using UnityEngine;
using UnityEngine.EventSystems;

public class MobileSwipeArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public bool useAxisX = true;
    public bool useAxisY = true;
    public string axisXName = "Horizontal";
    public string axisYName = "Vertical";
    public float xSensitivity = 1f;
    public float ySensitivity = 1f;
    
    public bool isDragging
    {
        get; private set;
    }

    private Vector2 previousTouchPosition;

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = !Application.isMobilePlatform ? true : Input.touchCount == 1;
        if (isDragging)
            previousTouchPosition = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = !Application.isMobilePlatform ? false : Input.touchCount == 1;
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || (Application.isMobilePlatform && Input.touchCount != 1))
        {
            UpdateVirtualAxes(Vector3.zero);
            return;
        }

        // Store a touch.
        Vector2 currentPosition = eventData.position;
        Vector2 pointerDelta = currentPosition - previousTouchPosition;
        // Set previous touch position to use next frame
        previousTouchPosition = currentPosition;
        // Update virtual axes
        UpdateVirtualAxes(new Vector2(pointerDelta.x * xSensitivity, pointerDelta.y * ySensitivity));
    }

    public void UpdateVirtualAxes(Vector2 value)
    {
        if (useAxisX)
            InputManager.SetAxis(axisXName, value.x);

        if (useAxisY)
            InputManager.SetAxis(axisYName, value.y);
    }
}
