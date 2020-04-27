using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MobileSwipeArea : MonoBehaviour
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

    private Graphic graphic;
    private Vector2 previousTouchPosition;
    private List<Touch> touches = new List<Touch>();
    private List<RaycastResult> raycastResults = new List<RaycastResult>();

    private void Awake()
    {
        graphic = GetComponent<Graphic>();
        graphic.raycastTarget = true;
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
        PointerEventData tempPointer;
        bool hasPointer = false;
        tempPointer = new PointerEventData(EventSystem.current);
        tempPointer.position = Input.mousePosition;
        EventSystem.current.RaycastAll(tempPointer, raycastResults);
        if (raycastResults != null && raycastResults.Count > 0)
        {
            for (int j = 0; j < raycastResults.Count; ++j)
            {
                if (raycastResults[j].gameObject == gameObject)
                {
                    if (!isDragging && Input.GetMouseButton(0))
                    {
                        OnPointerDown(Input.mousePosition);
                        return;
                    }
                    hasPointer = true;
                    break;
                }
            }
        }

        if (!hasPointer || !Input.GetMouseButton(0))
        {
            if (isDragging)
                OnPointerUp();
            return;
        }

        if (hasPointer)
            OnDrag(Input.mousePosition);
    }

    private void UpdateMobile()
    {
        PointerEventData tempPointer;
        touches.Clear();
        for (int i = 0; i < Input.touchCount; ++i)
        {
            tempPointer = new PointerEventData(EventSystem.current);
            tempPointer.position = Input.touches[i].position;
            EventSystem.current.RaycastAll(tempPointer, raycastResults);
            if (raycastResults != null && raycastResults.Count > 0)
            {
                for (int j = 0; j < raycastResults.Count; ++j)
                {
                    if (raycastResults[j].gameObject == gameObject)
                    {
                        touches.Add(Input.touches[i]);
                        break;
                    }
                }
            }
        }

        if (touches.Count != 1)
        {
            if (isDragging)
                OnPointerUp();
            return;
        }

        switch (touches[0].phase)
        {
            case TouchPhase.Began:
                OnPointerDown(touches[0].position);
                break;
            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                OnDrag(touches[0].position);
                break;
            case TouchPhase.Ended:
                OnPointerUp();
                break;
        }
    }

    private void OnPointerDown(Vector2 pointerPosition)
    {
        isDragging = true;
        previousTouchPosition = pointerPosition;
    }

    private void OnPointerUp()
    {
        isDragging = false;
    }

    private void OnDrag(Vector2 pointerPosition)
    {
        // Store a touch position
        Vector2 currentPosition = pointerPosition;
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
