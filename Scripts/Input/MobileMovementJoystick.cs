using UnityEngine;
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
    private bool isDragging = false;
    private int touchId;
    private Vector3 backgroundOffset;
    private Vector3 defaultControllerPosition;
    private Vector2 startDragPosition;

    private void Start()
    {
        if (movementBackground != null)
            backgroundOffset = movementBackground.position - movementController.position;
        defaultControllerPosition = movementController.position;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isDragging)
            return;

        var touchId = eventData.pointerId;
        if (ContainsTouchId(touchId))
            return;

        AddTouchId(touchId);

        isDragging = true;
        this.touchId = touchId;
        movementController.position = new Vector2(eventData.position.x, eventData.position.y);
        if (movementBackground != null)
            movementBackground.position = backgroundOffset + movementController.position;
        startDragPosition = movementController.position;
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
        movementController.position = defaultControllerPosition;
        if (movementBackground != null)
            movementBackground.position = backgroundOffset + movementController.position;
    }


    private void Update()
    {
        if (!isDragging)
        {
            UpdateVirtualAxes(Vector3.zero);
            return;
        }

        var newPos = Vector2.zero;

        var currentPosition = Input.mousePosition;
        if (Application.isMobilePlatform)
            currentPosition = Input.touches[touchId].position;

        var allowedPos = new Vector2(currentPosition.x, currentPosition.y) - startDragPosition;
        allowedPos = Vector2.ClampMagnitude(allowedPos, movementRange);

        if (useAxisX)
            newPos.x = allowedPos.x;

        if (useAxisY)
            newPos.y = allowedPos.y;

        var movePosition = new Vector2(startDragPosition.x + newPos.x, startDragPosition.y + newPos.y);
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
