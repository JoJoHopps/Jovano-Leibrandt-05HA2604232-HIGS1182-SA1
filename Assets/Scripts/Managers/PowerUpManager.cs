using UnityEngine;
using TMPro;
using System.Collections;

// PowerUpManager spawns power-up pickups over time, applies their effects
// when collected, tracks the timers, and shows the status on the HUD.
// Two power-ups: RAPID FIRE (shoot faster) and SHIELD (absorbs one hit).
public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    public enum PowerUpType { RapidFire, Shield }

    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] powerUpPrefabs;   // One prefab per power-up type
    [SerializeField] private float spawnInterval = 12f;     // Seconds between spawn attempts
    [SerializeField] private float pickupLifetime = 10f;    // Uncollected pickups vanish after this long
    [SerializeField] private float spawnRangeX = 15f;
    [SerializeField] private float spawnRangeZ = 15f;
    [SerializeField] private float spawnHeight = 1f;

    [Header("Rapid Fire Settings")]
    [SerializeField] private float rapidFireDuration = 6f;
    [SerializeField] private float rapidFireRateMultiplier = 0.4f; // Fire interval is multiplied by this (lower = faster)

    [Header("Shield Settings")]
    [SerializeField] private float shieldDuration = 8f;
    [SerializeField] private GameObject shieldVisual;       // Transparent sphere around the player

    [Header("UI")]
    [SerializeField] private TMP_Text statusText;

    private float rapidFireTimeLeft = 0f;
    private float shieldTimeLeft = 0f;
    private GameObject currentPickup; // Only one pickup exists at a time

    // Read-only properties other scripts use to check power-up state
    public bool IsRapidFireActive => rapidFireTimeLeft > 0f;
    public bool IsShieldActive => shieldTimeLeft > 0f;
    public float FireRateMultiplier => IsRapidFireActive ? rapidFireRateMultiplier : 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (shieldVisual != null) shieldVisual.SetActive(false);
        UpdateStatusUI();
        StartCoroutine(SpawnRoutine());
    }

    private void Update()
    {
        TickTimers();
        UpdateStatusUI();
    }

    // ─────────────────────────────────────────────
    // SPAWNING
    // ─────────────────────────────────────────────

    // Every spawnInterval seconds, spawns a random power-up (if none is already waiting)
    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // Stop spawning once the game has ended
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            {
                yield break;
            }

            // Unity treats destroyed objects as null, so this works after pickup/despawn
            if (currentPickup == null)
            {
                SpawnPowerUp();
            }
        }
    }

    private void SpawnPowerUp()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0)
        {
            Debug.LogWarning("PowerUpManager: No power-up prefabs assigned!");
            return;
        }

        GameObject prefab = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];

        float randomX = Random.Range(-spawnRangeX, spawnRangeX);
        float randomZ = Random.Range(-spawnRangeZ, spawnRangeZ);
        Vector3 spawnPosition = new Vector3(randomX, spawnHeight, randomZ);

        currentPickup = Instantiate(prefab, spawnPosition, Quaternion.identity);
        Destroy(currentPickup, pickupLifetime); // Despawn if the player ignores it

        Debug.Log($"PowerUpManager: Spawned {prefab.name} at {spawnPosition}");
    }

    // ─────────────────────────────────────────────
    // APPLYING POWER-UPS
    // ─────────────────────────────────────────────

    // Called by PowerUpPickup when the player collects one
    public void HandlePowerUp(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.RapidFire:
                rapidFireTimeLeft = rapidFireDuration;
                Debug.Log($"PowerUpManager: RAPID FIRE activated for {rapidFireDuration}s");
                break;

            case PowerUpType.Shield:
                shieldTimeLeft = shieldDuration;
                Debug.Log($"PowerUpManager: SHIELD activated for {shieldDuration}s");
                break;
        }

        AudioManager.Play(AudioManager.SoundType.PowerUp);
    }

    // Counts down active timers and toggles the shield visual
    private void TickTimers()
    {
        if (rapidFireTimeLeft > 0f)
        {
            rapidFireTimeLeft -= Time.deltaTime;
            if (rapidFireTimeLeft <= 0f)
            {
                rapidFireTimeLeft = 0f;
                Debug.Log("PowerUpManager: Rapid Fire expired.");
            }
        }

        if (shieldTimeLeft > 0f)
        {
            shieldTimeLeft -= Time.deltaTime;
            if (shieldTimeLeft <= 0f)
            {
                shieldTimeLeft = 0f;
                Debug.Log("PowerUpManager: Shield expired.");
            }
        }

        // Keep the shield bubble in sync with the shield state
        if (shieldVisual != null && shieldVisual.activeSelf != IsShieldActive)
        {
            shieldVisual.SetActive(IsShieldActive);
        }
    }

    // Called by PlayerController when the shield absorbs a hit - shield is used up
    public void ConsumeShield()
    {
        shieldTimeLeft = 0f;
        Debug.Log("PowerUpManager: Shield consumed by a hit.");
    }

    // Shows active power-ups and their remaining time on the HUD
    private void UpdateStatusUI()
    {
        if (statusText == null) return;

        if (!IsRapidFireActive && !IsShieldActive)
        {
            statusText.text = "Power-Up: None";
            return;
        }

        string status = "";
        if (IsRapidFireActive) status += $"Rapid Fire: {rapidFireTimeLeft:F1}s\n";
        if (IsShieldActive) status += $"Shield: {shieldTimeLeft:F1}s";
        statusText.text = status;
    }
}