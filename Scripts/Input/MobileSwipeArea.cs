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

    public bool IsDragging
    {
        get; private set;
    }

    private Graphic graphic;
    private Vector2? previousTouchPosition;
    private int pointerId;
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
        if (previousPointer != null)
            return;
        pointerId = eventData.pointerId;
        previousPointer = eventData;
        previousTouchPosition = null;
        IsDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (previousPointer == null || pointerId != eventData.pointerId)
            return;
        InputManager.UpdateMobileInputDragging();
        previousPointer = eventData;
        if (!previousTouchPosition.HasValue)
            previousTouchPosition = eventData.position;
        Vector2 pointerDelta = eventData.position - previousTouchPosition.Value;
        previousTouchPosition = eventData.position;
        UpdateVirtualAxes(new Vector2(pointerDelta.x * xSensitivity, pointerDelta.y * ySensitivity) * Time.deltaTime * 100f);
        lastDragFrame = Time.frameCount;
    }

    private void Update()
    {
        if (Time.frameCount > lastDragFrame && previousPointer != null)
            OnDrag(previousPointer);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData != null && eventData.pointerId != pointerId)
            return;
        IsDragging = false;
        previousPointer = null;
        UpdateVirtualAxes(Vector2.zero);
    }

    public void UpdateVirtualAxes(Vector2 value)
    {
        if (useAxisX)
            InputManager.SetAxis(axisXName, value.x);

        if (useAxisY)
            InputManager.SetAxis(axisYName, value.y);
    }
}