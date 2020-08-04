using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MobilePinchArea : MonoBehaviour, IMobileInputArea
{
    public string axisName = "Mouse ScrollWheel";
    [SerializeField]
    private float sensitivity = 1f;

    public bool IsZooming
    {
        get; private set;
    }

    private Graphic graphic;
    private Vector2 previousTouchPosition1;
    private Vector2 previousTouchPosition2;
    private List<Touch> touches = new List<Touch>();
    private List<RaycastResult> raycastResults = new List<RaycastResult>();

    private void Awake()
    {
        graphic = GetComponent<Graphic>();
        graphic.raycastTarget = true;
    }

    private void OnDisable()
    {
        InputManager.SetAxis(axisName, 0f);
    }

    public void Update()
    {
        if (Application.isMobilePlatform)
            UpdateMobile();
        else if (!Application.isConsolePlatform)
            UpdateStandalone();
    }

    private void UpdateStandalone()
    {
        PointerEventData tempPointer;
        bool hasPointer = false;
        tempPointer = new PointerEventData(EventSystem.current);
        tempPointer.position = Input.mousePosition;
        EventSystem.current.RaycastAll(tempPointer, raycastResults);
        if (raycastResults != null && raycastResults.Count > 0)
        {
            if (raycastResults[0].gameObject == gameObject)
            {
                if (!IsZooming && Input.GetMouseButton(1))
                {
                    OnPointerDown(Input.mousePosition, -Input.mousePosition);
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
            OnZoom_Standalone(Input.mousePosition, -Input.mousePosition);
        }
    }

    private void UpdateMobile()
    {
        PointerEventData tempPointer;
        touches.Clear();
        for (int i = 0; i < Input.touchCount; ++i)
        {
            tempPointer = new PointerEventData(EventSystem.current);
            tempPointer.position = Input.touches[i].position;
            EventSystem.current.RaycastAll(tempPointer, raycastResults);
            if (raycastResults != null && raycastResults.Count == 1)
            {
                if (raycastResults[0].gameObject == gameObject &&
                    !MobileMovementJoystick.JoystickTouches.Contains(Input.touches[i].fingerId))
                    touches.Add(Input.touches[i]);
            }
        }

        if (touches.Count < 2)
        {
            if (IsZooming)
                OnPointerUp();
            return;
        }

        if (touches[1].phase == TouchPhase.Began && !IsZooming)
            OnPointerDown(touches[0].position, touches[1].position);

        if (touches[0].phase == TouchPhase.Moved ||
            touches[0].phase == TouchPhase.Stationary ||
            touches[1].phase == TouchPhase.Moved ||
            touches[1].phase == TouchPhase.Stationary)
            OnZoom_Mobile(touches[0], touches[1]);
    }

    private void OnPointerDown(Vector2 pointerPosition1, Vector2 pointerPosition2)
    {
        IsZooming = true;
        previousTouchPosition1 = pointerPosition1;
        previousTouchPosition2 = pointerPosition2;
        InputManager.SetAxis(axisName, 0f);
    }

    private void OnPointerUp()
    {
        IsZooming = false;
        InputManager.SetAxis(axisName, 0f);
    }

    private void OnZoom_Standalone(Vector2 pointerPosition1, Vector2 pointerPosition2)
    {
        if (!IsZooming)
            return;
        // Find the magnitude of the vector (the distance) between the touches in each frame.
        float prevTouchDeltaMag = (previousTouchPosition1 - previousTouchPosition2).magnitude;
        float touchDeltaMag = (pointerPosition1 - pointerPosition2).magnitude;
        // Find the difference in the distances between each frame.
        float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
        // Set previous touch position to use next frame
        previousTouchPosition1 = pointerPosition1;
        previousTouchPosition2 = pointerPosition2;
        // Update virtual axes
        InputManager.SetAxis(axisName, deltaMagnitudeDiff * sensitivity * Time.deltaTime * 100f);
    }

    private void OnZoom_Mobile(Touch touch1, Touch touch2)
    {
        if (!IsZooming)
            return;
        // Find the position in the previous frame of each touch.
        Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
        Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;
        // Find the magnitude of the vector (the distance) between the touches in each frame.
        float prevTouchDeltaMag = (touch1PrevPos - touch2PrevPos).magnitude;
        float touchDeltaMag = (touch1.position - touch2.position).magnitude;
        // Find the difference in the distances between each frame.
        float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
        // Update virtual axes
        InputManager.SetAxis(axisName, deltaMagnitudeDiff * sensitivity * Time.deltaTime * 100f);
    }
}
