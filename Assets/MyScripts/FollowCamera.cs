using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform; // drag XR Origin > Camera Offset > Main Camera

    [Header("Offset from camera")]
    public Vector3 offset = new Vector3(0f, 0f, 2f); // 2 metres in front

    [Header("Settings")]
    public float followSpeed = 3f;
    public bool lockVertical = false; // true for HUD reset button

    private void LateUpdate()
    {
        if (cameraTransform == null) return;

        // Position in front of camera
        Vector3 forward = cameraTransform.forward;
        if (lockVertical) forward.y = 0f; // keep HUD at fixed height
        forward.Normalize();

        Vector3 targetPos = cameraTransform.position
            + forward * offset.z
            + cameraTransform.right * offset.x
            + cameraTransform.up * offset.y;

        transform.position = Vector3.Lerp(
            transform.position, targetPos,
            Time.deltaTime * followSpeed
        );

        // Always face the player
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(transform.position - cameraTransform.position),
            Time.deltaTime * followSpeed
        );
    }
}