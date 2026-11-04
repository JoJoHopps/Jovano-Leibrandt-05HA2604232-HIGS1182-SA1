using UnityEngine;

// CollectibleManager spawns scrap/fuel canisters ONLY when told to by
// GameManager - once at the start of the game, and again at the start
// of every new wave.
public class CollectibleManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject collectiblePrefab;
    [SerializeField] private float spawnRangeX = 15f;
    [SerializeField] private float spawnRangeZ = 15f;
    [SerializeField] private float spawnHeight = 1f;

    // Called by GameManager at the start of the game AND at the start
    // of every new wave - spawns a fresh batch of collectibles
    public void SpawnWaveCollectibles(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnSingleCollectible();
        }

        Debug.Log($"CollectibleManager: Spawned {count} collectibles for this wave.");
    }

    private void SpawnSingleCollectible()
    {
        float randomX = Random.Range(-spawnRangeX, spawnRangeX);
        float randomZ = Random.Range(-spawnRangeZ, spawnRangeZ);
        Vector3 spawnPosition = new Vector3(randomX, spawnHeight, randomZ);

        Instantiate(collectiblePrefab, spawnPosition, Quaternion.identity);
    }
}