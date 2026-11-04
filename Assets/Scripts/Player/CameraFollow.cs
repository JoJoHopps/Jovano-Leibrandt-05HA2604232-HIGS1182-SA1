using UnityEngine;

// CameraFollow makes the camera smoothly follow the player from above.
// Attach this to the Camera GameObject.
public class CameraFollow : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // INSPECTOR FIELDS
    // ─────────────────────────────────────────────
    [Header("Target")]
    [SerializeField] private Transform target; // The player's Transform

    [Header("Follow Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 15f, -10f); // Camera offset from player
    [SerializeField] private float smoothSpeed = 8f; // How smoothly the camera follows

    // ─────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────

    // LateUpdate runs AFTER all Update() methods
    // This ensures the player has moved before the camera updates
    private void LateUpdate()
    {
        if (target == null) return;

        FollowTarget();
    }

    // ─────────────────────────────────────────────
    // CUSTOM METHODS
    // ─────────────────────────────────────────────

    // Smoothly moves the camera to follow the target
    private void FollowTarget()
    {
        // Desired position = player position + our offset
        Vector3 desiredPosition = target.position + offset;

        // Lerp = Linear Interpolation – moves smoothly from current to desired
        // Higher smoothSpeed = snappier follow
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        transform.position = smoothedPosition;

        // Always look at the player
        transform.LookAt(target);
    }
}