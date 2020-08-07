﻿using UnityEngine;

[ExecuteInEditMode]
public class FollowCameraControls : FollowCamera
{
    [Header("Controls")]
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
    public float rotateXSmoothing = 10.0f;

    [Header("Y Rotation")]
    public bool limitYRotation;
    [Range(-360, 360)]
    public float minYRotation = 0;
    [Range(-360, 360)]
    public float maxYRotation = 0;
    public bool smoothRotateY;
    public float rotateYSmoothing = 10.0f;

    [Header("General Rotation Settings")]
    public float startXRotation;
    public float startYRotation;
    public float rotationSpeed = 5;

    [Header("Zoom")]
    public bool limitZoomDistance;
    public float minZoomDistance;
    public float maxZoomDistance;
    public bool smoothZoom;
    public float zoomSmoothing = 10.0f;

    [Header("General Zoom Settings")]
    public float startZoomDistance;
    public float zoomSpeed = 5;

    [Header("Aim Assistance")]
    public bool enableAimAssistance;
    public float aimAssistanceRadius = 0.5f;
    public float aimAssistanceDistance = 10f;
    public LayerMask aimAssistanceLayerMask;
    public Color aimAssistanceGizmosColor = Color.green;

    [Header("Save Camera")]
    public bool isSaveCamera;
    public string savePrefsPrefix = "GAMEPLAY";

    private float xVelocity;
    private float yVelocity;
    private float zoomVelocity;
    private RaycastHit aimAssistanceCastHit;

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
        if (isSaveCamera)
        {
            PlayerPrefs.SetFloat(savePrefsPrefix + "_XRotation", xRotation);
            PlayerPrefs.SetFloat(savePrefsPrefix + "_YRotation", yRotation);
            PlayerPrefs.SetFloat(savePrefsPrefix + "_ZoomDistance", zoomDistance);
            PlayerPrefs.Save();
        }

        float deltaTime = Time.deltaTime;

        // X rotation
        if (updateRotation || updateRotationX)
            xVelocity += InputManager.GetAxis("Mouse Y", false) * rotationSpeed;
        xRotation -= xVelocity;
        if (limitXRotation)
            xRotation = ClampAngleBetweenMinAndMax(xRotation, minXRotation, maxXRotation);
        else
            xRotation = ClampAngleBetweenMinAndMax(xRotation, -360, 360);

        // Y rotation
        if (updateRotation || updateRotationY)
            yVelocity += InputManager.GetAxis("Mouse X", false) * rotationSpeed;
        yRotation += yVelocity;
        if (limitYRotation)
            yRotation = ClampAngleBetweenMinAndMax(yRotation, minYRotation, maxYRotation);
        else
            yRotation = ClampAngleBetweenMinAndMax(yRotation, -360, 360);

        // Zoom
        if (updateZoom)
            zoomVelocity += InputManager.GetAxis("Mouse ScrollWheel", false) * zoomSpeed;
        zoomDistance += zoomVelocity;
        if (limitZoomDistance)
            zoomDistance = Mathf.Clamp(zoomDistance, minZoomDistance, maxZoomDistance);

        // X rotation smooth
        if (smoothRotateX)
            xVelocity = Mathf.Lerp(xVelocity, 0, deltaTime * rotateXSmoothing);
        else
            xVelocity = 0f;

        // Y rotation smooth
        if (smoothRotateY)
            yVelocity = Mathf.Lerp(yVelocity, 0, deltaTime * rotateYSmoothing);
        else
            yVelocity = 0f;

        // Zoom smooth
        if (smoothZoom)
            zoomVelocity = Mathf.Lerp(zoomVelocity, 0, deltaTime * zoomSmoothing);
        else
            zoomVelocity = 0f;

        if (enableAimAssistance)
        {
            if (Physics.SphereCast(CacheCameraTransform.position, aimAssistanceRadius, CacheCameraTransform.forward, out aimAssistanceCastHit, aimAssistanceDistance, aimAssistanceLayerMask))
            {
                // Set `xRotation`, `yRotation` by hit object's position

            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(CacheCameraTransform.position, CacheCameraTransform.position + CacheCameraTransform.forward * aimAssistanceCastHit.distance);
        Gizmos.DrawWireSphere(CacheCameraTransform.position + CacheCameraTransform.forward * aimAssistanceCastHit.distance, aimAssistanceRadius);
    }

    private float ClampAngleBetweenMinAndMax(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}
