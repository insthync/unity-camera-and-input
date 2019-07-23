using UnityEngine;
using UnityEngine.EventSystems;

public class MobileMovementJoystick : MobileInputComponent, IPointerDownHandler, IPointerUpHandler
{
    public int movementRange = 150;
    public bool useAxisX = true;
    public bool useAxisY = true;
    public string axisXName = "Horizontal";
    public string axisYName = "Vertical";
    public bool fixControllerPosition;
    public bool SetAsLastSiblingOnDrag;
    [SerializeField]
    private bool interactable = true;
    [Tooltip("Container which showing as area that able to control movement")]
    public RectTransform movementBackground;
    [Tooltip("This is the button to control movement")]
    public RectTransform movementController;

    public bool Interactable
    {
        get { return interactable; }
        set { interactable = value; }
    }

    private Vector3 backgroundOffset;
    private Vector3 defaultControllerLocalPosition;
    private Vector2 startDragPosition;
    private int defaultSiblingIndex;
    private int pointerId;
    private int correctPointerId;
    private bool isDragging;

    private void Start()
    {
        if (movementBackground != null)
            backgroundOffset = movementBackground.position - movementController.position;
        defaultControllerLocalPosition = movementController.localPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!Interactable || isDragging)
            return;

        pointerId = eventData.pointerId;
        if (fixControllerPosition)
            movementController.localPosition = defaultControllerLocalPosition;
        else
            movementController.position = GetPointerPosition(eventData.pointerId);
        if (SetAsLastSiblingOnDrag)
        {
            defaultSiblingIndex = transform.GetSiblingIndex();
            transform.SetAsLastSibling();
        }
        if (movementBackground != null)
            movementBackground.position = backgroundOffset + movementController.position;
        startDragPosition = movementController.position;
        UpdateVirtualAxes(Vector3.zero);
        isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (SetAsLastSiblingOnDrag)
            transform.SetSiblingIndex(defaultSiblingIndex);
        movementController.localPosition = defaultControllerLocalPosition;
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

        Vector2 newPos = Vector2.zero;

        correctPointerId = pointerId;
        if (correctPointerId > Input.touchCount - 1)
            correctPointerId = Input.touchCount - 1;

        Vector2 currentPosition = GetPointerPosition(correctPointerId);

        Vector2 allowedPos = currentPosition - startDragPosition;
        allowedPos = Vector2.ClampMagnitude(allowedPos, movementRange);

        if (useAxisX)
            newPos.x = allowedPos.x;

        if (useAxisY)
            newPos.y = allowedPos.y;

        Vector2 movePosition = startDragPosition + newPos;
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
