using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MobileSwipeArea : MonoBehaviour, IMobileInputArea, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public bool useAxisX = true;
    public bool useAxisY = true;
    public string axisXName = "Horizontal";
    public string axisYName = "Vertical";
    [SerializeField]
    private float xSensitivity = 1f;
    [SerializeField]
    private float ySensitivity = 1f;
    [SerializeField]
    private bool interactable = true;

    public bool Interactable
    {
        get { return interactable; }
        set { interactable = value; }
    }

    private bool _isSwiping;
    public bool IsSwiping
    {
        get => _isSwiping && Interactable; private set => _isSwiping = value;
    }

    private Graphic _graphic;
    private Vector2? _previousTouchPosition;
    private PointerEventData _previousPointer;
    private int _lastDragFrame;

    private void Awake()
    {
        _graphic = GetComponent<Graphic>();
        _graphic.raycastTarget = true;
    }

    private void OnDisable()
    {
        OnPointerUp(null);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (InputManager.touchedPointerIds.TryGetValue(eventData.pointerId, out GameObject touchedObject) && touchedObject != gameObject)
            return;
        if (_previousPointer != null)
            return;
        _previousPointer = eventData;
        _previousTouchPosition = null;
        IsSwiping = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_previousPointer == null || _previousPointer.pointerId != eventData.pointerId)
            return;
        _previousPointer = eventData;
        if (!_previousTouchPosition.HasValue)
            _previousTouchPosition = eventData.position;
        // Use previous position to find delta from last frame
        Vector2 pointerDelta = eventData.position - _previousTouchPosition.Value;
        // Set position to use next frame
        _previousTouchPosition = eventData.position;
        UpdateVirtualAxes(new Vector2(pointerDelta.x * xSensitivity, pointerDelta.y * ySensitivity) * Time.deltaTime * 100f);
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
        IsSwiping = false;
        _previousPointer = null;
        UpdateVirtualAxes(Vector2.zero);
    }

    public void UpdateVirtualAxes(Vector2 value)
    {
        if (!Interactable)
            value = Vector2.zero;

        if (useAxisX)
            InputManager.SetAxis(axisXName, value.x);

        if (useAxisY)
            InputManager.SetAxis(axisYName, value.y);
    }
}