using UnityEngine;

namespace Insthync.CameraAndInput
{
    [RequireComponent(typeof(MobileSwipeArea))]
    [RequireComponent(typeof(MobilePinchArea))]
    public class MobileSwipeAndPinchArea : MonoBehaviour, IMobileInputArea
    {
    }
}
