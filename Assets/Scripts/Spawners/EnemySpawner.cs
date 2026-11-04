using UnityEngine;

// EnemySpawner spawns enemies ONLY when told to by GameManager -
// either the initial wave, or additional enemies for a new wave.
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int initialSpawnCount = 1;
    [SerializeField] private int enemiesAddedPerWave = 1; // How many MORE enemies appear each wave
    [SerializeField] private int maxEnemies = 12;           // Safety cap for performance
    [SerializeField] private float spawnRangeX = 18f;
    [SerializeField] private float spawnRangeZ = 18f;
    [SerializeField] private float spawnHeight = 1f;

    private int currentEnemyCount = 0;

    // Called ONCE by GameManager when the game starts (Wave 1)
    public void SpawnInitialEnemies()
    {
        SpawnMultiple(initialSpawnCount);
        Debug.Log($"EnemySpawner: Spawned initial {initialSpawnCount} enemies.");
    }

    // Called by GameManager every time a new wave begins
    public void SpawnAdditionalEnemies()
    {
        if (currentEnemyCount >= maxEnemies)
        {
            Debug.Log("EnemySpawner: Max enemy cap reached, skipping extra spawn.");
            return;
        }

        SpawnMultiple(enemiesAddedPerWave);
        Debug.Log($"EnemySpawner: Spawned {enemiesAddedPerWave} additional enemies for new wave. Total active: {currentEnemyCount}");
    }

    // Shared helper - spawns a given number of enemies. Used by BOTH
    // the initial spawn and each wave's additional spawn, avoiding duplicate code.
    private void SpawnMultiple(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnSingleEnemy();
        }
    }

    private void SpawnSingleEnemy()
    {
        float randomX = Random.Range(-spawnRangeX, spawnRangeX);
        float randomZ = Random.Range(-spawnRangeZ, spawnRangeZ);
        Vector3 spawnPosition = new Vector3(randomX, spawnHeight, randomZ);

        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        currentEnemyCount++;
    }

    // Called by EnemyAI (via OnDestroy, see below) whenever one is destroyed
    public void OnEnemyDestroyed()
    {
        currentEnemyCount--;
        if (currentEnemyCount < 0) currentEnemyCount = 0;
    }
}