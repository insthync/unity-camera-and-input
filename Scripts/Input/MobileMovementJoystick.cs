using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Insthync.CameraAndInput
{
    public class MobileMovementJoystick : MonoBehaviour, IMobileInputArea, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public enum EMode
        {
            Default,
            SwipeArea
        }
        [Header("Joystick Settings")]
        public int movementRange = 150;
        public bool fixControllerPosition = false;
        public bool useAxisX = true;
        public bool useAxisY = true;
        public string axisXName = "Horizontal";
        public string axisYName = "Vertical";
        public EMode mode = EMode.Default;
        public bool setAsLastSiblingOnDrag = true;
        public bool hideWhileIdle = false;
        [SerializeField]
        private bool interactable = true;

        [Header("Default Mode Settings")]
        public float axisXScale = 1f;
        public float axisYScale = 1f;

        [Header("Swipe Area Mode Settings")]
        public float xSensitivity = 1f;
        public float ySensitivity = 1f;

        [Header("Button Events Settings")]
        public bool useButtons = false;
        public string[] buttonKeyNames = new string[0];

        [Header("Controller Background")]
        [Tooltip("Container which showing as area that able to control movement")]
        [FormerlySerializedAs("movementBackground")]
        public RectTransform controllerBackground = null;
        [Range(0f, 1f)]
        public float backgroundAlphaWhileIdling = 1f;
        [Range(0f, 1f)]
        public float backgroundAlphaWhileMoving = 1f;

        [Header("Controller Handler")]
        [Tooltip("This is the button to control movement")]
        [FormerlySerializedAs("movementController")]
        public RectTransform controllerHandler = null;
        [Range(0f, 1f)]
        public float handlerAlphaWhileIdling = 1f;
        [Range(0f, 1f)]
        public float handlerAlphaWhileMoving = 1f;

        [Header("Toggling")]
        public RectTransform controllerToggler = null;
        public float toggleAngleRangeMin = 75f;
        public float toggleAngleRangeMax = 105f;
        public float toggleDistanceRangeMin = 90f;
        public string[] toggleKeyNames = new string[0];
        public GameObject[] toggleSigns = new GameObject[0];
        public GameObject[] unToggleSigns = new GameObject[0];

        [Header("Events")]
        public UnityEvent onPointerDown = new UnityEvent();
        public UnityEvent onPointerUp = new UnityEvent();
        public UnityEvent onToggleOn = new UnityEvent();
        public UnityEvent onToggleOff = new UnityEvent();

        public bool Interactable
        {
            get { return interactable; }
            set { interactable = value; }
        }

        private bool _isDragging;
        public bool IsDragging
        {
            get => _isDragging && Interactable; private set => _isDragging = value;
        }
        public Vector2 CurrentPosition { get; private set; }
        public Vector2 Movement { get; private set; }
        public bool PreToggled { get; private set; }
        public bool IsToggled { get; private set; }

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
        private PointerEventData _previousPointer;
        private int _lastDragFrame;
        private bool _isResettingSiblingIndex;
        private bool _prevToggled;

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
                controllerHandler.anchoredPosition = Vector2.zero;
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
                controllerBackground.anchoredPosition = Vector2.zero;
                // Prepare background offset, it will be used to calculate joystick movement
                _backgroundOffset = controllerBackground.position - controllerHandler.position;
                // Get canvas group, will use it to change alpha later
                _backgroundCanvasGroup = controllerBackground.GetComponent<CanvasGroup>();
                if (_backgroundCanvasGroup != null)
                    _backgroundCanvasGroup.alpha = backgroundAlphaWhileIdling;
            }
            if (controllerToggler != null)
                controllerToggler.gameObject.SetActive(false);
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

        private void OnEnable()
        {
            if (_isResettingSiblingIndex)
            {
                _isResettingSiblingIndex = false;
                transform.SetSiblingIndex(_defaultSiblingIndex);
            }
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
            InputManager.touchedPointerIds[eventData.pointerId] = gameObject;

            // Simulate button pressing
            if (useButtons && buttonKeyNames != null)
            {
                foreach (string buttonKeyName in buttonKeyNames)
                {
                    InputManager.SetButtonDown(buttonKeyName);
                }
            }
            onPointerDown.Invoke();

            // Update toggling
            if (controllerToggler != null)
            {
                UpdateToggle(false);
                controllerToggler.gameObject.SetActive(false);
            }

            // Move transform
            if (setAsLastSiblingOnDrag)
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
            CurrentPosition = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_previousPointer == null || _previousPointer.pointerId != eventData.pointerId)
                return;
            _previousPointer = eventData;
            OnDrag(eventData.position, eventData.delta);
            IsDragging = true;
        }

        public void OnDrag(Vector2 pointerPosition, Vector2 pointerDelta)
        {
            CurrentPosition = pointerPosition;

            // Find distance from start position, it will be used to find move direction
            Vector2 distanceFromStartPosition = pointerPosition - _startDragPosition;
            distanceFromStartPosition = Vector2.ClampMagnitude(distanceFromStartPosition, movementRange);

            // Move the handler
            controllerHandler.localPosition = _startDragLocalPosition + distanceFromStartPosition;

            // Update virtual axes
            Vector2 movement;
            switch (mode)
            {
                case EMode.SwipeArea:
                    movement = new Vector2(pointerDelta.x * xSensitivity, pointerDelta.y * ySensitivity) * Time.deltaTime * 100f;
                    break;
                default:
                    movement = (_startDragPosition - (_startDragPosition + distanceFromStartPosition)) / movementRange * -1;
                    break;
            }
            UpdateVirtualAxes(movement);

            // Update toggling
            if (controllerToggler != null)
            {
                Vector2 direction = movement.normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                PreToggled = angle > toggleAngleRangeMin && angle < toggleAngleRangeMax && Vector3.Distance(pointerPosition, _startDragPosition) > toggleDistanceRangeMin;
                controllerToggler.gameObject.SetActive(PreToggled);
            }

            // Update dragging state
            InputManager.UpdateMobileInputDragging();
            _lastDragFrame = Time.frameCount;
        }

        private void Update()
        {
            if (IsToggled || PreToggled)
            {
                if (IsToggled)
                {
                    _startDragLocalPosition = controllerHandler.localPosition = _defaultControllerLocalPosition;
                    _startDragPosition = controllerHandler.position;
                    Vector2 delta = (Vector2)controllerToggler.position - _startDragPosition;
                    Vector2 dir = delta.normalized;
                    Vector2 newPosition = _startDragPosition + dir * movementRange;
                    controllerHandler.position = newPosition;
                    OnDrag(controllerToggler.position, delta);
                }
                // Toggled
                if (toggleKeyNames != null && toggleKeyNames.Length > 0)
                {
                    foreach (string toggleKeyName in toggleKeyNames)
                    {
                        InputManager.SetButtonDown(toggleKeyName);
                    }
                }
            }
            else if (_prevToggled)
            {
                // Untoggled
                if (toggleKeyNames != null && toggleKeyNames.Length > 0)
                {
                    foreach (string toggleKeyName in toggleKeyNames)
                    {
                        InputManager.SetButtonUp(toggleKeyName);
                    }
                }
            }
            _prevToggled = IsToggled || PreToggled;
            if (_prevToggled)
                return;
            if (IsDragging && Time.frameCount > _lastDragFrame)
                OnDrag(CurrentPosition, Vector2.zero);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_previousPointer != null && eventData != null && _previousPointer.pointerId != eventData.pointerId)
                return;
            if (_previousPointer != null)
                InputManager.touchedPointerIds.Remove(_previousPointer.pointerId);
            _previousPointer = null;

            // Simulate button pressing
            if (eventData != null)
            {
                if (useButtons && toggleKeyNames != null)
                {
                    foreach (string buttonKeyName in buttonKeyNames)
                    {
                        InputManager.SetButtonUp(buttonKeyName);
                    }
                }
                onPointerUp.Invoke();
            }

            // Reset transform sibling
            if (setAsLastSiblingOnDrag)
            {
                if (transform.parent.gameObject.activeInHierarchy)
                    transform.SetSiblingIndex(_defaultSiblingIndex);
                else
                    _isResettingSiblingIndex = true;
            }

            // Update toggling
            if (controllerToggler != null)
                UpdateToggle(controllerToggler.gameObject.activeSelf && RectTransformUtility.RectangleContainsScreenPoint(controllerToggler, eventData.position));

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
            if (!IsDragging && !IsToggled)
                value = Vector2.zero;

            Movement = value;

            if (useAxisX)
                InputManager.SetAxis(axisXName, value.x * (mode == EMode.SwipeArea ? 1f : axisXScale));

            if (useAxisY)
                InputManager.SetAxis(axisYName, value.y * (mode == EMode.SwipeArea ? 1f : axisYScale));
        }

        public void UpdateToggle(bool isOn)
        {
            IsToggled = isOn;
            if (isOn)
                onToggleOn.Invoke();
            else
                onToggleOff.Invoke();
            int i;
            for (i = 0; i < toggleSigns.Length; ++i)
            {
                toggleSigns[i].SetActive(isOn);
            }
            for (i = 0; i < unToggleSigns.Length; ++i)
            {
                unToggleSigns[i].SetActive(!isOn);
            }
            if (controllerToggler != null)
                controllerToggler.gameObject.SetActive(isOn);
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

        public void OnSetToggleOff(bool state)
        {
            if (!state)
            {
                if (controllerHandler != null)
                    controllerHandler.localPosition = _defaultControllerLocalPosition;

                // Reset background position
                if (controllerBackground != null)
                    controllerBackground.position = _backgroundOffset + controllerHandler.position;

                // Reset canvas alpha
                if (_backgroundCanvasGroup != null)
                    _backgroundCanvasGroup.alpha = backgroundAlphaWhileIdling;

                if (_handlerCanvasGroup != null)
                    _handlerCanvasGroup.alpha = handlerAlphaWhileIdling;

                SetIdleState();

                InputManager.SetAxis(axisXName, 0);
                InputManager.SetAxis(axisYName, 0);

                PreToggled = false;
                UpdateToggle(false);
            }
        }
    }
}
