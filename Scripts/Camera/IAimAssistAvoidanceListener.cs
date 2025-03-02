using UnityEngine;

namespace Insthync.CameraAndInput
{
    public interface IAimAssistAvoidanceListener
    {
        bool AvoidAimAssist(RaycastHit hitInfo);
    }
}
