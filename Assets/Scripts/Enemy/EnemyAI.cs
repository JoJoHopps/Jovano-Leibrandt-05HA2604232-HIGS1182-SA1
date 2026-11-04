using UnityEngine;

// EnemyAI controls enemy drone behaviour using a simple state machine.
// The enemy patrols near its spawn point until the player comes within
// detection range, then switches to chasing the player directly.
public class EnemyAI : MonoBehaviour
{
    // An enum defines a fixed set of named states - much cleaner than
    // using strings or numbers to track behaviour
    private enum AIState { Patrol, Chase }

    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 10f; // How close the player must be to be noticed

    [Header("Movement Settings")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float patrolRadius = 8f; // How far from spawn point it will wander
    [SerializeField] private float waypointReachedDistance = 0.5f;

    [Header("Combat Settings")]
    [SerializeField] private int maxHealth = 2; // Takes 2 shots to destroy

    // ─────────────────────────────────────────────
    // PRIVATE VARIABLES
    // ─────────────────────────────────────────────
    private AIState currentState = AIState.Patrol;
    private Rigidbody rb;
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
        rb = GetComponent<Rigidbody>();
        spawnPosition = transform.position;
        currentHealth = maxHealth;

        // Find the player in the scene using the "Player" tag
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
        if (player == null) return;
        UpdateState();
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        if (currentState == AIState.Patrol)
        {
            Patrol();
        }
        else if (currentState == AIState.Chase)
        {
            Chase();
        }
    }

    // ─────────────────────────────────────────────
    // CUSTOM METHODS
    // ─────────────────────────────────────────────

    // Checks the distance to the player and switches states accordingly
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
        else
        {
            if (currentState != AIState.Patrol)
            {
                currentState = AIState.Patrol;
                hasSpottedPlayer = false;
                Debug.Log($"EnemyAI: {gameObject.name} lost the player, returning to patrol.");
                PickNewPatrolPoint();
            }
        }
    }

    // Moves the enemy toward a random point near its spawn location
    private void Patrol()
    {
        MoveTowards(currentPatrolTarget, patrolSpeed);

        float distanceToTarget = Vector3.Distance(transform.position, currentPatrolTarget);
        if (distanceToTarget <= waypointReachedDistance)
        {
            PickNewPatrolPoint();
        }
    }

    // Moves the enemy directly toward the player's current position
    private void Chase()
    {
        // Speed scales with the game's difficulty multiplier - enemies get faster over time
        float speed = chaseSpeed * GetDifficultyMultiplier();
        MoveTowards(player.position, speed);
    }

    // Shared movement logic used by BOTH Patrol and Chase - avoids duplicate code
    private void MoveTowards(Vector3 targetPosition, float speed)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;
        direction = direction.normalized;

        Vector3 newPosition = rb.position + direction * speed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        // Rotate to face the direction of movement
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 5f * Time.fixedDeltaTime));
        }
    }

    // Picks a new random point within patrolRadius of the spawn position
    private void PickNewPatrolPoint()
    {
        // Random.insideUnitCircle gives a random 2D point inside a circle of radius 1
        Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
        currentPatrolTarget = spawnPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);
    }

    // Safely retrieves the current difficulty multiplier from GameManager
    private float GetDifficultyMultiplier()
    {
        if (GameManager.Instance != null)
        {
            return GameManager.Instance.DifficultyMultiplier;
        }
        return 1f; // Fallback if GameManager isn't found for some reason
    }

    // ─────────────────────────────────────────────
    // COMBAT
    // ─────────────────────────────────────────────

    // Called by PlayerController when a raycast hits this enemy
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"EnemyAI: {gameObject.name} took {amount} damage. Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"EnemyAI: {gameObject.name} destroyed.");
        Destroy(gameObject);
    }

    // ─────────────────────────────────────────────
    // COLLISION
    // ─────────────────────────────────────────────

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("EnemyAI: Collided with player!");

            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.KillPlayer();
            }
        }
    }
    // Unity automatically calls this whenever the enemy is destroyed,
    // whether from being shot or any other reason. Keeps the spawner's count accurate.
    private void OnDestroy()
    {
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.OnEnemyDestroyed();
        }
    }
}