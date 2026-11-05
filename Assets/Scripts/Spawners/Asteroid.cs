using UnityEngine;

// Asteroid keeps asteroids inside the play area by wrapping them to the
// opposite side, and tells the spawner when one is destroyed.
// Player collision is handled in PlayerController.
public class Asteroid : MonoBehaviour
{
    [Header("Boundary Settings")]
    [SerializeField] private float boundaryX = 22f;
    [SerializeField] private float boundaryZ = 22f;

    private void Update()
    {
        WrapPosition();
    }

    private void WrapPosition()
    {
        Vector3 pos = transform.position;

        if (pos.x > boundaryX) pos.x = -boundaryX;
        else if (pos.x < -boundaryX) pos.x = boundaryX;

        if (pos.z > boundaryZ) pos.z = -boundaryZ;
        else if (pos.z < -boundaryZ) pos.z = boundaryZ;

        transform.position = pos;
    }

    private void OnDestroy()
    {
        if (AsteroidSpawner.Instance != null)
        {
            AsteroidSpawner.Instance.OnAsteroidDestroyed();
        }
    }
}