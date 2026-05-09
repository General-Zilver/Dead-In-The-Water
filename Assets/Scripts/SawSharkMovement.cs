using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SawSharkMovement : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 5f;
    [SerializeField] private float attackRange = 15f;
    [SerializeField] private RuntimeAnimatorController attackAnimationController;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private RuntimeAnimatorController swimAnimationController;
    private bool isPlayingAttackAnimation;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();

        if (animator != null)
            swimAnimationController = animator.runtimeAnimatorController;
    }

    void Start()
    {
        FindPlayerIfNeeded();
    }

    void FixedUpdate()
    {
        FindPlayerIfNeeded();

        if (player == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 toPlayer = (Vector2)player.position - rb.position;
        Vector2 directionToPlayer = toPlayer.normalized;
        UpdateAttackAnimation(toPlayer.magnitude <= attackRange);

        rb.linearVelocity = directionToPlayer * moveSpeed;
        FaceDirection(directionToPlayer);
    }

    void UpdateAttackAnimation(bool shouldPlayAttack)
    {
        if (animator == null || isPlayingAttackAnimation == shouldPlayAttack)
            return;

        isPlayingAttackAnimation = shouldPlayAttack;
        if (shouldPlayAttack && attackAnimationController != null)
            animator.runtimeAnimatorController = attackAnimationController;
        else if (!shouldPlayAttack && swimAnimationController != null)
            animator.runtimeAnimatorController = swimAnimationController;
    }

    void FindPlayerIfNeeded()
    {
        if (player != null)
            return;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;
    }

    void FaceDirection(Vector2 direction)
    {
        if (spriteRenderer == null || Mathf.Abs(direction.x) < 0.01f)
            return;

        spriteRenderer.flipX = direction.x < 0f;
    }
}
