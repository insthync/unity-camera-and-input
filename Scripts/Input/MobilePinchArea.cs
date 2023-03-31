using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MobilePinchArea : MonoBehaviour, IMobileInputArea, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public string axisName = "Mouse ScrollWheel";
    [SerializeField]
    private float sensitivity = 1f;

    public bool DisableInput { get; set; }

    private bool isZooming;
    public bool IsZooming
    {
        get => isZooming && !DisableInput; private set => isZooming = value;
    }

    private Graphic graphic;
    private Vector2? previousTouchPosition1;
    private Vector2? previousTouchPosition2;
    private PointerEventData previousPointer1;
    private PointerEventData previousPointer2;
    private int lastDragFrame;

    private List<RaycastResult> raycastResults = new List<RaycastResult>();
    private MobileSwipeArea swipeArea;

    private void Awake()
    {
        graphic = GetComponent<Graphic>();
        graphic.raycastTarget = true;
        swipeArea = GetComponent<MobileSwipeArea>();
    }

    private void OnDisable()
    {
        OnPointerUp(null);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!Application.isMobilePlatform)
            return;
        if (InputManager.touchedPointerIds.TryGetValue(eventData.pointerId, out GameObject touchedObject) && touchedObject != gameObject)
            return;
        if (previousPointer1 == null)
            previousPointer1 = eventData;
        else if (previousPointer2 == null)
            previousPointer2 = eventData;
        if (previousPointer1 != null && previousPointer2 != null)
        {
            previousTouchPosition1 = null;
            previousTouchPosition2 = null;
            if (swipeArea != null)
                swipeArea.DisableInput = true;
            IsZooming = true;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!Application.isMobilePlatform)
            return;
        Vector2 pointerDelta1 = Vector2.zero;
        Vector2 pointerDelta2 = Vector2.zero;
        if (previousPointer1 != null && previousPointer1.pointerId == eventData.pointerId)
        {
            previousPointer1 = eventData;
            if (!previousTouchPosition1.HasValue)
                previousTouchPosition1 = eventData.position;
            pointerDelta1 = eventData.position - previousTouchPosition1.Value;
            previousTouchPosition1 = eventData.position;
        }
        if (previousPointer2 != null && previousPointer2.pointerId == eventData.pointerId)
        {
            previousPointer2 = eventData;
            if (!previousTouchPosition2.HasValue)
                previousTouchPosition2 = eventData.position;
            pointerDelta2 = eventData.position - previousTouchPosition2.Value;
            previousTouchPosition2 = eventData.position;
        }
        // Use 2 pointers to pinch
        if (previousPointer1 == null || previousPointer2 == null)
            return;
        Vector2 curPos1 = previousTouchPosition1.Value;
        Vector2 curPos2 = previousTouchPosition2.Value;
        // Find the position in the previous frame of each touch.
        Vector2 prevPos1 = curPos1 - pointerDelta1;
        Vector2 prevPos2 = curPos2 - pointerDelta2;
        // Find the magnitude of the vector (the distance) between the touches in each frame.
        float prevTouchDeltaMag = (prevPos1 - prevPos2).magnitude;
        float touchDeltaMag = (curPos1 - curPos2).magnitude;
        // Find the difference in the distances between each frame.
        float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
        // Update virtual axes
        UpdateVirtualAxisByDeltaMagnitudeDiff(deltaMagnitudeDiff);
        if (DisableInput)
            InputManager.UpdateMobileInputDragging();
        lastDragFrame = Time.frameCount;
    }

    public void Update()
    {
        if (!Application.isMobilePlatform && !Application.isConsolePlatform)
        {
            UpdateStandalone();
            return;
        }

        if (Time.frameCount > lastDragFrame && previousPointer1 != null && previousPointer2 != null)
        {
            OnDrag(previousPointer1);
            OnDrag(previousPointer2);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!Application.isMobilePlatform)
            return;
        if (eventData == null)
        {
            previousPointer1 = null;
            previousPointer2 = null;
        }
        else
        {
            if (previousPointer1 != null && previousPointer1.pointerId == eventData.pointerId)
                previousPointer1 = null;
            if (previousPointer2 != null && previousPointer2.pointerId == eventData.pointerId)
                previousPointer2 = null;
        }
        if (previousPointer1 == null || previousPointer2 == null)
        {
            UpdateVirtualAxis(0f);
            IsZooming = false;
            if (swipeArea != null)
                swipeArea.DisableInput = false;
        }
    }

    private void UpdateStandalone()
    {
        if (swipeArea != null)
            swipeArea.DisableInput = IsZooming;
        PointerEventData tempPointer;
        bool hasPointer = false;
        tempPointer = new PointerEventData(EventSystem.current);
        tempPointer.position = InputManager.MousePosition();
        EventSystem.current.RaycastAll(tempPointer, raycastResults);
        if (raycastResults != null && raycastResults.Count > 0)
        {
            if (raycastResults[0].gameObject == gameObject)
            {
                if (!IsZooming && Input.GetMouseButton(1))
                {
                    OnPointerDown(InputManager.MousePosition(), -InputManager.MousePosition());
                    return;
                }
                hasPointer = true;
            }
        }

        if (!hasPointer || !Input.GetMouseButton(1))
        {
            if (IsZooming)
                OnPointerUp();
            return;
        }

        if (hasPointer)
        {
            OnZoom_Standalone(InputManager.MousePosition(), -InputManager.MousePosition());
        }
    }

    private void OnPointerDown(Vector2 pointerPosition1, Vector2 pointerPosition2)
    {
        IsZooming = true;
        previousTouchPosition1 = pointerPosition1;
        previousTouchPosition2 = pointerPosition2;
        UpdateVirtualAxis(0f);
    }

    private void OnPointerUp()
    {
        IsZooming = false;
        UpdateVirtualAxis(0f);
    }

    private void OnZoom_Standalone(Vector2 pointerPosition1, Vector2 pointerPosition2)
    {
        if (!IsZooming)
            return;
        // Find the magnitude of the vector (the distance) between the touches in each frame.
        float prevTouchDeltaMag = (previousTouchPosition1.Value - previousTouchPosition2.Value).magnitude;
        float touchDeltaMag = (pointerPosition1 - pointerPosition2).magnitude;
        // Find the difference in the distances between each frame.
        float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
        // Set previous touch position to use next frame
        previousTouchPosition1 = pointerPosition1;
        previousTouchPosition2 = pointerPosition2;
        // Update virtual axes
        UpdateVirtualAxisByDeltaMagnitudeDiff(deltaMagnitudeDiff);
    }

    private void UpdateVirtualAxisByDeltaMagnitudeDiff(float deltaMagnitudeDiff)
    {
        UpdateVirtualAxis(deltaMagnitudeDiff * sensitivity * Time.deltaTime * 100f);
    }

    public void UpdateVirtualAxis(float value)
    {
        if (DisableInput)
            value = 0f;
        InputManager.SetAxis(axisName, value);
    }
}
