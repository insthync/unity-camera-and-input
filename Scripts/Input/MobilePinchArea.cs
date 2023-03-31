using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MobilePinchArea : MonoBehaviour, IMobileInputArea, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public string axisName = "Mouse ScrollWheel";
    [SerializeField]
    private float sensitivity = 1f;
    [SerializeField]
    private bool interactable = true;

    public bool Interactable
    {
        get { return interactable; }
        set { interactable = value; }
    }

    private bool _isPinching;
    public bool IsPinching
    {
        get => _isPinching && Interactable; private set => _isPinching = value;
    }

    private Graphic _graphic;
    private Vector2? _previousTouchPosition1;
    private Vector2? _previousTouchPosition2;
    private PointerEventData _previousPointer1;
    private PointerEventData _previousPointer2;
    private int _lastDragFrame;

    private List<RaycastResult> _raycastResults = new List<RaycastResult>();
    private MobileSwipeArea _swipeArea;

    private void Awake()
    {
        _graphic = GetComponent<Graphic>();
        _graphic.raycastTarget = true;
        _swipeArea = GetComponent<MobileSwipeArea>();
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
        if (_previousPointer1 == null)
            _previousPointer1 = eventData;
        else if (_previousPointer2 == null)
            _previousPointer2 = eventData;
        if (_previousPointer1 != null && _previousPointer2 != null)
        {
            _previousTouchPosition1 = null;
            _previousTouchPosition2 = null;
            if (_swipeArea != null)
                _swipeArea.Interactable = false;
            IsPinching = true;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!Application.isMobilePlatform)
            return;
        Vector2 pointerDelta1 = Vector2.zero;
        Vector2 pointerDelta2 = Vector2.zero;
        if (_previousPointer1 != null && _previousPointer1.pointerId == eventData.pointerId)
        {
            _previousPointer1 = eventData;
            if (!_previousTouchPosition1.HasValue)
                _previousTouchPosition1 = eventData.position;
            // Use previous position to find delta from last frame
            pointerDelta1 = eventData.position - _previousTouchPosition1.Value;
            // Set position to use next frame
            _previousTouchPosition1 = eventData.position;
        }
        if (_previousPointer2 != null && _previousPointer2.pointerId == eventData.pointerId)
        {
            _previousPointer2 = eventData;
            if (!_previousTouchPosition2.HasValue)
                _previousTouchPosition2 = eventData.position;
            // Use previous position to find delta from last frame
            pointerDelta2 = eventData.position - _previousTouchPosition2.Value;
            // Set position to use next frame
            _previousTouchPosition2 = eventData.position;
        }
        // Use 2 pointers to pinch
        if (_previousPointer1 == null || _previousPointer2 == null)
            return;
        Vector2 curPos1 = _previousTouchPosition1.Value;
        Vector2 curPos2 = _previousTouchPosition2.Value;
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
        // Update dragging state
        InputManager.UpdateMobileInputDragging();
        _lastDragFrame = Time.frameCount;
    }

    public void Update()
    {
        if (!Application.isMobilePlatform && !Application.isConsolePlatform)
        {
            UpdateStandalone();
            return;
        }

        if (Time.frameCount > _lastDragFrame && _previousPointer1 != null && _previousPointer2 != null)
        {
            OnDrag(_previousPointer1);
            OnDrag(_previousPointer2);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!Application.isMobilePlatform)
            return;
        if (eventData == null)
        {
            _previousPointer1 = null;
            _previousPointer2 = null;
        }
        else
        {
            if (_previousPointer1 != null && _previousPointer1.pointerId == eventData.pointerId)
                _previousPointer1 = null;
            if (_previousPointer2 != null && _previousPointer2.pointerId == eventData.pointerId)
                _previousPointer2 = null;
        }
        if (_previousPointer1 == null || _previousPointer2 == null)
        {
            UpdateVirtualAxis(0f);
            IsPinching = false;
            if (_swipeArea != null)
                _swipeArea.Interactable = true;
        }
    }

    private void UpdateStandalone()
    {
        if (_swipeArea != null)
            _swipeArea.Interactable = !IsPinching;
        PointerEventData tempPointer;
        bool hasPointer = false;
        tempPointer = new PointerEventData(EventSystem.current);
        tempPointer.position = InputManager.MousePosition();
        EventSystem.current.RaycastAll(tempPointer, _raycastResults);
        if (_raycastResults != null && _raycastResults.Count > 0)
        {
            if (_raycastResults[0].gameObject == gameObject)
            {
                if (!IsPinching && Input.GetMouseButton(1))
                {
                    OnPointerDown(InputManager.MousePosition(), -InputManager.MousePosition());
                    return;
                }
                hasPointer = true;
            }
        }

        if (!hasPointer || !Input.GetMouseButton(1))
        {
            if (IsPinching)
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
        IsPinching = true;
        _previousTouchPosition1 = pointerPosition1;
        _previousTouchPosition2 = pointerPosition2;
        UpdateVirtualAxis(0f);
    }

    private void OnPointerUp()
    {
        IsPinching = false;
        UpdateVirtualAxis(0f);
    }

    private void OnZoom_Standalone(Vector2 pointerPosition1, Vector2 pointerPosition2)
    {
        if (!IsPinching)
            return;
        // Find the magnitude of the vector (the distance) between the touches in each frame.
        float prevTouchDeltaMag = (_previousTouchPosition1.Value - _previousTouchPosition2.Value).magnitude;
        float touchDeltaMag = (pointerPosition1 - pointerPosition2).magnitude;
        // Find the difference in the distances between each frame.
        float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
        // Set previous touch position to use next frame
        _previousTouchPosition1 = pointerPosition1;
        _previousTouchPosition2 = pointerPosition2;
        // Update virtual axes
        UpdateVirtualAxisByDeltaMagnitudeDiff(deltaMagnitudeDiff);
    }

    private void UpdateVirtualAxisByDeltaMagnitudeDiff(float deltaMagnitudeDiff)
    {
        UpdateVirtualAxis(deltaMagnitudeDiff * sensitivity * Time.deltaTime * 100f);
    }

    public void UpdateVirtualAxis(float value)
    {
        if (!Interactable)
            value = 0f;
        InputManager.SetAxis(axisName, value);
    }
}
