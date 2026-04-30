using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float pullSpeedMultiplier = 3f;
    [SerializeField] private float momentumSpeedMultiplier = 3f;
    [SerializeField] private float pullReleaseDistancePercent = 0.25f;
    [SerializeField] private float momentumDuration = 3f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;

    private enum PullState { None, Pulling, Momentum }
    private PullState pullState = PullState.None;
    private Transform pullTarget;
    private Vector2 pullTargetFallback;
    private float initialPullDistance;
    private Vector2 lastPullDirection;
    private float momentumTimer;

    // True only while the player is being actively pulled toward the target
    public bool IsPulling => pullState == PullState.Pulling;

    // True during the boosted movement window after the pull ends
    public bool IsInGrappleMomentum => pullState == PullState.Momentum;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    // Read player input every frame
    private void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;

        if (animator != null)
        {
            //animator.SetFloat("Speed", moveInput.sqrMagnitude);
        }
    }

    // Apply movement or pull velocity each physics step
    private void FixedUpdate()
    {
        switch (pullState)
        {
            case PullState.None:
                rb.linearVelocity = moveInput * moveSpeed;
                break;
            case PullState.Pulling:
                HandlePulling();
                break;
            case PullState.Momentum:
                HandleMomentum();
                break;
        }
    }

    // Pull player toward target at pullSpeedMultiplier speed, player input adds on top
    private void HandlePulling()
    {
        Vector2 targetPos = pullTarget != null ? (Vector2)pullTarget.position : pullTargetFallback;
        if (pullTarget != null)
            pullTargetFallback = (Vector2)pullTarget.position;

        float remaining = Vector2.Distance(rb.position, targetPos);

        if (remaining <= initialPullDistance * pullReleaseDistancePercent)
        {
            momentumTimer = momentumDuration;
            pullState = PullState.Momentum;
            return;
        }

        Vector2 pullDir = (targetPos - rb.position).normalized;
        // Store every frame so it is fresh when we enter momentum
        lastPullDirection = pullDir;
        rb.linearVelocity = pullDir * (moveSpeed * pullSpeedMultiplier) + moveInput * moveSpeed;
    }

    // Give the player boosted speed for momentumDuration seconds but let them steer freely
    private void HandleMomentum()
    {
        momentumTimer -= Time.fixedDeltaTime;
        if (momentumTimer <= 0f)
        {
            pullState = PullState.None;
            return;
        }

        // If the player is pressing a direction, move that way at boosted speed
        // If not pressing anything, keep coasting in the last pull direction
        if (moveInput.sqrMagnitude > 0f)
            rb.linearVelocity = moveInput * (moveSpeed * momentumSpeedMultiplier);
        else
            rb.linearVelocity = lastPullDirection * (moveSpeed * momentumSpeedMultiplier);
    }

    // Called by the harpoon projectile when it hits a PullNPC
    public void StartPull(Transform target)
    {
        if (target == null) return;
        pullTarget = target;
        pullTargetFallback = (Vector2)target.position;
        initialPullDistance = Vector2.Distance(rb.position, (Vector2)target.position);
        pullState = PullState.Pulling;
    }
}
