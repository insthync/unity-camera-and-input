using UnityEngine;

[ExecuteInEditMode]
public class FollowCamera : MonoBehaviour
{

    // The target we are following
    public Transform target;
    public Vector3 targetOffset;
    [Range(0, 65)]
    public float damping = 2.0f;
    [Header("Rotation")]
    public float xRotation;
    public float yRotation;
    [Tooltip("If this is TRUE, will update Y-rotation follow target")]
    public bool useTargetYRotation;
    [Header("Zoom")]
    public float zoomDistance = 10.0f;
    [Header("Zoom by ratio (Currently work well with landscape screen)")]
    public bool zoomByAspectRatio;
    public float zoomByAspectRatioWidth;
    public float zoomByAspectRatioHeight;
    public float zoomByAspectRatioMin;

    private void FixedUpdate()
    {
        var targetPosition = target == null ? Vector3.zero : target.position;
        var targetYRotation = target == null ? 0 : target.eulerAngles.y;
        var wantedPosition = targetPosition + targetOffset;
        var wantedYRotation = useTargetYRotation ? targetYRotation : yRotation;

        // Convert the angle into a rotation
        var currentRotation = Quaternion.Euler(xRotation, wantedYRotation, 0);

        // Set the position of the camera on the x-z plane to:
        // distance meters behind the target
        wantedPosition -= currentRotation * Vector3.forward * zoomDistance;

        // Update position
        transform.position = Vector3.Lerp(transform.position, wantedPosition, damping * Time.fixedDeltaTime);

        // Always look at the target
        transform.LookAt(targetPosition + targetOffset);

        if (zoomByAspectRatio)
        {
            var targetaspect = zoomByAspectRatioWidth / zoomByAspectRatioHeight;
            var windowaspect = (float)Screen.width / (float)Screen.height;
            var scaleheight = windowaspect / targetaspect;
            var diffScaleHeight = 1 - scaleheight;
            if (diffScaleHeight < zoomByAspectRatioMin)
                diffScaleHeight = zoomByAspectRatioMin;
            zoomDistance = diffScaleHeight * 20f;
        }
    }

    private void LateUpdate()
    {
#if UNITY_EDITOR
        // Update camera when it's updating edit mode (not play mode)
        if (!Application.isPlaying && Application.isEditor)
            FixedUpdate();
#endif
    }
}
