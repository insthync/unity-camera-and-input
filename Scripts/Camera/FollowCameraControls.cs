using UnityEngine;

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
    public float minXRotation = 0;
    public float maxXRotation = 0;
    [Header("Y Rotation")]
    public bool limitYRotation;
    public float minYRotation = 0;
    public float maxYRotation = 0;
    [Header("General Rotation Settings")]
    public float startXRotation;
    public float startYRotation;
    public float rotationSpeed = 5;
    [Header("Zoom")]
    public bool limitZoomDistance;
    public float minZoomDistance;
    public float maxZoomDistance;
    [Header("General Zoom Settings")]
    public float startZoomDistance;
    public float zoomSpeed = 5;

    private void Start()
    {
        xRotation = startXRotation;
        yRotation = startYRotation;
        zoomDistance = startZoomDistance;
    }

    protected override void LateUpdate()
    {
        if (updateRotation || updateRotationX)
        {
            float mY = InputManager.GetAxis("Mouse Y", false);
            xRotation -= mY * rotationSpeed;
            if (limitXRotation)
                xRotation = Mathf.Clamp(xRotation, minXRotation, maxXRotation);
        }

        if (updateRotation || updateRotationY)
        {
            float mX = InputManager.GetAxis("Mouse X", false);
            yRotation += mX * rotationSpeed;
            if (limitYRotation)
                yRotation = Mathf.Clamp(yRotation, minYRotation, maxYRotation);
        }

        if (updateZoom)
        {
            float mZ = InputManager.GetAxis("Mouse ScrollWheel", false);
            zoomDistance += mZ * zoomSpeed;
            if (limitZoomDistance)
                zoomDistance = Mathf.Clamp(zoomDistance, minZoomDistance, maxZoomDistance);
        }

        base.LateUpdate();
    }
}
