using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float pullSpeedMultiplier = 3f;
    [SerializeField] private float momentumSpeedMultiplier = 5f;
    [SerializeField] private float pullReleaseDistancePercent = 0.25f;
    [SerializeField] private float momentumDuration = 6f;
    [SerializeField] private float maxPullDuration = 3f;

    [Header("World Bounds")]
    [SerializeField] private float minX = -80.07f;
    [SerializeField] private float maxX = 112.06f;
    [SerializeField] private float minY = -24.65f;
    [SerializeField] private float maxY = 30.55f;
    [SerializeField] private bool horizontalWrapEnabled = false;
    [SerializeField] private bool useColliderBoundsForClamp = true;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private Collider2D playerCollider;

    private enum PullState { None, Pulling, Momentum }
    private PullState pullState = PullState.None;
    private Transform pullTarget;
    private Vector2 pullTargetFallback;
    private float initialPullDistance;
    private Vector2 lastPullDirection;
    private float momentumTimer;
    private float pullTimer;
    private PlayerGrappleState grappleState;
    private bool startMomentumAfterPull;
    private Action onPullComplete;

    // True only while the player is being actively pulled toward the target
    public bool IsPulling => pullState == PullState.Pulling;

    // True while pulling toward a defeated SawShark before the ride starts
    public bool IsSawSharkPulling => pullState == PullState.Pulling && !startMomentumAfterPull;

    // True during the boosted movement window after the pull ends
    public bool IsInGrappleMomentum => pullState == PullState.Momentum;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        grappleState = GetComponent<PlayerGrappleState>();
        playerCollider = GetComponent<Collider2D>();
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
        pullTimer += Time.fixedDeltaTime;

        Vector2 targetPos = pullTarget != null ? (Vector2)pullTarget.position : pullTargetFallback;
        if (pullTarget != null)
            pullTargetFallback = (Vector2)pullTarget.position;

        float remaining = Vector2.Distance(rb.position, targetPos);

        if (remaining <= initialPullDistance * pullReleaseDistancePercent || pullTimer >= maxPullDuration)
        {
            FinishPull();
            return;
        }

        Vector2 pullDir = (targetPos - rb.position).normalized;
        // Store every frame so it is fresh when we enter momentum
        lastPullDirection = pullDir;
        rb.linearVelocity = pullDir * (moveSpeed * pullSpeedMultiplier) + moveInput * moveSpeed;
    }

    // Give the player boosted speed that fades back to normal while they steer freely
    private void HandleMomentum()
    {
        momentumTimer -= Time.fixedDeltaTime;
        if (momentumTimer <= 0f)
        {
            pullState = PullState.None;
            if (grappleState != null)
                grappleState.EndGrapple();
            return;
        }

        float remainingPercent = Mathf.Clamp01(momentumTimer / momentumDuration);
        float currentMultiplier = Mathf.Lerp(1f, momentumSpeedMultiplier, remainingPercent);

        // If the player is pressing a direction, move that way and remember it.
        // If not pressing anything, keep coasting in the last direction used during momentum.
        if (moveInput.sqrMagnitude > 0f)
        {
            lastPullDirection = moveInput;
            rb.linearVelocity = moveInput * (moveSpeed * currentMultiplier);
        }
        else
        {
            rb.linearVelocity = lastPullDirection * (moveSpeed * currentMultiplier);
        }
    }

    private void LateUpdate()
    {
        KeepPlayerInsideBounds();
    }

    // Called by the harpoon projectile when it hits a PullNPC
    public bool StartPull(Transform target)
    {
        return StartPull(target, null);
    }

    public bool StartPull(Transform target, Action pullCompleteCallback)
    {
        if (target == null) return false;

        if (grappleState != null && !grappleState.TryStartJellyfishPull())
            return false;

        BeginPull(target, true, pullCompleteCallback);
        return true;
    }

    // Called when a SawShark dies from a harpoon. This pulls the player to the SawShark,
    // then starts the ride instead of giving jellyfish momentum.
    public bool StartSawSharkPull(Transform target, Action rideStartCallback)
    {
        if (target == null) return false;

        if (grappleState != null && !grappleState.TryStartSawSharkRide())
            return false;

        BeginPull(target, false, rideStartCallback);
        return true;
    }

    public void CancelGrapple()
    {
        pullState = PullState.None;
        pullTarget = null;
        onPullComplete = null;
        startMomentumAfterPull = false;
        momentumTimer = 0f;
        pullTimer = 0f;

        if (grappleState != null)
            grappleState.EndGrapple();
    }

    void BeginPull(Transform target, bool shouldStartMomentum, Action pullCompleteCallback)
    {
        pullTarget = target;
        pullTargetFallback = (Vector2)target.position;
        initialPullDistance = Vector2.Distance(rb.position, (Vector2)target.position);
        pullTimer = 0f;
        startMomentumAfterPull = shouldStartMomentum;
        onPullComplete = pullCompleteCallback;
        pullState = PullState.Pulling;
    }

    void FinishPull()
    {
        pullTarget = null;
        pullTimer = 0f;
        Action callback = onPullComplete;
        onPullComplete = null;

        if (startMomentumAfterPull)
        {
            startMomentumAfterPull = false;

            if (callback != null)
                callback.Invoke();

            StartMomentum();
            return;
        }

        pullState = PullState.None;
        rb.linearVelocity = Vector2.zero;

        startMomentumAfterPull = false;

        if (callback != null)
            callback.Invoke();
    }

    void StartMomentum()
    {
        pullTarget = null;
        pullTimer = 0f;
        onPullComplete = null;
        startMomentumAfterPull = false;
        momentumTimer = momentumDuration;
        pullState = PullState.Momentum;
    }

    void KeepPlayerInsideBounds()
    {
        Vector2 position = rb.position;
        Vector2 velocity = rb.linearVelocity;
        Vector2 extents = GetBoundaryExtents();

        if (horizontalWrapEnabled)
        {
            if (position.x - extents.x <= minX && velocity.x < 0f)
                position.x = maxX - extents.x;
            else if (position.x + extents.x >= maxX && velocity.x > 0f)
                position.x = minX + extents.x;
        }
        else
        {
            float clampedX = Mathf.Clamp(position.x, minX + extents.x, maxX - extents.x);
            if (!Mathf.Approximately(position.x, clampedX))
                velocity.x = 0f;

            position.x = clampedX;
        }

        float clampedY = Mathf.Clamp(position.y, minY + extents.y, maxY - extents.y);
        if (!Mathf.Approximately(position.y, clampedY))
            velocity.y = 0f;

        position.y = clampedY;

        rb.position = position;
        rb.linearVelocity = velocity;
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }

    Vector2 GetBoundaryExtents()
    {
        if (!useColliderBoundsForClamp || playerCollider == null)
            return Vector2.zero;

        Bounds bounds = playerCollider.bounds;
        return new Vector2(bounds.extents.x, bounds.extents.y);
    }
}
