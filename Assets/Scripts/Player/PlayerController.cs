using UnityEngine;

// PlayerController handles player input, movement, shooting (prefab projectiles),
// taking hits, and talking to the animation, audio, and camera-shake systems.
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float fireRate = 0.3f;              // Seconds between shots
    [SerializeField] private float projectileSpawnOffset = 1f;   // How far in front of the player the laser appears

    [Header("Hit Feedback")]
    [SerializeField] private float deathShakeDuration = 0.5f;
    [SerializeField] private float deathShakeMagnitude = 0.5f;
    [SerializeField] private float shieldShakeDuration = 0.15f;
    [SerializeField] private float shieldShakeMagnitude = 0.2f;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CameraShake cameraShake;

    // ─────────────────────────────────────────────
    // PRIVATE VARIABLES
    // ─────────────────────────────────────────────
    private Rigidbody rb;
    private AnimationController animationController;
    private float nextFireTime = 0f;
    private bool isAlive = true;
    private bool wasMoving = false;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 mouseAimDirection = Vector3.forward; // Mouse decides aim; WASD decides movement

    // ─────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────

    private void Start()
    {
        // Cache all components once so we never search for them again
        rb = GetComponent<Rigidbody>();
        animationController = GetComponent<AnimationController>();

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (cameraShake == null && playerCamera != null)
        {
            cameraShake = playerCamera.GetComponent<CameraShake>();
        }

        if (animationController == null)
        {
            Debug.LogWarning("PlayerController: No AnimationController found on Player.");
        }

        Debug.Log("PlayerController: Player initialized.");
    }

    private void Update()
    {
        if (!isAlive) return;

        HandleInput();
        UpdateMouseAimDirection();
        HandleShooting();
    }

    private void FixedUpdate()
    {
        if (!isAlive) return;
        ApplyMovement();
    }

    // ─────────────────────────────────────────────
    // MOVEMENT
    // ─────────────────────────────────────────────

    // Reads WASD / arrow keys and updates the idle/thrust animation
    private void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        bool isMoving = Mathf.Abs(horizontalInput) > 0.01f || Mathf.Abs(verticalInput) > 0.01f;
        UpdateMovementAnimation(isMoving);
    }

    // Only tells the Animator when the moving/idle state actually CHANGES
    private void UpdateMovementAnimation(bool isMoving)
    {
        if (isMoving == wasMoving) return;

        wasMoving = isMoving;

        if (animationController != null)
        {
            animationController.SetMoving(isMoving);
        }
    }

    private void ApplyMovement()
    {
        Vector3 moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;
        Vector3 newPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    // ─────────────────────────────────────────────
    // AIMING & SHOOTING
    // ─────────────────────────────────────────────

    // Works out which direction the mouse is pointing in the world (used for shooting only)
    private void UpdateMouseAimDirection()
    {
        Ray mouseRay = playerCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(mouseRay, out float rayDistance))
        {
            Vector3 mouseWorldPosition = mouseRay.GetPoint(rayDistance);
            Vector3 directionToMouse = mouseWorldPosition - transform.position;
            directionToMouse.y = 0f;

            if (directionToMouse.sqrMagnitude > 0.01f)
            {
                mouseAimDirection = directionToMouse.normalized;
            }
        }
    }

    private void HandleShooting()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + GetCurrentFireInterval();
            FireProjectile();
        }
    }

    // Returns the time between shots, shortened if Rapid Fire is active
    private float GetCurrentFireInterval()
    {
        float multiplier = 1f;

        if (PowerUpManager.Instance != null)
        {
            multiplier = PowerUpManager.Instance.FireRateMultiplier;
        }

        return fireRate * multiplier;
    }

    // Spawns a laser projectile prefab in front of the player, facing the aim direction
    private void FireProjectile()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("PlayerController: No projectile prefab assigned!");
            return;
        }

        Vector3 spawnPosition = transform.position + mouseAimDirection * projectileSpawnOffset;
        Quaternion spawnRotation = Quaternion.LookRotation(mouseAimDirection);

        Instantiate(projectilePrefab, spawnPosition, spawnRotation);

        Debug.Log("PlayerController: Laser fired!");

        AudioManager.Play(AudioManager.SoundType.Shoot);

        if (animationController != null)
        {
            animationController.PlayShoot();
        }
    }

    // ─────────────────────────────────────────────
    // TAKING DAMAGE
    // ─────────────────────────────────────────────

    // Called whenever an asteroid or enemy touches the player.
    // 'attacker' is the object that hit us, so a shield can destroy it.
    public void TakeHit(GameObject attacker)
    {
        if (!isAlive) return;

        // Ignore hits once the game is already over or won
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            return;
        }

        if (PowerUpManager.Instance != null && PowerUpManager.Instance.IsShieldActive)
        {
            AbsorbHitWithShield(attacker);
        }
        else
        {
            Die();
        }
    }

    // The shield blocks the hit, is used up, and destroys whatever hit us
    private void AbsorbHitWithShield(GameObject attacker)
    {
        Debug.Log($"PlayerController: Shield absorbed a hit from {attacker.name}!");

        PowerUpManager.Instance.ConsumeShield();
        Destroy(attacker);

        AudioManager.Play(AudioManager.SoundType.PlayerHit);
        ShakeCamera(shieldShakeDuration, shieldShakeMagnitude);
    }

    private void Die()
    {
        isAlive = false;
        Debug.Log("PlayerController: Player has been killed!");

        if (animationController != null)
        {
            animationController.PlayHit();
        }

        AudioManager.Play(AudioManager.SoundType.PlayerHit);
        ShakeCamera(deathShakeDuration, deathShakeMagnitude);

        GameManager.Instance.TriggerGameOver();
    }

    // Shared helper so shake calls are null-safe in one place
    private void ShakeCamera(float duration, float magnitude)
    {
        if (cameraShake != null)
        {
            cameraShake.Shake(duration, magnitude);
        }
    }

    // ─────────────────────────────────────────────
    // COLLISION DETECTION
    // ─────────────────────────────────────────────

    // All hazard collisions are handled here in ONE place
    // (Asteroid.cs and EnemyAI.cs no longer duplicate this logic)
    private void OnCollisionEnter(Collision collision)
    {
        HandleHazardCollision(collision);
    }

    private void HandleHazardCollision(Collision collision)
    {
        if (collision.gameObject.CompareTag("Asteroid"))
        {
            Debug.Log("PlayerController: Hit by asteroid!");
            TakeHit(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("PlayerController: Hit by enemy!");
            TakeHit(collision.gameObject);
        }
    }
}