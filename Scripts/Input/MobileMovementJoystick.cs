using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class MobileMovementJoystick : MonoBehaviour, IMobileInputArea, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
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


    private bool _isDragging;
    public bool IsDragging
    {
        get => _isDragging && Interactable; private set => _isDragging = value;
    }

    public Vector2 CurrentPosition { get; private set; }

    private Vector3 _backgroundOffset;
    private Vector3 _defaultControllerLocalPosition;
    private Vector2 _startDragPosition;
    private Vector2 _startDragLocalPosition;
    private int _defaultSiblingIndex;
    private CanvasGroup _canvasGroup;
    private float _defaultCanvasGroupAlpha;
    private CanvasGroup _backgroundCanvasGroup;
    private CanvasGroup _handlerCanvasGroup;
    private MobileInputConfig _config;
    private Vector2? _previousTouchPosition;
    private PointerEventData _previousPointer;
    private int _lastDragFrame;

    private void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 1f;
        }
        if (controllerHandler != null)
        {
            controllerHandler.anchorMin = Vector2.one * 0.5f;
            controllerHandler.anchorMax = Vector2.one * 0.5f;
            controllerHandler.pivot = Vector2.one * 0.5f;
            // Get canvas group, will use it to change alpha later
            _handlerCanvasGroup = controllerHandler.GetComponent<CanvasGroup>();
            if (_handlerCanvasGroup != null)
                _handlerCanvasGroup.alpha = handlerAlphaWhileIdling;
        }
        if (controllerBackground != null && controllerBackground.transform == transform)
        {
            controllerBackground = null;
            Debug.LogWarning($"{this} `controllerBackground` must not the same game object with this component");
        }
        if (controllerBackground != null)
        {
            controllerBackground.anchorMin = Vector2.one * 0.5f;
            controllerBackground.anchorMax = Vector2.one * 0.5f;
            controllerBackground.pivot = Vector2.one * 0.5f;
            // Prepare background offset, it will be used to calculate joystick movement
            _backgroundOffset = controllerBackground.position - controllerHandler.position;
            // Get canvas group, will use it to change alpha later
            _backgroundCanvasGroup = controllerBackground.GetComponent<CanvasGroup>();
            if (_backgroundCanvasGroup != null)
                _backgroundCanvasGroup.alpha = backgroundAlphaWhileIdling;
        }
        _config = GetComponent<MobileInputConfig>();
        if (_config != null)
        {
            // Updating default canvas group alpha when loading new config
            _config.onLoadAlpha += OnLoadAlpha;
        }
        _defaultCanvasGroupAlpha = _canvasGroup.alpha;
        _defaultControllerLocalPosition = controllerHandler.localPosition;
        _defaultSiblingIndex = transform.GetSiblingIndex();
        SetIdleState();
    }

    private void OnDestroy()
    {
        if (_config != null)
            _config.onLoadAlpha -= OnLoadAlpha;
    }

    private void OnDisable()
    {
        OnPointerUp(null);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!Interactable)
            return;

        if (_previousPointer != null)
            return;
        _previousPointer = eventData;
        _previousTouchPosition = null;
        InputManager.touchedPointerIds[eventData.pointerId] = gameObject;

        // Simulate button pressing
        if (useButtons && buttonKeyNames != null)
        {
            foreach (string buttonKeyName in buttonKeyNames)
                InputManager.SetButtonDown(buttonKeyName);
        }
        onPointerDown.Invoke();

        // Move transform
        if (SetAsLastSiblingOnDrag)
            transform.SetAsLastSibling();

        // Set handler position
        if (fixControllerPosition)
            controllerHandler.localPosition = _defaultControllerLocalPosition;
        else
            controllerHandler.position = eventData.position;

        // Set background position
        if (controllerBackground != null)
            controllerBackground.position = _backgroundOffset + controllerHandler.position;

        // Set canvas alpha
        if (_backgroundCanvasGroup != null)
            _backgroundCanvasGroup.alpha = backgroundAlphaWhileMoving;

        if (_handlerCanvasGroup != null)
            _handlerCanvasGroup.alpha = handlerAlphaWhileMoving;

        _startDragPosition = controllerHandler.position;
        _startDragLocalPosition = controllerHandler.localPosition;
        SetDraggingState();
        IsDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_previousPointer == null || _previousPointer.pointerId != eventData.pointerId)
            return;
        _previousPointer = eventData;
        if (!_previousTouchPosition.HasValue)
            _previousTouchPosition = eventData.position;
        CurrentPosition = eventData.position;

        // Use previous position to find delta from last frame
        Vector2 pointerDelta = eventData.position - _previousTouchPosition.Value;
        // Set position to use next frame
        _previousTouchPosition = eventData.position;

        // Find distance from start position, it will be used to find move direction
        Vector2 distanceFromStartPosition = eventData.position - _startDragPosition;
        distanceFromStartPosition = Vector2.ClampMagnitude(distanceFromStartPosition, movementRange);

        // Move the handler
        controllerHandler.localPosition = _startDragLocalPosition + distanceFromStartPosition;

        // Update virtual axes
        switch (mode)
        {
            case EMode.SwipeArea:
                UpdateVirtualAxes(new Vector2(pointerDelta.x * xSensitivity, pointerDelta.y * ySensitivity) * Time.deltaTime * 100f);
                break;
            default:
                UpdateVirtualAxes((_startDragPosition - (_startDragPosition + distanceFromStartPosition)) / movementRange * -1);
                break;
        }
        // Update dragging state
        InputManager.UpdateMobileInputDragging();
        _lastDragFrame = Time.frameCount;
    }

    private void Update()
    {
        if (Time.frameCount > _lastDragFrame && _previousPointer != null)
            OnDrag(_previousPointer);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_previousPointer != null && eventData != null && _previousPointer.pointerId != eventData.pointerId)
            return;
        if (_previousPointer != null)
            InputManager.touchedPointerIds.Remove(_previousPointer.pointerId);
        _previousPointer = null;

        // Simulate button pressing
        if (useButtons && buttonKeyNames != null)
        {
            foreach (string buttonKeyName in buttonKeyNames)
            {
                InputManager.SetButtonUp(buttonKeyName);
            }
        }
        onPointerUp.Invoke();

        // Reset transform sibling
        if (eventData != null && SetAsLastSiblingOnDrag)
            transform.SetSiblingIndex(_defaultSiblingIndex);

        // Reset handler position
        controllerHandler.localPosition = _defaultControllerLocalPosition;

        // Reset background position
        if (controllerBackground != null)
            controllerBackground.position = _backgroundOffset + controllerHandler.position;

        // Reset canvas alpha
        if (_backgroundCanvasGroup != null)
            _backgroundCanvasGroup.alpha = backgroundAlphaWhileIdling;

        if (_handlerCanvasGroup != null)
            _handlerCanvasGroup.alpha = handlerAlphaWhileIdling;

        UpdateVirtualAxes(Vector3.zero);
        SetIdleState();
        IsDragging = false;
    }

    public void UpdateVirtualAxes(Vector2 value)
    {
        if (!IsDragging)
            return;

        if (useAxisX)
            InputManager.SetAxis(axisXName, value.x * (mode == EMode.SwipeArea ? 1f : axisXScale));

        if (useAxisY)
            InputManager.SetAxis(axisYName, value.y * (mode == EMode.SwipeArea ? 1f : axisYScale));
    }

    private void SetIdleState()
    {
        if (_canvasGroup)
            _canvasGroup.alpha = hideWhileIdle ? 0f : _defaultCanvasGroupAlpha;
    }

    private void SetDraggingState()
    {
        if (_canvasGroup)
            _canvasGroup.alpha = _defaultCanvasGroupAlpha;
    }

    public void OnLoadAlpha(float alpha)
    {
        _defaultCanvasGroupAlpha = alpha;
    }
}
