using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class FollowCamera : MonoBehaviour
{
    public Camera targetCamera;
    public Transform target;
    public Vector3 targetOffset;
    [Header("Follow smoothness")]
    public bool smoothFollow;
    public float followSmoothing = 10.0f;
    [Header("Rotation")]
    public float xRotation;
    public float yRotation;
    [Tooltip("If this is TRUE, will update Y-rotation follow target")]
    public bool useTargetYRotation;
    [Header("Zoom")]
    public float zoomDistance = 10.0f;
    [Header("Zoom by ratio")]
    public bool zoomByAspectRatio;
    public List<ZoomByAspectRatioSetting> zoomByAspectRatioSettings = new List<ZoomByAspectRatioSetting>()
    {
        new ZoomByAspectRatioSetting() { width = 16, height = 9, zoomDistance = 0.0001f },
        new ZoomByAspectRatioSetting() { width = 16, height = 10, zoomDistance = 1.75f },
        new ZoomByAspectRatioSetting() { width = 3, height = 2, zoomDistance = 3f },
        new ZoomByAspectRatioSetting() { width = 4, height = 3, zoomDistance = 5.5f },
        new ZoomByAspectRatioSetting() { width = 5, height = 4, zoomDistance = 7 },
    };
    [Header("Wall hit spring")]
    public bool enableWallHitSpring;
    public LayerMask wallHitLayerMask = -1;
    public QueryTriggerInteraction wallHitQueryTriggerInteraction = QueryTriggerInteraction.Ignore;

    // Properties
    public Transform CacheCameraTransform { get; private set; }

    public Camera CacheCamera
    {
        get { return targetCamera; }
    }

    private GameObject targetFollower;
    private Vector3 targetPosition;
    private Vector3 targetUp;
    private Quaternion targetRotation;
    private float targetYRotation;
    private Vector3 wantedPosition;
    private float wantedYRotation;
    private float windowAspect;
    private Quaternion wantedRotation;
    private Ray tempRay;
    private RaycastHit[] tempHits;
    private float tempDistance;

    protected virtual void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Gizmos.color = Color.red;
        Gizmos.DrawLine(tempRay.origin, tempRay.origin + tempRay.direction * tempDistance);
#endif
    }

    protected virtual void Awake()
    {
        PrepareComponents();
    }

    private void PrepareComponents()
    {
        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();
        CacheCameraTransform = CacheCamera.transform;
        if (targetFollower == null && Application.isPlaying)
            targetFollower = new GameObject("_CameraTargetFollower");
    }

    private void OnDestroy()
    {
        if (targetFollower != null)
            Destroy(targetFollower);
    }

    protected virtual void LateUpdate()
    {
        if (Application.isPlaying)
        {
            if (target != null)
            {
                if (!smoothFollow)
                {
                    targetFollower.transform.position = target.transform.position;
                    targetFollower.transform.eulerAngles = target.transform.eulerAngles;
                }
                else
                {
                    targetFollower.transform.position = Vector3.Lerp(targetFollower.transform.position, target.transform.position, followSmoothing * Time.deltaTime);
                    targetFollower.transform.eulerAngles = target.transform.eulerAngles;
                }
                targetPosition = targetFollower.transform.position;
                targetUp = targetFollower.transform.up;
                targetYRotation = targetFollower.transform.eulerAngles.y;
            }
        }
        else
        {
#if UNITY_EDITOR
            PrepareComponents();
            if (target == null)
            {
                targetPosition = Vector3.zero;
                targetUp = Vector3.up;
                targetYRotation = 0f;
            }
            else
            {
                targetPosition = target.position;
                targetUp = target.up;
                targetYRotation = target.eulerAngles.y;
            }
#endif
        }

        // Update target position by offsets
        targetPosition += (targetOffset.x * CacheCameraTransform.right) + (targetOffset.y * targetUp) + (targetOffset.z * CacheCameraTransform.forward);

        if (zoomByAspectRatio)
        {
            windowAspect = CacheCamera.aspect;
            zoomByAspectRatioSettings.Sort();
            foreach (ZoomByAspectRatioSetting data in zoomByAspectRatioSettings)
            {
                if (windowAspect + windowAspect * 0.025f > data.Aspect() &&
                    windowAspect - windowAspect * 0.025f < data.Aspect())
                {
                    zoomDistance = data.zoomDistance;
                    break;
                }
            }
        }

        if (zoomDistance == 0f)
            zoomDistance = 0.0001f;

        if (CacheCamera.orthographic)
            CacheCamera.orthographicSize = zoomDistance;

        wantedYRotation = useTargetYRotation ? targetYRotation : yRotation;

        // Convert the angle into a rotation
        targetRotation = Quaternion.Euler(xRotation, wantedYRotation, 0);
        wantedRotation = targetRotation;

        // Set the position of the camera on the x-z plane to:
        // distance meters behind the target
        wantedPosition = targetPosition - (wantedRotation * Vector3.forward * zoomDistance);

        if (enableWallHitSpring)
        {
            float nearest = float.MaxValue;
            tempRay = new Ray(targetPosition, wantedRotation * -Vector3.forward);
            tempDistance = Vector3.Distance(targetPosition, wantedPosition);
            tempHits = Physics.RaycastAll(tempRay, tempDistance, wallHitLayerMask, wallHitQueryTriggerInteraction);
            for (int i = 0; i < tempHits.Length; i++)
            {
                if (tempHits[i].distance < nearest)
                {
                    nearest = tempHits[i].distance;
                    wantedPosition = tempHits[i].point;
                }
            }
        }

        // Update position & rotation
        CacheCameraTransform.position = wantedPosition;
        CacheCameraTransform.rotation = wantedRotation;
    }

    [System.Serializable]
    public struct ZoomByAspectRatioSetting : System.IComparable
    {
        public int width;
        public int height;
        public float zoomDistance;

        public float Aspect()
        {
            return (float)width / (float)height;
        }

        public int CompareTo(object obj)
        {
            if (!(obj is ZoomByAspectRatioSetting))
                return 0;
            return Aspect().CompareTo(((ZoomByAspectRatioSetting)obj).Aspect());
        }
    }
}
