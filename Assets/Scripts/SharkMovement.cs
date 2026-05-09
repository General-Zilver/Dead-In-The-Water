using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SharkMovement : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 5f;
    [SerializeField] private float attackRange = 15f;
    [SerializeField] private string swimStateName = "Shark_Swim";
    [SerializeField] private string attackStateName = "Shark_Attack";

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool isPlayingAttackAnimation;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
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
        if (shouldPlayAttack)
            animator.Play(attackStateName, 0, 0f);
        else
            animator.Play(swimStateName, 0, 0f);
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
