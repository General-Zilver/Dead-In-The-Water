using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SwordFishMovement : MonoBehaviour
{
    private enum SwordFishState
    {
        Chase,
        Windup,
        Dash,
        Recovery
    }

    public Transform player;
    public float moveSpeed = 5.5f;
    public float attackRange = 6f;
    public float windupDuration = 2f;
    public float dashSpeed = 12f;
    public float dashDuration = 2f;
    public float recoveryDuration = 1f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private SwordFishState currentState = SwordFishState.Chase;
    private float stateTimer;
    private Vector2 dashDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
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

        switch (currentState)
        {
            case SwordFishState.Chase:
                HandleChase();
                break;
            case SwordFishState.Windup:
                HandleWindup();
                break;
            case SwordFishState.Dash:
                HandleDash();
                break;
            case SwordFishState.Recovery:
                HandleRecovery();
                break;
        }
    }

    void HandleChase()
    {
        Vector2 directionToPlayer = ((Vector2)player.position - rb.position).normalized;
        rb.linearVelocity = directionToPlayer * moveSpeed;
        FaceDirection(directionToPlayer);

        float distanceToPlayer = Vector2.Distance(rb.position, player.position);
        if (distanceToPlayer <= attackRange)
            StartWindup();
    }

    void HandleWindup()
    {
        rb.linearVelocity = Vector2.zero;

        Vector2 directionToPlayer = ((Vector2)player.position - rb.position).normalized;
        FaceDirection(directionToPlayer);

        stateTimer += Time.fixedDeltaTime;
        if (stateTimer < windupDuration)
            return;

        StartDash();
    }

    void HandleDash()
    {
        rb.linearVelocity = Vector2.zero;
        rb.MovePosition(rb.position + dashDirection * dashSpeed * Time.fixedDeltaTime);
        FaceDirection(dashDirection);

        stateTimer += Time.fixedDeltaTime;
        if (stateTimer < dashDuration)
            return;

        StartRecovery();
    }

    void HandleRecovery()
    {
        rb.linearVelocity = Vector2.zero;

        stateTimer += Time.fixedDeltaTime;
        if (stateTimer >= recoveryDuration)
            currentState = SwordFishState.Chase;
    }

    void StartWindup()
    {
        currentState = SwordFishState.Windup;
        stateTimer = 0f;
        rb.linearVelocity = Vector2.zero;
        Debug.Log("SwordFishMovement: windup started.");
    }

    void StartDash()
    {
        Vector2 capturedPlayerPosition = player.position;
        dashDirection = (capturedPlayerPosition - rb.position).normalized;
        if (dashDirection.sqrMagnitude <= 0.01f)
            dashDirection = Vector2.right;

        currentState = SwordFishState.Dash;
        stateTimer = 0f;
        FaceDirection(dashDirection);
        Debug.Log("SwordFishMovement: dash started.");
    }

    void StartRecovery()
    {
        currentState = SwordFishState.Recovery;
        stateTimer = 0f;
        rb.linearVelocity = Vector2.zero;
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
