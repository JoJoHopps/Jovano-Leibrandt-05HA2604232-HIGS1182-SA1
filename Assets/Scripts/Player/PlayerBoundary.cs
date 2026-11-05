using UnityEngine;

/// <summary>
/// Keeps the player inside a rectangular play area.
/// The position is clamped every physics step, and any velocity pushing
/// outward is cancelled so the ship stops at the edge instead of jittering.
/// Also locks the player's height so it can never fall off the map.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerBoundary : MonoBehaviour
{
    [Header("Play Area (world units)")]
    [Tooltip("Centre of the play area on the X/Z plane.")]
    [SerializeField] private Vector3 centre = Vector3.zero;
    [Tooltip("Distance from the centre to the left/right edge.")]
    [SerializeField] private float halfWidth = 20f;
    [Tooltip("Distance from the centre to the top/bottom edge.")]
    [SerializeField] private float halfDepth = 20f;

    [Header("Height")]
    [Tooltip("Locks the player to the height it starts at, so it cannot fall.")]
    [SerializeField] private bool lockHeight = true;

    private Rigidbody rb;
    private float fixedY;
    private bool wasAtEdge;

    private void Awake()
    {
        // GetComponent: grab the Rigidbody so we can move it safely with physics.
        rb = GetComponent<Rigidbody>();
        fixedY = rb.position.y;
    }

    // FixedUpdate runs in step with physics, which is where Rigidbody changes belong.
    private void FixedUpdate()
    {
        ClampToBounds();
    }

    // Custom method: pushes the player back inside the play area if it left it.
    public void ClampToBounds()
    {
        Vector3 pos = rb.position;
        Vector3 vel = rb.linearVelocity;
        bool atEdge = false;

        float minX = centre.x - halfWidth;
        float maxX = centre.x + halfWidth;
        float minZ = centre.z - halfDepth;
        float maxZ = centre.z + halfDepth;

        if (pos.x < minX) { pos.x = minX; if (vel.x < 0f) vel.x = 0f; atEdge = true; }
        if (pos.x > maxX) { pos.x = maxX; if (vel.x > 0f) vel.x = 0f; atEdge = true; }
        if (pos.z < minZ) { pos.z = minZ; if (vel.z < 0f) vel.z = 0f; atEdge = true; }
        if (pos.z > maxZ) { pos.z = maxZ; if (vel.z > 0f) vel.z = 0f; atEdge = true; }

        if (lockHeight)
        {
            pos.y = fixedY;
            vel.y = 0f;
        }

        rb.position = pos;
        rb.linearVelocity = vel;

        // Log only when the player first touches the edge, not every frame.
        if (atEdge && !wasAtEdge)
            Debug.Log("PlayerBoundary: player reached the edge of the play area.");
        wasAtEdge = atEdge;
    }

    // Draws the play area as a green box in the Scene view when the player is selected.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(centre, new Vector3(halfWidth * 2f, 1f, halfDepth * 2f));
    }
}