using UnityEngine;

// Collectible.cs sits on each scrap/canister item.
// When the player walks into it, it gets collected and adds to the score.
public class Collectible : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering object is the Player
        if (other.CompareTag("Player"))
        {
            CollectItem();
        }
    }

    // Handles what happens when this item is collected
    private void CollectItem()
    {
        Debug.Log($"Collectible: {gameObject.name} collected by player!");

        // Tell the GameManager to add to the score
        GameManager.Instance.AddScore(1);

        // Destroy this collectible from the scene
        Destroy(gameObject);
    }
}