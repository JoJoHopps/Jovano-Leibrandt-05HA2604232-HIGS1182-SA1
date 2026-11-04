using UnityEngine;
using System.Collections;

// PlayerController handles all player input, movement, and shooting.
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Shooting Settings")]
    [SerializeField] private float shootRange = 50f;
    [SerializeField] private float fireRate = 0.3f;
    [SerializeField] private LayerMask shootableLayers; // Covers BOTH Asteroid and Enemy layers
    [SerializeField] private int damagePerShot = 1;
    [SerializeField] private float laserDuration = 0.05f;
    [SerializeField] private Color laserColor = Color.green;
    [SerializeField] private GameObject impactEffectPrefab; // Visual effect spawned where the raycast hits

    [Header("References")]
    [SerializeField] private Camera playerCamera;

    private Rigidbody rb;
    private float nextFireTime = 0f;
    private bool isAlive = true;
    private float horizontalInput;
    private float verticalInput;
    private LineRenderer laserLine;
    private Vector3 mouseAimDirection;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        SetupLaserLine();

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

    private void SetupLaserLine()
    {
        laserLine = gameObject.AddComponent<LineRenderer>();
        laserLine.positionCount = 2;
        laserLine.startWidth = 0.05f;
        laserLine.endWidth = 0.05f;
        laserLine.material = new Material(Shader.Find("Sprites/Default"));
        laserLine.startColor = laserColor;
        laserLine.endColor = laserColor;
        laserLine.useWorldSpace = true;
        laserLine.enabled = false;
    }

    private void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void ApplyMovement()
    {
        Vector3 moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;
        Vector3 newPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

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
            nextFireTime = Time.time + fireRate;
            FireRaycast();
        }
    }

    private void FireRaycast()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        Vector3 rayDirection = mouseAimDirection;
        Vector3 laserEndPoint = rayOrigin + rayDirection * shootRange;

        Debug.Log("PlayerController: Shot fired!");
        Debug.DrawRay(rayOrigin, rayDirection * shootRange, Color.red, 1f);

        // Raycast now checks BOTH Asteroid and Enemy layers via shootableLayers
        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, shootRange, shootableLayers))
        {
            Debug.Log($"PlayerController: Raycast hit {hit.collider.gameObject.name}");
            laserEndPoint = hit.point;
            HandleHit(hit);
            SpawnImpactEffect(hit.point);
        }
        else
        {
            Debug.Log("PlayerController: Raycast hit nothing.");
        }

        ShowLaser(rayOrigin, laserEndPoint);
    }

    // Decides what happens based on WHAT was hit - distinguishes Asteroid from Enemy using tags
    private void HandleHit(RaycastHit hit)
    {
        if (hit.collider.CompareTag("Asteroid"))
        {
            Debug.Log($"PlayerController: Asteroid destroyed - {hit.collider.gameObject.name}");
            Destroy(hit.collider.gameObject);
        }
        else if (hit.collider.CompareTag("Enemy"))
        {
            Debug.Log($"PlayerController: Enemy hit - {hit.collider.gameObject.name}");

            EnemyAI enemy = hit.collider.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(damagePerShot);
            }
        }
        else
        {
            Debug.Log($"PlayerController: Hit object has no combat logic - tag: {hit.collider.tag}");
        }
    }

    // Spawns a short-lived visual effect at the point the raycast hit something
    private void SpawnImpactEffect(Vector3 position)
    {
        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, position, Quaternion.identity);
        }
    }

    private void ShowLaser(Vector3 startPoint, Vector3 endPoint)
    {
        laserLine.SetPosition(0, startPoint);
        laserLine.SetPosition(1, endPoint);
        laserLine.enabled = true;
        StartCoroutine(HideLaserAfterDelay());
    }

    private IEnumerator HideLaserAfterDelay()
    {
        yield return new WaitForSeconds(laserDuration);
        laserLine.enabled = false;
    }

    public void KillPlayer()
    {
        if (!isAlive) return;

        isAlive = false;
        Debug.Log("PlayerController: Player has been killed!");

        GameManager.Instance.TriggerGameOver();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Asteroid"))
        {
            Debug.Log("PlayerController: Hit by asteroid!");
            KillPlayer();
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("PlayerController: Hit by enemy!");
            KillPlayer();
        }
    }
}