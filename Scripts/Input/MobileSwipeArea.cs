using UnityEngine;
using UnityEngine.EventSystems;

public class MobileSwipeArea : MobileInputComponent, IPointerDownHandler, IPointerUpHandler
{
    public bool useAxisX = true;
    public bool useAxisY = true;
    public string axisXName = "Horizontal";
    public string axisYName = "Vertical";
    public float xSensitivity = 1f;
    public float ySensitivity = 1f;

    private bool isDragging = false;
    private int touchId = -1;
    private Vector2 previousTouchPosition;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isDragging)
            return;

        var touchId = eventData.pointerId;
        if (ContainsTouchId(touchId))
            return;

        AddTouchId(touchId);

        isDragging = true;
        previousTouchPosition = Input.mousePosition;
        this.touchId = touchId;
        if (Application.isMobilePlatform)
            previousTouchPosition = Input.touches[touchId].position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        if (eventData.pointerId != touchId)
            return;

        RemoveTouchId(touchId);

        isDragging = false;
        touchId = -1;
    }

    private void Update()
    {
        if (!isDragging)
        {
            UpdateVirtualAxes(Vector3.zero);
            return;
        }

        var currentPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        if (Application.isMobilePlatform)
            currentPosition = Input.touches[touchId].position;

        var pointerDelta = currentPosition - previousTouchPosition;
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
