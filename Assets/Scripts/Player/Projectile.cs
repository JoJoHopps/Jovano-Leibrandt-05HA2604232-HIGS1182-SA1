using UnityEngine;

// Projectile.cs is attached to a small "ImpactEffect" prefab.
// It represents the visual "hit" flash created at the point our raycast
// lands on a target. Our shooting is raycast-based (instant hit) rather
// than a physically moving bullet, so this script's job is purely to
// manage the lifespan of the visual feedback effect.
public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.2f; // How long the effect stays visible

    private void Start()
    {
        // Destroy automatically removes this GameObject after 'lifetime' seconds
        Destroy(gameObject, lifetime);
        Debug.Log("Projectile: Impact effect spawned.");
    }
}