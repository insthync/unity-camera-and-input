using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class MobileMovementJoystick : MonoBehaviour, IMobileInputArea, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    internal static readonly HashSet<int> JoystickTouches = new HashSet<int>();
    public enum EMode
    {
        Default,
        SwipeArea
    }
    [Header("Joystick Settings")]
    public int movementRange = 150;
    public bool fixControllerPosition;
    public bool useAxisX = true;
    public bool useAxisY = true;
    public string axisXName = "Horizontal";
    public string axisYName = "Vertical";
    [SerializeField]
    private EMode mode = EMode.Default;
    [SerializeField]
    private bool interactable = true;
    [SerializeField]
    private bool setAsLastSiblingOnDrag;
    [SerializeField]
    private bool hideWhileIdle;

    [Header("Default Mode Settings")]
    [SerializeField]
    private float axisXScale = 1f;
    [SerializeField]
    private float axisYScale = 1f;

    [Header("Swipe Area Mode Settings")]
    [SerializeField]
    private float xSensitivity = 1f;
    [SerializeField]
    private float ySensitivity = 1f;

    [Header("Button Events Settings")]
    [SerializeField]
    private bool useButtons = false;
    [SerializeField]
    private string[] buttonKeyNames = new string[0];

    [Header("Controller Background")]
    [Tooltip("Container which showing as area that able to control movement")]
    [SerializeField]
    [FormerlySerializedAs("movementBackground")]
    private RectTransform controllerBackground = null;
    [SerializeField]
    [Range(0f, 1f)]
    private float backgroundAlphaWhileIdling = 1f;
    [SerializeField]
    [Range(0f, 1f)]
    private float backgroundAlphaWhileMoving = 1f;

    [Header("Controller Handler")]
    [Tooltip("This is the button to control movement")]
    [SerializeField]
    [FormerlySerializedAs("movementController")]
    private RectTransform controllerHandler = null;
    [SerializeField]
    [Range(0f, 1f)]
    private float handlerAlphaWhileIdling = 1f;
    [SerializeField]
    [Range(0f, 1f)]
    private float handlerAlphaWhileMoving = 1f;

    [Header("Events")]
    [SerializeField]
    private UnityEvent onPointerDown = new UnityEvent();
    [SerializeField]
    private UnityEvent onPointerUp = new UnityEvent();

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
    private MobileInputConfig config;
    private bool swipping;

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
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
        config = GetComponent<MobileInputConfig>();
        if (config != null)
        {
            // Updating default canvas group alpha when loading new config
            config.onLoadAlpha += OnLoadAlpha;
        }
        defaultCanvasGroupAlpha = canvasGroup.alpha;
        defaultControllerLocalPosition = controllerHandler.localPosition;
        defaultSiblingIndex = transform.GetSiblingIndex();
        SetIdleState();
    }

    private void OnDestroy()
    {
        if (config != null)
            config.onLoadAlpha -= OnLoadAlpha;
    }

    private void OnDisable()
    {
        UpdateVirtualAxes(Vector2.zero);
        SetIdleState();
    }

    private void LateUpdate()
    {
        if (swipping)
        {
            // Clear axis movement after swipped
            UpdateVirtualAxes(Vector2.zero);
            swipping = false;
        }
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

        Vector2 pointerDelta = eventData.position - CurrentPosition; // Current Position actually is previous pointer position
        // Get cursor position (Also using it as previous touch position to use next frame)
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
        switch (mode)
        {
            case EMode.SwipeArea:
                UpdateVirtualAxes(new Vector2(pointerDelta.x * xSensitivity, pointerDelta.y * ySensitivity) * Time.deltaTime * 100f);
                swipping = true;
                break;
            default:
                UpdateVirtualAxes((startDragPosition - (startDragPosition + newOffsets)) / movementRange * -1);
                break;
        }
    }

    public void UpdateVirtualAxes(Vector2 value)
    {
        if (useAxisX)
            InputManager.SetAxis(axisXName, value.x * (mode == EMode.SwipeArea ? 1f : axisXScale));

        if (useAxisY)
            InputManager.SetAxis(axisYName, value.y * (mode == EMode.SwipeArea ? 1f : axisYScale));
    }

    private void SetIdleState()
    {
        if (canvasGroup)
            canvasGroup.alpha = hideWhileIdle ? 0f : defaultCanvasGroupAlpha;
    }

    private void SetDraggingState()
    {
        if (canvasGroup)
            canvasGroup.alpha = defaultCanvasGroupAlpha;
    }

    public void OnLoadAlpha(float alpha)
    {
        defaultCanvasGroupAlpha = alpha;
    }
}
