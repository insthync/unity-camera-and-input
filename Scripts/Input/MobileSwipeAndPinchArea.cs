using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(MobileSwipeArea))]
[RequireComponent(typeof(MobilePinchArea))]
public class MobileSwipeAndPinchArea : MonoBehaviour
{
    public MobileSwipeArea CacheMobileSwipeArea { get; private set; }
    public MobilePinchArea CacheMobilePinchArea { get; private set; }
    
    private void Awake()
    {
        CacheMobileSwipeArea = GetComponent<MobileSwipeArea>();
        CacheMobilePinchArea = GetComponent<MobilePinchArea>();
    }
}
