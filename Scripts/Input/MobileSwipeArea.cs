using UnityEngine;
using UnityEngine.EventSystems;

public class MobileSwipeArea : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool useAxisX = true;
    public bool useAxisY = true;
    public string axisXName = "Horizontal";
    public string axisYName = "Vertical";
    public float xSensitivity = 1f;
    public float ySensitivity = 1f;

    private bool isDragging = false;
    private int touchId;
    private Vector3 previousTouchPosition;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isDragging)
            return;

        isDragging = true;
        previousTouchPosition = Input.mousePosition;
        touchId = eventData.pointerId;
        if (Application.isMobilePlatform)
            previousTouchPosition = Input.touches[touchId].position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging)
            return;
        if (eventData.pointerId != touchId)
            return;

        isDragging = false;
        touchId = -1;
        UpdateVirtualAxes(Vector3.zero);
    }

    private void Update()
    {
        if (!isDragging)
        {
            UpdateVirtualAxes(Vector3.zero);
            return;
        }

        var currentPosition = Input.mousePosition;
        if (Application.isMobilePlatform)
            currentPosition = Input.touches[touchId].position;
        Vector3 pointerDelta;
        pointerDelta = currentPosition - previousTouchPosition;
        previousTouchPosition = currentPosition;
        UpdateVirtualAxes(new Vector3(pointerDelta.x * xSensitivity, pointerDelta.y * ySensitivity, 0));
    }

    public void UpdateVirtualAxes(Vector3 value)
    {
        value = value.normalized;

        if (useAxisX)
            InputManager.SetAxis(axisXName, value.x);

        if (useAxisY)
            InputManager.SetAxis(axisYName, value.y);
    }
}
