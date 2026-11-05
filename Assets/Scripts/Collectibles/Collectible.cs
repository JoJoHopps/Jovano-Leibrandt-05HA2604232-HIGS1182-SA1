using UnityEngine;

// Collectible sits on each scrap canister. When the player touches it,
// it adds to the score, plays a sound and visual effect, and disappears.
public class Collectible : MonoBehaviour
{
    [SerializeField] private GameObject pickupEffectPrefab; // Optional flash effect
    [SerializeField] private float spinSpeed = 60f;

    private void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f);
    }

    // Requires: Collider with Is Trigger ON
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CollectItem();
        }
    }

    private void CollectItem()
    {
        Debug.Log($"Collectible: {gameObject.name} collected by player!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(1);
        }

        AudioManager.Play(AudioManager.SoundType.Pickup);

        if (pickupEffectPrefab != null)
        {
            Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}