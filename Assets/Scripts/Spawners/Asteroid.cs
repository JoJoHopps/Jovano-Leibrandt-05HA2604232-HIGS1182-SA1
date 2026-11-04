using UnityEngine;

// Asteroid.cs handles asteroid boundary wrapping and collision with the player.
// It also notifies the AsteroidSpawner when it is destroyed.
public class Asteroid : MonoBehaviour
{
    [Header("Boundary Settings")]
    [SerializeField] private float boundaryX = 22f;
    [SerializeField] private float boundaryZ = 22f;

    private void Update()
    {
        WrapPosition();
    }

    // Wraps the asteroid to the opposite side of the play area when it goes out of bounds
    private void WrapPosition()
    {
        Vector3 pos = transform.position;

        if (pos.x > boundaryX) pos.x = -boundaryX;
        else if (pos.x < -boundaryX) pos.x = boundaryX;

        if (pos.z > boundaryZ) pos.z = -boundaryZ;
        else if (pos.z < -boundaryZ) pos.z = boundaryZ;

        transform.position = pos;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Asteroid: Collided with Player!");

            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.KillPlayer();
            }
        }
    }

    // Unity automatically calls OnDestroy whenever this GameObject is destroyed,
    // whether from being shot or anything else. We use this to tell the spawner
    // to reduce its active asteroid count.
    private void OnDestroy()
    {
        if (AsteroidSpawner.Instance != null)
        {
            AsteroidSpawner.Instance.OnAsteroidDestroyed();
        }
    }
}