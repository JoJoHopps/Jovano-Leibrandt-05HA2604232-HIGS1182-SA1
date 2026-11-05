using UnityEngine;

// AnimationController is the ONLY script that talks to the Animator.
// PlayerController just calls these simple methods (SetMoving, PlayShoot, PlayHit)
// and doesn't need to know anything about Animator parameters.
public class AnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    // StringToHash converts parameter names to numbers once, which is faster
    // and avoids typos in repeated string lookups
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int ShootHash = Animator.StringToHash("Shoot");
    private static readonly int HitHash = Animator.StringToHash("Hit");

    private void Awake()
    {
        // The Animator lives on the PlayerModel child object, so search the children
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError("AnimationController: No Animator found on Player or its children!");
        }
    }

    // Idle <-> Thrust animation, driven by whether movement keys are held
    public void SetMoving(bool isMoving)
    {
        if (animator == null) return;
        animator.SetBool(IsMovingHash, isMoving);
    }

    // One-shot shoot animation
    public void PlayShoot()
    {
        if (animator == null) return;
        animator.SetTrigger(ShootHash);
    }

    // One-shot hit / death animation
    public void PlayHit()
    {
        if (animator == null) return;
        animator.SetTrigger(HitHash);
        Debug.Log("AnimationController: Hit/Death animation triggered.");
    }
}