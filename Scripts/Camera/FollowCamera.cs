using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

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

    // Job
    private TransformAccessArray followJobTransforms;
    private FollowCameraJob followJob;
    private JobHandle followJobHandle;

    // Improve Garbage collector
    private Vector3 targetPosition;
    private float targetYRotation;
    private Vector3 wantedPosition;
    private float wantedYRotation;
    private float targetaspect;
    private float windowaspect;
    private float scaleheight;
    private float diffScaleHeight;

    private void OnEnable()
    {
        followJobTransforms = new TransformAccessArray(new Transform[] { CacheTransform });
    }

    private void OnDisable()
    {
        followJobTransforms.Dispose();
        followJobHandle.Complete();
    }

    protected virtual void Update()
    {
        targetPosition = target == null ? Vector3.zero : target.position;
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

        followJob = new FollowCameraJob()
        {
            targetPosition = targetPosition,
            targetYRotation = targetYRotation,
            targetOffset = targetOffset,
            damping = damping,
            dontSmoothFollow = dontSmoothFollow,
            lookAtDamping = lookAtDamping,
            dontSmoothLookAt = dontSmoothLookAt,
            xRotation = xRotation,
            yRotation = yRotation,
            useTargetYRotation = useTargetYRotation,
            zoomDistance = zoomDistance,
            deltaTime = Time.deltaTime,
        };
        followJobHandle = followJob.Schedule(followJobTransforms);
        JobHandle.ScheduleBatchedJobs();
    }

    protected virtual void LateUpdate()
    {
#if UNITY_EDITOR
        // Update camera when it's updating edit mode (not play mode)
        if (!Application.isPlaying && Application.isEditor)
            Update();
#endif
        followJobHandle.Complete();
    }
}

public struct FollowCameraJob : IJobParallelForTransform
{
    public Vector3 targetPosition;
    public float targetYRotation;
    public Vector3 targetOffset;
    public float damping;
    public bool dontSmoothFollow;
    public float lookAtDamping;
    public bool dontSmoothLookAt;
    public float xRotation;
    public float yRotation;
    public bool useTargetYRotation;
    public float zoomDistance;
    public float deltaTime;

    private Vector3 wantedPosition;
    private float wantedYRotation;
    private Quaternion currentRotation;
    private Quaternion lookAtRotation;

    public void Execute(int index, TransformAccess transform)
    {
        wantedPosition = targetPosition + targetOffset;
        wantedYRotation = useTargetYRotation ? targetYRotation : yRotation;

        // Convert the angle into a rotation
        currentRotation = Quaternion.Euler(xRotation, wantedYRotation, 0);

        // Set the position of the camera on the x-z plane to:
        // distance meters behind the target
        wantedPosition -= currentRotation * Vector3.forward * zoomDistance;

        // Update position
        if (!dontSmoothFollow)
            transform.position = Vector3.Slerp(transform.position, wantedPosition, damping * deltaTime);
        else
            transform.position = wantedPosition;

        lookAtRotation = Quaternion.LookRotation(targetPosition + targetOffset - transform.position);
        // Always look at the target
        if (!dontSmoothLookAt)
            transform.rotation = Quaternion.Slerp(transform.rotation, lookAtRotation, lookAtDamping * deltaTime);
        else
            transform.rotation = lookAtRotation;
    }
}
