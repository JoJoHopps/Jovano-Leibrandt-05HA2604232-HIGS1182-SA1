using UnityEngine;

// AsteroidSpawner spawns asteroids ONLY when told to by GameManager -
// either the initial wave, or additional asteroids for a new wave.
public class AsteroidSpawner : MonoBehaviour
{
    public static AsteroidSpawner Instance { get; private set; }

    [Header("Spawning Settings")]
    [SerializeField] private GameObject asteroidPrefab;
    [SerializeField] private int initialSpawnCount = 6;
    [SerializeField] private int asteroidsAddedPerWave = 2; // How many MORE asteroids appear each wave
    [SerializeField] private int maxAsteroids = 30;          // Safety cap for performance
    [SerializeField] private float spawnRangeX = 20f;
    [SerializeField] private float spawnRangeZ = 20f;
    [SerializeField] private float spawnHeight = 1f;
    [SerializeField] private float asteroidSpeed = 2f;

    private int currentAsteroidCount = 0;

    private void Awake()
    {
        Instance = this;
    }

    // Called ONCE by GameManager when the game starts (Wave 1)
    public void SpawnInitialAsteroids()
    {
        SpawnMultiple(initialSpawnCount);
        Debug.Log($"AsteroidSpawner: Spawned initial {initialSpawnCount} asteroids.");
    }

    // Called by GameManager every time a new wave begins
    public void SpawnAdditionalAsteroids()
    {
        if (currentAsteroidCount >= maxAsteroids)
        {
            Debug.Log("AsteroidSpawner: Max asteroid cap reached, skipping extra spawn.");
            return;
        }

        SpawnMultiple(asteroidsAddedPerWave);
        Debug.Log($"AsteroidSpawner: Spawned {asteroidsAddedPerWave} additional asteroids for new wave. Total active: {currentAsteroidCount}");
    }

    // Shared helper - spawns a given number of asteroids. Used by BOTH
    // the initial spawn and each wave's additional spawn, avoiding duplicate code.
    private void SpawnMultiple(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnSingleAsteroid();
        }
    }

    private void SpawnSingleAsteroid()
    {
        float randomX = Random.Range(-spawnRangeX, spawnRangeX);
        float randomZ = Random.Range(-spawnRangeZ, spawnRangeZ);
        Vector3 spawnPosition = new Vector3(randomX, spawnHeight, randomZ);

        GameObject asteroid = Instantiate(asteroidPrefab, spawnPosition, Quaternion.identity);
        SetAsteroidMovement(asteroid);
        currentAsteroidCount++;
    }

    private void SetAsteroidMovement(GameObject asteroid)
    {
        Rigidbody rb = asteroid.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            ).normalized;

            // Asteroids drift slightly faster on later waves too
            rb.linearVelocity = randomDirection * asteroidSpeed * GetDifficultyMultiplier();
        }
    }

    private float GetDifficultyMultiplier()
    {
        if (GameManager.Instance != null)
        {
            return GameManager.Instance.DifficultyMultiplier;
        }
        return 1f;
    }

    // Called by Asteroid.cs (via OnDestroy) whenever one is destroyed
    public void OnAsteroidDestroyed()
    {
        currentAsteroidCount--;
        if (currentAsteroidCount < 0) currentAsteroidCount = 0;
    }
}