using UnityEngine;

// PowerUpPickup sits on each power-up prefab in the world.
// When the player touches it, it activates the power-up and removes itself.
public class PowerUpPickup : MonoBehaviour
{
    [SerializeField] private PowerUpManager.PowerUpType powerUpType; // Set per-prefab in the Inspector
    [SerializeField] private GameObject pickupEffectPrefab;          // Optional flash effect
    [SerializeField] private float spinSpeed = 90f;

    private void Update()
    {
        // Spin so the pickup is easy to notice
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f);
    }

    // Requires: Collider with Is Trigger ON (the player's Rigidbody makes this fire)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    private void Collect()
    {
        Debug.Log($"PowerUpPickup: {powerUpType} collected!");

        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.HandlePowerUp(powerUpType);
        }

        if (pickupEffectPrefab != null)
        {
            Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}