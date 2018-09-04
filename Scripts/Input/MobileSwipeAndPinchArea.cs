using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(MobileSwipeArea))]
[RequireComponent(typeof(MobilePinchArea))]
public class MobileSwipeAndPinchArea : MobileInputComponent, IPointerDownHandler, IPointerUpHandler
{
    private MobileSwipeArea cacheMobileSwipeArea;
    public MobileSwipeArea CacheMobileSwipeArea
    {
        get
        {
            if (cacheMobileSwipeArea == null)
                cacheMobileSwipeArea = GetComponent<MobileSwipeArea>();
            return cacheMobileSwipeArea;
        }
    }

    private MobilePinchArea cacheMobilePinchArea;
    public MobilePinchArea CacheMobilePinchArea
    {
        get
        {
            if (cacheMobilePinchArea == null)
                cacheMobilePinchArea = GetComponent<MobilePinchArea>();
            return cacheMobilePinchArea;
        }
    }

    private int pointerCount = 0;

    public void OnPointerDown(PointerEventData eventData)
    {
        ++pointerCount;
        if (pointerCount == 2)
            CacheMobilePinchArea.OnPointerDown(eventData);
        if (pointerCount == 1)
        {
            CacheMobilePinchArea.OnPointerDown(eventData);
            CacheMobileSwipeArea.OnPointerDown(eventData);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        --pointerCount;
        if (pointerCount >= 1)
            CacheMobilePinchArea.OnPointerUp(eventData);
        if (pointerCount == 0)
            CacheMobileSwipeArea.OnPointerUp(eventData);
    }

    void Update()
    {
        if (pointerCount > 1)
        {
            CacheMobilePinchArea.enabled = true;
            CacheMobileSwipeArea.enabled = false;
        }
        else if (pointerCount > 0)
        {
            CacheMobilePinchArea.enabled = false;
            CacheMobileSwipeArea.enabled = true;
        }
        else
        {
            CacheMobilePinchArea.enabled = false;
            CacheMobileSwipeArea.enabled = false;
        }
    }
}
