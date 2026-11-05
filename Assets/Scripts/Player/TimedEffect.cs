using UnityEngine;

// TimedEffect is attached to short-lived visual effect prefabs
// (impact flashes, pickup flashes). It grows slightly then removes itself.
public class TimedEffect : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.25f;
    [SerializeField] private float growMultiplier = 2.5f; // Final size relative to starting size

    private Vector3 startScale;
    private float timer = 0f;

    private void Start()
    {
        startScale = transform.localScale;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / lifetime);
        transform.localScale = Vector3.Lerp(startScale, startScale * growMultiplier, progress);
    }
}