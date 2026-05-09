using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SeaAnglerMovement : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 6.5f;

    [SerializeField] private float lookedAtDotThreshold = 0.7f;
    [SerializeField] private float visualScale = 1.25f;
    [SerializeField] private float attackRange = 15f;
    [SerializeField] private RuntimeAnimatorController attackAnimationController;
    [SerializeField] private RuntimeAnimatorController hidingAnimationController;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private RuntimeAnimatorController swimAnimationController;
    private PlayerAim playerAim;
    private bool isPlayingAttackAnimation;
    private bool isPlayingHidingAnimation;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = EnemyVisualScaler.CreateScaledVisual(gameObject, visualScale);
        animator = spriteRenderer != null ? spriteRenderer.GetComponent<Animator>() : GetComponentInChildren<Animator>();

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
        bool playerIsLooking = PlayerIsLookingAtSeaAngler();

        if (playerIsLooking)
        {
            rb.linearVelocity = Vector2.zero;
            UpdateHidingAnimation(true);
            return;
        }

        UpdateHidingAnimation(false);
        UpdateAttackAnimation(toPlayer.magnitude <= attackRange);

        rb.linearVelocity = directionToPlayer * moveSpeed;
        FaceDirection(directionToPlayer);
    }

    void UpdateAttackAnimation(bool shouldPlayAttack)
    {
        if (animator == null || isPlayingHidingAnimation || isPlayingAttackAnimation == shouldPlayAttack)
            return;

        isPlayingAttackAnimation = shouldPlayAttack;
        if (shouldPlayAttack && attackAnimationController != null)
            animator.runtimeAnimatorController = attackAnimationController;
        else if (!shouldPlayAttack && swimAnimationController != null)
            animator.runtimeAnimatorController = swimAnimationController;
    }

    void UpdateHidingAnimation(bool shouldPlayHiding)
    {
        if (animator == null || isPlayingHidingAnimation == shouldPlayHiding)
            return;

        isPlayingHidingAnimation = shouldPlayHiding;
        if (shouldPlayHiding && hidingAnimationController != null)
        {
            isPlayingAttackAnimation = false;
            animator.runtimeAnimatorController = hidingAnimationController;
        }
        else
        {
            UpdateAttackAnimation(player != null && Vector2.Distance(rb.position, player.position) <= attackRange);
        }
    }

    void FindPlayerIfNeeded()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }

        if (player != null && playerAim == null)
            playerAim = player.GetComponentInChildren<PlayerAim>();

        if (playerAim == null)
            playerAim = FindFirstObjectByType<PlayerAim>();
    }

    bool PlayerIsLookingAtSeaAngler()
    {
        if (player == null)
            return false;

        Vector2 playerToSeaAngler = ((Vector2)transform.position - (Vector2)player.position).normalized;

        if (playerAim != null && playerAim.HasAimDirection)
        {
            float aimDot = Vector2.Dot(playerAim.AimDirection.normalized, playerToSeaAngler);
            return aimDot >= lookedAtDotThreshold;
        }

        if (playerAim == null)
            return false;

        bool seaAnglerIsRightOfPlayer = transform.position.x > player.position.x;
        return seaAnglerIsRightOfPlayer != playerAim.FacingLeft;
    }

    void FaceDirection(Vector2 direction)
    {
        if (spriteRenderer == null || Mathf.Abs(direction.x) < 0.01f)
            return;

        spriteRenderer.flipX = direction.x < 0f;
    }
}
