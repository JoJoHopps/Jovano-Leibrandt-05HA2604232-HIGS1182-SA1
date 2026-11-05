using UnityEngine;

// CameraFollow makes the camera smoothly follow the player from above.
// Attach this to the Main Camera. It works together with CameraShake
// (if present on the same object) by adding the shake offset on top.
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 15f, -10f);
    [SerializeField] private float smoothSpeed = 8f;

    private CameraShake cameraShake;          // Optional shake component
    private Vector3 smoothedBasePosition;     // Follow position WITHOUT the shake applied

    private void Awake()
    {
        // Cache the optional CameraShake component (may be null, which is fine)
        cameraShake = GetComponent<CameraShake>();
    }

    private void Start()
    {
        smoothedBasePosition = transform.position;
    }

    private void LateUpdate()
    {
        if (target == null) return;
        FollowTarget();
    }

    private void FollowTarget()
    {
        Vector3 desiredPosition = target.position + offset;

        // We smooth the BASE position separately so the shake offset
        // doesn't get fed back into the smoothing and accumulate
        smoothedBasePosition = Vector3.Lerp(
            smoothedBasePosition,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        Vector3 shakeOffset = (cameraShake != null) ? cameraShake.ShakeOffset : Vector3.zero;

        transform.position = smoothedBasePosition + shakeOffset;
        transform.LookAt(target);
    }
}