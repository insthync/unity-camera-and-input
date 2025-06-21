using System.Collections.Generic;
using UnityEngine;

namespace Insthync.CameraAndInput
{
    [ExecuteInEditMode]
    [DefaultExecutionOrder(int.MinValue)]
    public class FollowCamera : MonoBehaviour
    {
        public Camera targetCamera;
        public Transform target;
        public Vector3 targetOffset;
        public bool updateCameraOnUpdate = false;
        public bool updateCameraOnLateUpdate = true;
        public bool updateCameraOnFixedUpdate = false;
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
        public float minDistanceToPerformWallHitSpring = 1f;
        public float wallHitSpringPushForwardDistance = 0.5f;
        public float wallHitSpringRadius = 0.5f;
        public LayerMask wallHitLayerMask = -1;
        public QueryTriggerInteraction wallHitQueryTriggerInteraction = QueryTriggerInteraction.Ignore;

        // Properties
        public Transform CacheCameraTransform { get; private set; }

        public Camera CacheCamera
        {
            get { return targetCamera; }
        }

        private GameObject _targetFollower;
        public Vector3 _targetPosition;
        public Vector3 _targetUp;
        public float _targetYRotation = 0f;
        // Being used in Update and DrawGizmos functions
        private Ray _tempRay;
        private float _tempDistance;
        private Vector3 _debugFrom;
        private Vector3 _debugTo;

        protected virtual void OnDrawGizmos()
        {
#if UNITY_EDITOR
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_debugFrom, _debugTo);
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
            if (_targetFollower == null && Application.isPlaying)
                _targetFollower = new GameObject("_CameraTargetFollower");
            if (!updateCameraOnUpdate &&
                !updateCameraOnLateUpdate &&
                !updateCameraOnFixedUpdate)
                updateCameraOnLateUpdate = true;
        }

        private void OnDestroy()
        {
            if (_targetFollower != null)
                Destroy(_targetFollower);
        }

        protected virtual void Update()
        {
            if (updateCameraOnUpdate)
                UpdateCamera(Time.deltaTime);
        }

        protected virtual void LateUpdate()
        {
            if (updateCameraOnLateUpdate)
                UpdateCamera(Time.deltaTime);
        }

        protected virtual void FixedUpdate()
        {
            if (updateCameraOnFixedUpdate)
                UpdateCamera(Time.fixedDeltaTime);
        }

        protected virtual void UpdateCamera(float deltaTime)
        {
            if (Application.isPlaying)
            {
                if (target != null)
                {
                    if (!smoothFollow)
                    {
                        _targetFollower.transform.position = target.transform.position;
                        _targetFollower.transform.eulerAngles = target.transform.eulerAngles;
                    }
                    else
                    {
                        _targetFollower.transform.position = Vector3.Lerp(_targetFollower.transform.position, target.transform.position, followSmoothing * deltaTime);
                        _targetFollower.transform.eulerAngles = target.transform.eulerAngles;
                    }
                    _targetPosition = _targetFollower.transform.position;
                    _targetUp = _targetFollower.transform.up;
                    _targetYRotation = _targetFollower.transform.eulerAngles.y;
                }
            }
            else
            {
#if UNITY_EDITOR
                PrepareComponents();
                if (target == null)
                {
                    _targetPosition = Vector3.zero;
                    _targetUp = Vector3.up;
                    _targetYRotation = 0f;
                }
                else
                {
                    _targetPosition = target.position;
                    _targetUp = target.up;
                    _targetYRotation = target.eulerAngles.y;
                }
#endif
            }

            // Update target position by offsets
            _targetPosition += (targetOffset.x * CacheCameraTransform.right) + (targetOffset.y * _targetUp) + (targetOffset.z * CacheCameraTransform.forward);

            if (zoomByAspectRatio)
            {
                float windowAspect = CacheCamera.aspect;
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

            float wantedYRotation = useTargetYRotation ? _targetYRotation : yRotation;

            // Convert the angle into a rotation
            Quaternion wantedRotation = Quaternion.Euler(xRotation, wantedYRotation, 0);

            // Set the position of the camera on the x-z plane to:
            // distance meters behind the target
            Vector3 wantedPosition = _targetPosition - (wantedRotation * Vector3.forward * zoomDistance);

            if (enableWallHitSpring)
            {
                float nearest = float.MaxValue;
                // Direction from the target to the camera
                Vector3 directionToCamera = wantedRotation * -Vector3.forward;
                _tempRay = new Ray(_targetPosition, directionToCamera);
                _tempDistance = Vector3.Distance(_targetPosition, wantedPosition);
                RaycastHit[] hits = Physics.SphereCastAll(_tempRay, wallHitSpringRadius, _tempDistance, wallHitLayerMask, wallHitQueryTriggerInteraction);
                foreach (var hit in hits)
                {
                    if (hit.distance < minDistanceToPerformWallHitSpring || hit.distance >= nearest)
                        continue;
                    nearest = hit.distance;
                    Vector3 collisionPointWithOffset = _tempRay.origin + (directionToCamera.normalized * (hit.distance - wallHitSpringPushForwardDistance));
                    if (Vector3.Distance(_targetPosition, collisionPointWithOffset) < _tempDistance)
                    {
                        wantedPosition = collisionPointWithOffset;
                    }
                }
            }

            // Update position & rotation
            CacheCameraTransform.position = wantedPosition;
            CacheCameraTransform.rotation = wantedRotation;

            _debugFrom = _targetPosition;
            _debugTo = wantedPosition;
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
}
