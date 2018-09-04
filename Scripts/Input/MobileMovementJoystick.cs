﻿using UnityEngine;
using UnityEngine.EventSystems;

public class MobileMovementJoystick : MobileInputComponent, IPointerDownHandler, IPointerUpHandler
{
    public int movementRange = 150;
    public bool useAxisX = true;
    public bool useAxisY = true;
    public string axisXName = "Horizontal";
    public string axisYName = "Vertical";
    [Tooltip("Container which showing as area that able to control movement")]
    public RectTransform movementBackground;
    [Tooltip("This is the button to control movement")]
    public RectTransform movementController;
    private Vector3 backgroundOffset;
    private Vector3 defaultControllerPosition;
    private Vector2 startDragPosition;
    private int pointerId;
    private int correctPointerId;
    private bool isDragging;

    private void Start()
    {
        if (movementBackground != null)
            backgroundOffset = movementBackground.position - movementController.position;
        defaultControllerPosition = movementController.position;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerId = eventData.pointerId;
        movementController.position = GetPointerPosition(eventData.pointerId);
        if (movementBackground != null)
            movementBackground.position = backgroundOffset + movementController.position;
        startDragPosition = movementController.position;
        UpdateVirtualAxes(Vector3.zero);
        isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        movementController.position = defaultControllerPosition;
        if (movementBackground != null)
            movementBackground.position = backgroundOffset + movementController.position;
        isDragging = false;
    }

    private void Update()
    {
        if (!isDragging)
        {
            UpdateVirtualAxes(Vector3.zero);
            return;
        }

        var newPos = Vector2.zero;

        correctPointerId = pointerId;
        if (correctPointerId > Input.touchCount - 1)
            correctPointerId = Input.touchCount - 1;

        var currentPosition = GetPointerPosition(correctPointerId);

        var allowedPos = currentPosition - startDragPosition;
        allowedPos = Vector2.ClampMagnitude(allowedPos, movementRange);

        if (useAxisX)
            newPos.x = allowedPos.x;

        if (useAxisY)
            newPos.y = allowedPos.y;

        var movePosition = startDragPosition + newPos;
        movementController.position = movePosition;
        // Update virtual axes
        UpdateVirtualAxes((startDragPosition - movePosition) / movementRange * -1);
    }

    public void UpdateVirtualAxes(Vector2 value)
    {
        if (useAxisX)
            InputManager.SetAxis(axisXName, value.x);

        if (useAxisY)
            InputManager.SetAxis(axisYName, value.y);
    }
}
