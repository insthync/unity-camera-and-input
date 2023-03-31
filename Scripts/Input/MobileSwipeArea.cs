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

    public bool DisableInput { get; set; }

    private bool isDragging;
    public bool IsDragging
    {
        get => isDragging && !DisableInput; private set => isDragging = value;
    }

    private Graphic graphic;
    private Vector2? previousTouchPosition;
    private PointerEventData previousPointer;
    private int lastDragFrame;

    private void Awake()
    {
        graphic = GetComponent<Graphic>();
        graphic.raycastTarget = true;
    }

    private void OnDisable()
    {
        OnPointerUp(null);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (InputManager.touchedPointerIds.TryGetValue(eventData.pointerId, out GameObject touchedObject) && touchedObject != gameObject)
            return;
        if (previousPointer != null)
            return;
        previousPointer = eventData;
        previousTouchPosition = null;
        IsDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (previousPointer == null || previousPointer.pointerId != eventData.pointerId)
            return;
        previousPointer = eventData;
        if (!previousTouchPosition.HasValue)
            previousTouchPosition = eventData.position;
        Vector2 pointerDelta = eventData.position - previousTouchPosition.Value;
        previousTouchPosition = eventData.position;
        UpdateVirtualAxes(new Vector2(pointerDelta.x * xSensitivity, pointerDelta.y * ySensitivity) * Time.deltaTime * 100f);
        if (DisableInput)
            InputManager.UpdateMobileInputDragging();
        lastDragFrame = Time.frameCount;
    }

    private void Update()
    {
        if (Time.frameCount > lastDragFrame && previousPointer != null)
            OnDrag(previousPointer);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (previousPointer != null && eventData != null && previousPointer.pointerId != eventData.pointerId)
            return;
        IsDragging = false;
        previousPointer = null;
        UpdateVirtualAxes(Vector2.zero);
    }

    public void UpdateVirtualAxes(Vector2 value)
    {
        if (DisableInput)
            value = Vector2.zero;

        if (useAxisX)
            InputManager.SetAxis(axisXName, value.x);

        if (useAxisY)
            InputManager.SetAxis(axisYName, value.y);
    }
}