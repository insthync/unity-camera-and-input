using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MobileSwipeArea : MonoBehaviour, IMobileInputArea
{
    public bool useAxisX = true;
    public bool useAxisY = true;
    public string axisXName = "Horizontal";
    public string axisYName = "Vertical";
    [SerializeField]
    private float xSensitivity = 1f;
    [SerializeField]
    private float ySensitivity = 1f;

    public bool IsDragging
    {
        get; private set;
    }

    private Graphic graphic;
    private Vector2 previousTouchPosition;
    private List<RaycastResult> raycastResults = new List<RaycastResult>();
    private List<Touch> touches = new List<Touch>();
    private int draggingFingerId;

    private void Awake()
    {
        graphic = GetComponent<Graphic>();
        graphic.raycastTarget = true;
    }

    private void OnDisable()
    {
        UpdateVirtualAxes(Vector2.zero);
    }

    public void Update()
    {
        if (Application.isMobilePlatform)
            UpdateMobile();
        else if (!Application.isConsolePlatform)
            UpdateStandalone();
    }

    private void UpdateStandalone()
    {
        if (!IsDragging && Input.GetMouseButton(0))
        {
            // Is it touched inside the touch area or not?
            PointerEventData tempPointer = new PointerEventData(EventSystem.current);
            tempPointer.position = Input.mousePosition;
            EventSystem.current.RaycastAll(tempPointer, raycastResults);
            if (raycastResults != null && raycastResults.Count == 1 && raycastResults[0].gameObject == gameObject)
            {
                // Yes, it is
                OnPointerDown(Input.mousePosition);
                return;
            }
        }

        if (IsDragging && !Input.GetMouseButton(0))
        {
            OnPointerUp();
            return;
        }

        OnDrag(Input.mousePosition);
    }

    private void UpdateMobile()
    {
        touches.Clear();
        PointerEventData tempPointer;
        for (int i = 0; i < Input.touchCount; ++i)
        {
            if (Input.touches[i].fingerId == draggingFingerId)
            {
                touches.Add(Input.touches[i]);
                continue;
            }
            // Is it touched inside the touch area or not?
            tempPointer = new PointerEventData(EventSystem.current);
            tempPointer.position = Input.touches[i].position;
            EventSystem.current.RaycastAll(tempPointer, raycastResults);
            if (raycastResults != null && raycastResults.Count == 1 && raycastResults[0].gameObject == gameObject &&
                !MobileMovementJoystick.JoystickTouches.Contains(Input.touches[i].fingerId))
            {
                // Yes, it is
                touches.Add(Input.touches[i]);
            }
        }

        if (touches.Count != 1)
        {
            if (IsDragging)
                OnPointerUp();
            return;
        }

        if (touches[0].phase == TouchPhase.Began && !IsDragging)
        {
            draggingFingerId = touches[0].fingerId;
            OnPointerDown(touches[0].position);
        }

        if (touches[0].phase == TouchPhase.Moved ||
            touches[0].phase == TouchPhase.Stationary)
            OnDrag(touches[0].position);
    }

    private void OnPointerDown(Vector2 pointerPosition)
    {
        IsDragging = true;
        previousTouchPosition = pointerPosition;
        UpdateVirtualAxes(Vector2.zero);
    }

    private void OnPointerUp()
    {
        IsDragging = false;
        UpdateVirtualAxes(Vector2.zero);
    }

    private void OnDrag(Vector2 pointerPosition)
    {
        if (!IsDragging)
            return;
        Vector2 pointerDelta = pointerPosition - previousTouchPosition;
        // Set previous touch position to use next frame
        previousTouchPosition = pointerPosition;
        // Update virtual axes
        UpdateVirtualAxes(new Vector2(pointerDelta.x * xSensitivity, pointerDelta.y * ySensitivity) * Time.deltaTime * 100f);
    }

    public void UpdateVirtualAxes(Vector2 value)
    {
        if (useAxisX)
            InputManager.SetAxis(axisXName, value.x);

        if (useAxisY)
            InputManager.SetAxis(axisYName, value.y);
    }
}
