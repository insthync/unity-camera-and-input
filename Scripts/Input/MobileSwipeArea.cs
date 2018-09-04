﻿using UnityEngine;
using UnityEngine.EventSystems;

public class MobileSwipeArea : MobileInputComponent, IPointerDownHandler, IPointerUpHandler
{
    public bool useAxisX = true;
    public bool useAxisY = true;
    public string axisXName = "Horizontal";
    public string axisYName = "Vertical";
    public float xSensitivity = 1f;
    public float ySensitivity = 1f;

    private int pointerId;
    private bool isDragging = false;
    private Vector2 previousTouchPosition;

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerId = eventData.pointerId;
        previousTouchPosition = GetPointerPosition(pointerId);
        isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    private void Update()
    {
        if (!isDragging)
        {
            UpdateVirtualAxes(Vector3.zero);
            return;
        }

        var currentPosition = GetPointerPosition(pointerId);

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
