using UnityEngine;
using System.Collections;

// EnemyAI controls an enemy drone using a simple state machine.
// PATROL: wander near the spawn point.
// CHASE: when the player enters detection range, hunt them down.
// All balance values are exposed in the Inspector for easy testing.
public class EnemyAI : MonoBehaviour
{
    private enum AIState { Patrol, Chase }

    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 10f;

    [Header("Movement Settings")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float patrolRadius = 8f;
    [SerializeField] private float waypointReachedDistance = 0.5f;
    [SerializeField] private float turnSpeed = 5f;

    [Header("Combat Settings")]
    [SerializeField] private int maxHealth = 2;

    [Header("Feedback Settings")]
    [SerializeField] private Color hitFlashColor = Color.white;
    [SerializeField] private float hitFlashDuration = 0.1f;
    [SerializeField] private GameObject deathEffectPrefab; // Optional

    // ─────────────────────────────────────────────
    // PRIVATE VARIABLES
    // ─────────────────────────────────────────────
    private AIState currentState = AIState.Patrol;
    private Rigidbody rb;
    private Renderer enemyRenderer;
    private Color originalColor;
    private Transform player;
    private Vector3 spawnPosition;
    private Vector3 currentPatrolTarget;
    private int currentHealth;
    private bool hasSpottedPlayer = false;

    // ─────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────

    private void Start()
    {
        // Cache components once
        rb = GetComponent<Rigidbody>();
        enemyRenderer = GetComponentInChildren<Renderer>();

        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
        }

        spawnPosition = transform.position;
        currentHealth = maxHealth;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        PickNewPatrolPoint();

        Debug.Log($"EnemyAI: {gameObject.name} initialized with {currentHealth} health.");
    }

    private void Update()
    {
        if (player == null || !IsGameActive()) return;
        UpdateState();
    }

    private void FixedUpdate()
    {
        if (player == null || !IsGameActive()) return;

        if (currentState == AIState.Patrol)
        {
            Patrol();
        }
        else
        {
            Chase();
        }
    }

    // ─────────────────────────────────────────────
    // AI LOGIC
    // ─────────────────────────────────────────────

    // Enemies freeze once the game is over or won
    private bool IsGameActive()
    {
        return GameManager.Instance == null ||
               GameManager.Instance.CurrentState == GameManager.GameState.Playing;
    }

    // Switches between Patrol and Chase based on distance to the player
    private void UpdateState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            if (currentState != AIState.Chase)
            {
                currentState = AIState.Chase;

                if (!hasSpottedPlayer)
                {
                    Debug.Log($"EnemyAI: {gameObject.name} spotted the player!");
                    hasSpottedPlayer = true;
                }
            }
        }
        else if (currentState != AIState.Patrol)
        {
            currentState = AIState.Patrol;
            hasSpottedPlayer = false;
            Debug.Log($"EnemyAI: {gameObject.name} lost the player, returning to patrol.");
            PickNewPatrolPoint();
        }
    }

    private void Patrol()
    {
        MoveTowards(currentPatrolTarget, patrolSpeed);

        if (Vector3.Distance(transform.position, currentPatrolTarget) <= waypointReachedDistance)
        {
            PickNewPatrolPoint();
        }
    }

    // Chase speed scales with the wave difficulty multiplier from GameManager
    private void Chase()
    {
        float speed = chaseSpeed * GetDifficultyMultiplier();
        MoveTowards(player.position, speed);
    }

    // Shared movement used by BOTH Patrol and Chase
    private void MoveTowards(Vector3 targetPosition, float speed)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;
        direction = direction.normalized;

        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
        }
    }

    private void PickNewPatrolPoint()
    {
        Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
        currentPatrolTarget = spawnPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);
    }

    private float GetDifficultyMultiplier()
    {
        if (GameManager.Instance != null)
        {
            return GameManager.Instance.DifficultyMultiplier;
        }
        return 1f;
    }

    // ─────────────────────────────────────────────
    // COMBAT
    // ─────────────────────────────────────────────

    // Called by Projectile when a laser hits this enemy
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"EnemyAI: {gameObject.name} took {amount} damage. Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(FlashOnHit());
        }
    }

    // Briefly changes colour so the player can SEE the hit landed
    private IEnumerator FlashOnHit()
    {
        if (enemyRenderer == null) yield break;

        enemyRenderer.material.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashDuration);
        enemyRenderer.material.color = originalColor;
    }

    private void Die()
    {
        Debug.Log($"EnemyAI: {gameObject.name} destroyed.");

        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    // Keeps the spawner's active enemy count accurate
    private void OnDestroy()
    {
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.OnEnemyDestroyed();
        }
    }
}