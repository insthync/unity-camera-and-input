using UnityEngine;

[ExecuteInEditMode]
public class FollowCamera : MonoBehaviour
{
    private Transform cacheTransform;
    public Transform CacheTransform
    {
        get
        {
            if (cacheTransform == null)
                cacheTransform = GetComponent<Transform>();
            return cacheTransform;
        }
    }

    // The target we are following
    public Transform target;
    public Vector3 targetOffset;
    [Header("Follow")]
    public float damping = 10.0f;
    public bool dontSmoothFollow;
    [Header("Look at")]
    public float lookAtDamping = 2.0f;
    public bool dontSmoothLookAt;
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

    // Improve Garbage collector
    private Vector3 targetPosition;
    private float targetYRotation;
    private Vector3 wantedPosition;
    private float wantedYRotation;
    private Quaternion currentRotation;
    private Quaternion lookAtRotation;
    private float targetaspect;
    private float windowaspect;
    private float scaleheight;
    private float diffScaleHeight;

    private void Update()
    {
        targetPosition = target == null ? Vector3.zero : target.position;
        targetYRotation = target == null ? 0 : target.eulerAngles.y;
        wantedPosition = targetPosition + targetOffset;
        wantedYRotation = useTargetYRotation ? targetYRotation : yRotation;

        // Convert the angle into a rotation
        currentRotation = Quaternion.Euler(xRotation, wantedYRotation, 0);

        // Set the position of the camera on the x-z plane to:
        // distance meters behind the target
        wantedPosition -= currentRotation * Vector3.forward * zoomDistance;

        // Update position
        if (!dontSmoothFollow)
            CacheTransform.position = Vector3.Slerp(CacheTransform.position, wantedPosition, damping * Time.deltaTime);
        else
            CacheTransform.position = wantedPosition;

        lookAtRotation = Quaternion.LookRotation(targetPosition + targetOffset - CacheTransform.position);
        // Always look at the target
        if (!dontSmoothLookAt)
            CacheTransform.rotation = Quaternion.Slerp(CacheTransform.rotation, lookAtRotation, lookAtDamping * Time.deltaTime);
        else
            CacheTransform.rotation = lookAtRotation;

        if (zoomByAspectRatio)
        {
            targetaspect = zoomByAspectRatioWidth / zoomByAspectRatioHeight;
            windowaspect = (float)Screen.width / (float)Screen.height;
            scaleheight = windowaspect / targetaspect;
            diffScaleHeight = 1 - scaleheight;
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
            Update();
#endif
    }
}
