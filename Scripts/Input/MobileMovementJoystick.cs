using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class MobileMovementJoystick : MonoBehaviour, IMobileInputArea, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    internal static readonly HashSet<int> JoystickTouches = new HashSet<int>();
    public int movementRange = 150;
    public bool useAxisX = true;
    public bool useAxisY = true;
    public bool useButtons = false;
    public string axisXName = "Horizontal";
    public string axisYName = "Vertical";
    public string[] buttonKeyNames;
    public bool fixControllerPosition;
    [SerializeField]
    private bool interactable = true;
    [SerializeField]
    private bool setAsLastSiblingOnDrag;
    [SerializeField]
    private bool hideWhileIdle;
    [Header("Controller Background")]
    [Tooltip("Container which showing as area that able to control movement")]
    [SerializeField]
    [FormerlySerializedAs("movementBackground")]
    private RectTransform controllerBackground;
    [SerializeField]
    private float backgroundAlphaWhileIdling = 1f;
    [SerializeField]
    private float backgroundAlphaWhileMoving = 1f;
    [Header("Controller Handler")]
    [Tooltip("This is the button to control movement")]
    [SerializeField]
    [FormerlySerializedAs("movementController")]
    private RectTransform controllerHandler;
    [SerializeField]
    private float handlerAlphaWhileIdling = 1f;
    [SerializeField]
    private float handlerAlphaWhileMoving = 1f;
    [Header("Events")]
    [SerializeField]
    private UnityEvent onPointerDown;
    [SerializeField]
    private UnityEvent onPointerUp;

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
    private CanvasGroup backgroundCanvasGroup;
    private CanvasGroup handlerCanvasGroup;

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
        if (controllerBackground != null)
        {
            // Prepare background offset, it will be used to calculate joystick movement
            backgroundOffset = controllerBackground.position - controllerHandler.position;
            // Get canvas group, will use it to change alpha later
            backgroundCanvasGroup = controllerBackground.GetComponent<CanvasGroup>();
            if (backgroundCanvasGroup != null)
                backgroundCanvasGroup.alpha = backgroundAlphaWhileIdling;
        }
        if (controllerHandler != null)
        {
            // Get canvas group, will use it to change alpha later
            handlerCanvasGroup = controllerHandler.GetComponent<CanvasGroup>();
            if (handlerCanvasGroup != null)
                handlerCanvasGroup.alpha = handlerAlphaWhileIdling;
        }
        defaultControllerLocalPosition = controllerHandler.localPosition;
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

        if (useButtons && buttonKeyNames != null)
        {
            foreach (string buttonKeyName in buttonKeyNames)
                InputManager.SetButtonDown(buttonKeyName);
        }

        onPointerDown.Invoke();

        if (fixControllerPosition)
            controllerHandler.localPosition = defaultControllerLocalPosition;
        else
            controllerHandler.position = eventData.position;

        if (SetAsLastSiblingOnDrag)
            transform.SetAsLastSibling();

        if (controllerBackground != null)
            controllerBackground.position = backgroundOffset + controllerHandler.position;

        if (backgroundCanvasGroup != null)
            backgroundCanvasGroup.alpha = backgroundAlphaWhileMoving;

        if (handlerCanvasGroup != null)
            handlerCanvasGroup.alpha = handlerAlphaWhileMoving;

        CurrentPosition = startDragPosition = controllerHandler.position;
        startDragLocalPosition = controllerHandler.localPosition;
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

        if (useButtons && buttonKeyNames != null)
        {
            foreach (string buttonKeyName in buttonKeyNames)
                InputManager.SetButtonUp(buttonKeyName);
        }

        onPointerUp.Invoke();

        controllerHandler.localPosition = defaultControllerLocalPosition;

        if (controllerBackground != null)
            controllerBackground.position = backgroundOffset + controllerHandler.position;

        if (backgroundCanvasGroup != null)
            backgroundCanvasGroup.alpha = backgroundAlphaWhileIdling;

        if (handlerCanvasGroup != null)
            handlerCanvasGroup.alpha = handlerAlphaWhileIdling;

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

        controllerHandler.localPosition = startDragLocalPosition + newOffsets;
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
