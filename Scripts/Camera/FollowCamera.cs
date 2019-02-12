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

    public Camera CacheCamera
    {
        get
        {
            if (targetCamera == null)
                targetCamera = GetComponent<Camera>();
            return targetCamera;
        }
    }

    private Transform cacheCameraTransform;
    public Transform CacheCameraTransform
    {
        get
        {
            if (cacheCameraTransform == null && CacheCamera != null)
                cacheCameraTransform = CacheCamera.GetComponent<Transform>();
            return cacheCameraTransform;
        }
    }

    public Camera targetCamera;
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
    private float targetaspect;
    private float windowaspect;
    private float scaleheight;
    private float diffScaleHeight;
    private float deltaTime;
    private Quaternion currentRotation;
    private Quaternion lookAtRotation;

    protected virtual void Update()
    {
        targetPosition = target == null ? Vector3.zero : target.position;
        Vector3 upVector = target == null ? Vector3.up : target.up;
        targetPosition += (targetOffset.x * CacheTransform.right) + (targetOffset.y * upVector) + (targetOffset.z * CacheTransform.forward);
        targetYRotation = target == null ? 0 : target.eulerAngles.y;

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

        if (CacheCamera != null && CacheCamera.orthographic)
            CacheCamera.orthographicSize = zoomDistance;

        deltaTime = Time.deltaTime;

        wantedPosition = targetPosition;
        wantedYRotation = useTargetYRotation ? targetYRotation : yRotation;

        // Convert the angle into a rotation
        currentRotation = Quaternion.Euler(xRotation, wantedYRotation, 0);

        // Set the position of the camera on the x-z plane to:
        // distance meters behind the target
        wantedPosition -= currentRotation * Vector3.forward * zoomDistance;

        // Update position
        if (!dontSmoothFollow)
            CacheTransform.position = Vector3.Slerp(CacheTransform.position, wantedPosition, damping * deltaTime);
        else
            CacheTransform.position = wantedPosition;

        lookAtRotation = Quaternion.LookRotation(targetPosition - CacheTransform.position);
        // Always look at the target
        if (!dontSmoothLookAt)
            CacheTransform.rotation = Quaternion.Slerp(CacheTransform.rotation, lookAtRotation, lookAtDamping * deltaTime);
        else
            CacheTransform.rotation = lookAtRotation;
    }

    protected virtual void LateUpdate()
    {
#if UNITY_EDITOR
        // Update camera when it's updating edit mode (not play mode)
        if (!Application.isPlaying && Application.isEditor)
            Update();
#endif
    }
}
