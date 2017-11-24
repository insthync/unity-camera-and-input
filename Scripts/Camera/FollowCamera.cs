using UnityEngine;

[ExecuteInEditMode]
public class FollowCamera : MonoBehaviour
{

    // The target we are following
    public Transform target;
    public float xRotation;
    public float yRotation;
    public bool useTargetYRotation;
    public Vector3 targetOffset;
    public float zoomDistance = 10.0f;
    // Smoothness setting
    [Range(0, 65)]
    public float damping = 2.0f;

    private void FixedUpdate()
    {
        // Early out if we don't have a target
        if (!target) return;

        var wantedPosition = target.position + targetOffset;
        var wantedYRotation = useTargetYRotation ? target.eulerAngles.y : yRotation;

        // Convert the angle into a rotation
        var currentRotation = Quaternion.Euler(xRotation, wantedYRotation, 0);

        // Set the position of the camera on the x-z plane to:
        // distance meters behind the target
        wantedPosition -= currentRotation * Vector3.forward * zoomDistance;

        // Update position
        transform.position = Vector3.Lerp(transform.position, wantedPosition, damping * Time.deltaTime);

        // Always look at the target
        transform.LookAt(target.transform.position + targetOffset);
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
