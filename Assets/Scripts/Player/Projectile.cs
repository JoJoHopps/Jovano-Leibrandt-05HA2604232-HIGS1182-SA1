using UnityEngine;

// Projectile is attached to the laser bolt PREFAB that the player fires.
// It flies forward at a constant speed and applies damage to whatever it hits:
//   - Asteroids are destroyed
//   - Enemy drones take damage
// Collisions are detected with OnTriggerEnter and identified by tag.
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 30f;
    [SerializeField] private float lifetime = 2f;   // Auto-destroy if it hits nothing
    [SerializeField] private int damage = 1;

    [Header("Effects")]
    [SerializeField] private GameObject impactEffectPrefab;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Launch();
        Destroy(gameObject, lifetime);
    }

    // Gives the projectile its velocity in the direction it is facing
    private void Launch()
    {
        if (rb == null)
        {
            Debug.LogError("Projectile: Missing Rigidbody on projectile prefab!");
            return;
        }

        rb.useGravity = false;
        rb.linearVelocity = transform.forward * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only asteroids and enemies are valid targets - ignore everything else
        // (the player, scrap, power-ups all pass straight through)
        if (other.CompareTag("Asteroid") || other.CompareTag("Enemy"))
        {
            ApplyDamage(other);
            SpawnImpactEffect();
            Destroy(gameObject);
        }
    }

    // Decides what happens based on what was hit
    private void ApplyDamage(Collider target)
    {
        if (target.CompareTag("Asteroid"))
        {
            Debug.Log($"Projectile: Asteroid destroyed - {target.gameObject.name}");
            Destroy(target.gameObject);
        }
        else if (target.CompareTag("Enemy"))
        {
            EnemyAI enemy = target.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                Debug.Log($"Projectile: Hit enemy {target.gameObject.name} for {damage} damage");
                enemy.TakeDamage(damage);
            }
        }
    }

    private void SpawnImpactEffect()
    {
        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}