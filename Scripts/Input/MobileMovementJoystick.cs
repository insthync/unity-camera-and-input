using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MobileMovementJoystick : MonoBehaviour, IMobileInputArea, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    internal static readonly HashSet<int> JoystickTouches = new HashSet<int>();
    public int movementRange = 150;
    public bool useAxisX = true;
    public bool useAxisY = true;
    public string axisXName = "Horizontal";
    public string axisYName = "Vertical";
    public bool fixControllerPosition;
    [SerializeField]
    private bool interactable = true;
    [SerializeField]
    private bool setAsLastSiblingOnDrag;
    [SerializeField]
    private bool hideWhileIdle;
    [Tooltip("Container which showing as area that able to control movement")]
    public RectTransform movementBackground;
    [Tooltip("This is the button to control movement")]
    public RectTransform movementController;

    public bool Interactable
    {
        get { return interactable; }
        set { interactable = value; }
    }

    public bool SetAsLastSiblingOnDrag
    {
        get { return setAsLastSiblingOnDrag; }
        set { setAsLastSiblingOnDrag = value; }
    }

    public bool HideWhileIdle
    {
        get { return hideWhileIdle; }
        set { hideWhileIdle = value; }
    }

    public bool IsDragging
    {
        get; private set;
    }

    public Vector2 CurrentPosition
    {
        get; private set;
    }

    private Vector3 backgroundOffset;
    private Vector3 defaultControllerLocalPosition;
    private Vector2 startDragPosition;
    private Vector2 startDragLocalPosition;
    private int defaultSiblingIndex;
    private CanvasGroup canvasGroup;
    private float defaultCanvasGroupAlpha;

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            defaultCanvasGroupAlpha = 1f;
        }
        else
        {
            // Prepare defualt group alpha
            defaultCanvasGroupAlpha = canvasGroup.alpha;
        }
        if (movementBackground != null)
        {
            // Prepare background offset, it will be used to calculate joystick movement
            backgroundOffset = movementBackground.position - movementController.position;
        }
        defaultControllerLocalPosition = movementController.localPosition;
        defaultSiblingIndex = transform.GetSiblingIndex();
        SetIdleState();
    }

    private void OnDisable()
    {
        UpdateVirtualAxes(Vector2.zero);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!Interactable || IsDragging)
            return;

        if (fixControllerPosition)
            movementController.localPosition = defaultControllerLocalPosition;
        else
            movementController.position = eventData.position;

        if (SetAsLastSiblingOnDrag)
            transform.SetAsLastSibling();

        if (movementBackground != null)
            movementBackground.position = backgroundOffset + movementController.position;

        CurrentPosition = startDragPosition = movementController.position;
        startDragLocalPosition = movementController.localPosition;
        UpdateVirtualAxes(Vector3.zero);
        if (!JoystickTouches.Contains(eventData.pointerId))
            JoystickTouches.Add(eventData.pointerId);
        IsDragging = true;
        SetDraggingState();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (SetAsLastSiblingOnDrag)
            transform.SetSiblingIndex(defaultSiblingIndex);
        movementController.localPosition = defaultControllerLocalPosition;
        if (movementBackground != null)
            movementBackground.position = backgroundOffset + movementController.position;
        UpdateVirtualAxes(Vector3.zero);
        if (JoystickTouches.Contains(eventData.pointerId))
            JoystickTouches.Remove(eventData.pointerId);
        IsDragging = false;
        SetIdleState();
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (!IsDragging)
        {
            // It will be true while it's Interactable 
            return;
        }

        // Get cursor position
        CurrentPosition = eventData.position;

        Vector2 allowedOffsets = CurrentPosition - startDragPosition;
        allowedOffsets = Vector2.ClampMagnitude(allowedOffsets, movementRange);

        // Prepare offsets
        Vector2 newOffsets = Vector2.zero;
        if (useAxisX)
            newOffsets.x = allowedOffsets.x;
        if (useAxisY)
            newOffsets.y = allowedOffsets.y;

        movementController.localPosition = startDragLocalPosition + newOffsets;
        // Update virtual axes
        UpdateVirtualAxes((startDragPosition - (startDragPosition + newOffsets)) / movementRange * -1);
    }

    public void UpdateVirtualAxes(Vector2 value)
    {
        if (useAxisX)
            InputManager.SetAxis(axisXName, value.x);

        if (useAxisY)
            InputManager.SetAxis(axisYName, value.y);
    }

    private void SetIdleState()
    {
        canvasGroup.alpha = hideWhileIdle ? 0f : defaultCanvasGroupAlpha;
    }

    private void SetDraggingState()
    {
        canvasGroup.alpha = defaultCanvasGroupAlpha;
    }
}
