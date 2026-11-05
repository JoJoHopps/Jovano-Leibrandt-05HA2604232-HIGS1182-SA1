using UnityEngine;
using System.Collections;

// ─────────────────────────────────────────────────────────────────
// EXTERNAL SCRIPT: Camera Shake
// SOURCE: https://youtu.be/9A9yj8KnM8c?si=bkL7DXU_4sm9IwYS
// ADAPTATIONS MADE FOR ASTEROID SCAVENGER:
// 1. The common version of this script moves transform.localPosition
//    directly. That fights with our CameraFollow script (both would
//    overwrite the camera position every frame). Instead, this version
//    exposes a ShakeOffset that CameraFollow ADDS to its own position.
// 2. The shake fades out over its duration (magnitude * remaining time)
//    so it feels like an impact rather than a constant vibration.
// 3. Starting a new shake while one is running restarts it cleanly.
// 4. Shake is triggered from PlayerController when the player is hit.
// ─────────────────────────────────────────────────────────────────
public class CameraShake : MonoBehaviour
{
    // CameraFollow reads this every frame and adds it to the camera position
    public Vector3 ShakeOffset { get; private set; } = Vector3.zero;

    private Coroutine shakeRoutine;

    // Call this from any script: cameraShake.Shake(0.4f, 0.5f);
    public void Shake(float duration, float magnitude)
    {
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
        }

        shakeRoutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
        Debug.Log($"CameraShake: Shaking for {duration}s at magnitude {magnitude}");
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Strength fades from 1 to 0 over the shake's duration
            float fade = 1f - (elapsed / duration);

            // Random offset on X and Y (the screen plane for our angled top-down camera)
            float offsetX = Random.Range(-1f, 1f) * magnitude * fade;
            float offsetY = Random.Range(-1f, 1f) * magnitude * fade;
            ShakeOffset = new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null; // Wait one frame
        }

        ShakeOffset = Vector3.zero;
        shakeRoutine = null;
    }
}