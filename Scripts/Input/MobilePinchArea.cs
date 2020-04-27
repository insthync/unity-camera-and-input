using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MobilePinchArea : MonoBehaviour
{
    public string axisName = "Mouse ScrollWheel";
    public float sensitivity = 1f;

    public bool isZooming
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
            for (int j = 0; j < raycastResults.Count; ++j)
            {
                if (raycastResults[j].gameObject == gameObject)
                {
                    if (!isZooming && Input.GetMouseButton(1))
                    {
                        OnPointerDown(Input.mousePosition, -Input.mousePosition);
                        return;
                    }
                    hasPointer = true;
                    break;
                }
            }
        }

        if (!hasPointer || !Input.GetMouseButton(1))
        {
            if (isZooming)
                OnPointerUp();
            return;
        }

        if (hasPointer)
        {
            OnZoom(Input.mousePosition, -Input.mousePosition);
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
            if (raycastResults != null && raycastResults.Count > 0)
            {
                for (int j = 0; j < raycastResults.Count; ++j)
                {
                    if (raycastResults[j].gameObject == gameObject)
                    {
                        touches.Add(Input.touches[i]);
                        break;
                    }
                }
            }
        }

        if (touches.Count < 2)
        {
            if (isZooming)
                OnPointerUp();
            return;
        }

        if (touches[touches.Count - 1].phase == TouchPhase.Began && !isZooming)
            OnPointerDown(touches[touches.Count - 1].position, touches[touches.Count - 2].position);

        if (touches[touches.Count - 1].phase == TouchPhase.Moved ||
            touches[touches.Count - 1].phase == TouchPhase.Stationary ||
            touches[touches.Count - 2].phase == TouchPhase.Began ||
            touches[touches.Count - 2].phase == TouchPhase.Stationary)
            OnZoom(touches[touches.Count - 1].position, touches[touches.Count - 2].position);
    }

    private void OnPointerDown(Vector2 pointerPosition1, Vector2 pointerPosition2)
    {
        isZooming = true;
        previousTouchPosition1 = pointerPosition1;
        previousTouchPosition2 = pointerPosition2;
    }

    private void OnPointerUp()
    {
        isZooming = false;
    }

    private void OnZoom(Vector2 pointerPosition1, Vector2 pointerPosition2)
    {
        if (!isZooming)
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
        InputManager.SetAxis(axisName, deltaMagnitudeDiff * sensitivity);
    }
}
