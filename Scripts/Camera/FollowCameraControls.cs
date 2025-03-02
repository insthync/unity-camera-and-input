﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Insthync.CameraAndInput
{
    [ExecuteInEditMode]
    [DefaultExecutionOrder(int.MinValue)]
    public class FollowCameraControls : FollowCamera
    {
        [Header("Controls")]
        public string xRotationAxisName = "Mouse Y";
        public string yRotationAxisName = "Mouse X";
        public string zoomAxisName = "Mouse ScrollWheel";
        public bool updateRotation = true;
        public bool updateRotationX = false;
        public bool updateRotationY = false;
        public bool updateZoom = true;

        [Header("X Rotation")]
        public bool limitXRotation;
        [Range(-360, 360)]
        public float minXRotation = 0;
        [Range(-360, 360)]
        public float maxXRotation = 0;
        public bool smoothRotateX;
        [FormerlySerializedAs("rotateXSmoothing")]
        public float rotateXDeacceleration = 10.0f;

        [Header("Y Rotation")]
        public bool limitYRotation;
        [Range(-360, 360)]
        public float minYRotation = 0;
        [Range(-360, 360)]
        public float maxYRotation = 0;
        public bool smoothRotateY;
        [FormerlySerializedAs("rotateYSmoothing")]
        public float rotateYDeacceleration = 10.0f;

        [Header("General Rotation Settings")]
        public float startXRotation;
        public float startYRotation;
        public float rotationSpeed = 5;
        [Range(0.1f, 1f)]
        public float rotationSpeedScale = 1;

        [Header("Zoom")]
        public bool limitZoomDistance;
        public float minZoomDistance;
        public float maxZoomDistance;
        public bool smoothZoom;
        [FormerlySerializedAs("zoomSmoothing")]
        public float zoomDeacceleration = 10.0f;

        [Header("General Zoom Settings")]
        public float startZoomDistance;
        public float zoomSpeed = 0.05f;
        [Range(0.1f, 1f)]
        public float zoomSpeedScale = 1;

        [Header("Aim Assist")]
        public bool enableAimAssist = false;
        public bool enableAimAssistX = true;
        public bool enableAimAssistY = true;
        public float aimAssistRadius = 0.5f;
        public float aimAssistMinDistanceFromFollowingTarget = 3f;
        public float aimAssistDistance = 10f;
        [Tooltip("Set both target layers and obstacle layers, it will be used in `SphereCastAll` function")]
        public LayerMask aimAssistLayerMask;
        [Tooltip("Set only obstacle layers, it will be used to check hitting object layer is an obstacle or not. If it is, it won't perform aim assisting")]
        public LayerMask aimAssistObstacleLayerMask = Physics.DefaultRaycastLayers;
        public float aimAssistXSpeed = 10f;
        public float aimAssistYSpeed = 10f;
        [Range(0f, 360f)]
        [FormerlySerializedAs("aimAssistAngleLessThan")]
        public float aimAssistMaxAngleFromFollowingTarget = 360f;

        [Header("Recoil")]
        public float recoilReturnSpeed = 2f;
        public float recoilSmoothing = 6f;

        [Header("Save Camera")]
        public bool isSaveCamera;
        public string savePrefsPrefix = "GAMEPLAY";

        public IAimAssistAvoidanceListener AimAssistAvoidanceListener { get; set; }
        public float XRotationVelocity { get; set; }
        public float YRotationVelocity { get; set; }
        public float ZoomVelocity { get; set; }

        private Vector3 _targetRecoilRotation;
        private Vector3 _currentRecoilRotation;
        // Being used in Update and DrawGizmos functions
        private RaycastHit _aimAssistCastHit;

        private void Start()
        {
            xRotation = startXRotation;
            yRotation = startYRotation;
            zoomDistance = startZoomDistance;

            if (isSaveCamera)
            {
                xRotation = PlayerPrefs.GetFloat(savePrefsPrefix + "_XRotation", xRotation);
                yRotation = PlayerPrefs.GetFloat(savePrefsPrefix + "_YRotation", yRotation);
                zoomDistance = PlayerPrefs.GetFloat(savePrefsPrefix + "_ZoomDistance", zoomDistance);
            }
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            if (isSaveCamera)
            {
                PlayerPrefs.SetFloat(savePrefsPrefix + "_XRotation", xRotation);
                PlayerPrefs.SetFloat(savePrefsPrefix + "_YRotation", yRotation);
                PlayerPrefs.SetFloat(savePrefsPrefix + "_ZoomDistance", zoomDistance);
                PlayerPrefs.Save();
            }

            // X rotation
            if (updateRotation || updateRotationX)
                XRotationVelocity += InputManager.GetAxis(xRotationAxisName, false) * rotationSpeed * rotationSpeedScale;
            xRotation -= XRotationVelocity;

            if (limitXRotation)
                xRotation = ClampAngleBetweenMinAndMax(xRotation, minXRotation, maxXRotation);
            else
                xRotation = ClampAngleBetweenMinAndMax(xRotation, -360, 360);

            // Y rotation
            if (updateRotation || updateRotationY)
                YRotationVelocity += InputManager.GetAxis(yRotationAxisName, false) * rotationSpeed * rotationSpeedScale;
            yRotation += YRotationVelocity;

            if (limitYRotation)
                yRotation = ClampAngleBetweenMinAndMax(yRotation, minYRotation, maxYRotation);
            else
                yRotation = ClampAngleBetweenMinAndMax(yRotation, -360, 360);

            // Zoom
            if (updateZoom)
                ZoomVelocity += InputManager.GetAxis(zoomAxisName, false) * zoomSpeed * zoomSpeedScale;
            zoomDistance += ZoomVelocity;
            if (limitZoomDistance)
            {
                if (zoomDistance < minZoomDistance)
                {
                    ZoomVelocity = 0f;
                    zoomDistance = minZoomDistance;
                }
                if (zoomDistance > maxZoomDistance)
                {
                    ZoomVelocity = 0f;
                    zoomDistance = maxZoomDistance;
                }
            }

            // X rotation smooth
            if (smoothRotateX)
                XRotationVelocity = Mathf.LerpAngle(XRotationVelocity, 0, deltaTime * rotateXDeacceleration);
            else
                XRotationVelocity = 0f;

            // Y rotation smooth
            if (smoothRotateY)
                YRotationVelocity = Mathf.LerpAngle(YRotationVelocity, 0, deltaTime * rotateYDeacceleration);
            else
                YRotationVelocity = 0f;

            // Zoom smooth
            if (smoothZoom)
                ZoomVelocity = Mathf.Lerp(ZoomVelocity, 0, deltaTime * zoomDeacceleration);
            else
                ZoomVelocity = 0f;
        }

        public void Recoil(float x, float y, float z)
        {
            _targetRecoilRotation += new Vector3(x, y, z);
        }

        protected override void LateUpdate()
        {
            float deltaTime = Time.deltaTime;
            UpdateAimAssist(deltaTime);
            base.LateUpdate();

            // Update recoiling
            _targetRecoilRotation = Vector3.Lerp(_targetRecoilRotation, Vector3.zero, deltaTime * recoilReturnSpeed);
            _currentRecoilRotation = Vector3.Lerp(_currentRecoilRotation, _targetRecoilRotation, Time.fixedDeltaTime * recoilSmoothing);
            CacheCameraTransform.eulerAngles += _currentRecoilRotation;
        }

        protected void UpdateAimAssist(float deltaTime)
        {
            if (!enableAimAssist || !Application.isPlaying)
                return;
            RaycastHit[] hits = Physics.SphereCastAll(CacheCameraTransform.position, aimAssistRadius, CacheCameraTransform.forward, aimAssistDistance, aimAssistLayerMask);
            System.Array.Sort(hits, 0, hits.Length, new RaycastHitComparer());
            RaycastHit tempHit;
            RaycastHit? hitTarget = null;
            Vector3 cameraDir = CacheCameraTransform.forward;
            Vector3 targetDir;
            for (int i = 0; i < hits.Length; ++i)
            {
                tempHit = hits[i];
                if (aimAssistObstacleLayerMask.value == (aimAssistObstacleLayerMask.value | (1 << tempHit.transform.gameObject.layer)))
                    return;
                if (AimAssistAvoidanceListener != null && AimAssistAvoidanceListener.AvoidAimAssist(tempHit))
                    continue;
                if (Vector3.Distance(target.position, tempHit.point) <= aimAssistMinDistanceFromFollowingTarget)
                    continue;
                targetDir = (tempHit.point - target.position).normalized;
                if (Vector3.Angle(cameraDir, targetDir) > aimAssistMaxAngleFromFollowingTarget)
                    continue;
                hitTarget = tempHit;
                break;
            }
            if (!hitTarget.HasValue)
                return;
            // Set `xRotation`, `yRotation` by hit object's position
            _aimAssistCastHit = hitTarget.Value;
            Vector3 targetCenter = _aimAssistCastHit.collider.bounds.center;
            Vector3 directionToTarget = (targetCenter - CacheCameraTransform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
            if (enableAimAssistX)
                xRotation = Mathf.MoveTowardsAngle(xRotation, lookRotation.eulerAngles.x, aimAssistXSpeed * deltaTime);
            if (enableAimAssistY)
                yRotation = Mathf.MoveTowardsAngle(yRotation, lookRotation.eulerAngles.y, aimAssistYSpeed * deltaTime);
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
#if UNITY_EDITOR
            Gizmos.color = Color.green;
            Gizmos.DrawLine(CacheCameraTransform.position, CacheCameraTransform.position + CacheCameraTransform.forward * _aimAssistCastHit.distance);
            Gizmos.DrawWireSphere(CacheCameraTransform.position + CacheCameraTransform.forward * _aimAssistCastHit.distance, aimAssistRadius);
#endif
        }

        private float ClampAngleBetweenMinAndMax(float angle, float min, float max)
        {
            if (angle < -360)
                angle += 360;
            if (angle > 360)
                angle -= 360;
            return Mathf.Clamp(angle, min, max);
        }

        /// <summary>
        /// Sort ASC by distance from origin to impact point
        /// </summary>
        public struct RaycastHitComparer : IComparer<RaycastHit>
        {
            public int Compare(RaycastHit x, RaycastHit y)
            {
                return x.distance.CompareTo(y.distance);
            }
        }
    }
}
