using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(MobileSwipeArea))]
[RequireComponent(typeof(MobilePinchArea))]
public class MobileSwipeAndPinchArea : MonoBehaviour, IMobileInputArea
{
}
