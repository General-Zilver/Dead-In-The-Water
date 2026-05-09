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
    [SerializeField] private float visualScale = 1.25f;
    [SerializeField] private RuntimeAnimatorController chargingBackgroundController;
    [SerializeField] private RuntimeAnimatorController dashingAnimationController;
    [SerializeField] private float chargingBackgroundScaleMultiplier = 2f;
    [SerializeField] private float dashingScaleMultiplier = 1f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator visualAnimator;
    private RuntimeAnimatorController swimAnimationController;
    private GameObject chargingBackgroundObject;
    private SpriteRenderer chargingBackgroundRenderer;
    private SwordFishState currentState = SwordFishState.Chase;
    private float stateTimer;
    private Vector2 dashDirection;
    private Vector2 regularSpriteSize;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = EnemyVisualScaler.CreateScaledVisual(gameObject, visualScale);
        visualAnimator = spriteRenderer != null ? spriteRenderer.GetComponent<Animator>() : GetComponentInChildren<Animator>();

        if (visualAnimator != null)
            swimAnimationController = visualAnimator.runtimeAnimatorController;

        if (spriteRenderer != null && spriteRenderer.sprite != null)
            regularSpriteSize = spriteRenderer.sprite.bounds.size;
    }

    void LateUpdate()
    {
        if (currentState == SwordFishState.Dash)
        {
            MatchRegularVisualSize(spriteRenderer, dashingScaleMultiplier);
            RotateVisualToDirection(dashDirection);
        }

        if (chargingBackgroundObject != null && chargingBackgroundObject.activeSelf)
            MatchRegularVisualSize(chargingBackgroundRenderer, chargingBackgroundScaleMultiplier);
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
        {
            currentState = SwordFishState.Chase;
            SetDashingAnimation(false);
            ResetVisualRotation();
        }
    }

    void StartWindup()
    {
        currentState = SwordFishState.Windup;
        stateTimer = 0f;
        rb.linearVelocity = Vector2.zero;
        ShowChargingBackground(true);
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
        ShowChargingBackground(false);
        SetDashingAnimation(true);
        FaceDirection(dashDirection);
        RotateVisualToDirection(dashDirection);
        Debug.Log("SwordFishMovement: dash started.");
    }

    void StartRecovery()
    {
        currentState = SwordFishState.Recovery;
        stateTimer = 0f;
        rb.linearVelocity = Vector2.zero;
        SetDashingAnimation(false);
        ResetVisualRotation();
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

        if (chargingBackgroundRenderer != null)
            chargingBackgroundRenderer.flipX = spriteRenderer.flipX;
    }

    void ShowChargingBackground(bool visible)
    {
        if (!visible)
        {
            if (chargingBackgroundObject != null)
                chargingBackgroundObject.SetActive(false);

            return;
        }

        if (chargingBackgroundController == null || spriteRenderer == null)
            return;

        if (chargingBackgroundObject == null)
            CreateChargingBackground();

        chargingBackgroundObject.SetActive(true);

        Animator chargingAnimator = chargingBackgroundObject.GetComponent<Animator>();
        if (chargingAnimator != null)
            chargingAnimator.Play(0, 0, 0f);
    }

    void CreateChargingBackground()
    {
        chargingBackgroundObject = new GameObject("ChargingBackground");
        Transform backgroundTransform = chargingBackgroundObject.transform;
        backgroundTransform.SetParent(transform, false);
        backgroundTransform.localPosition = Vector3.zero;
        backgroundTransform.localRotation = Quaternion.identity;
        backgroundTransform.localScale = Vector3.one * visualScale;

        chargingBackgroundRenderer = chargingBackgroundObject.AddComponent<SpriteRenderer>();
        chargingBackgroundRenderer.color = spriteRenderer.color;
        chargingBackgroundRenderer.flipX = spriteRenderer.flipX;
        chargingBackgroundRenderer.flipY = spriteRenderer.flipY;
        chargingBackgroundRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
        chargingBackgroundRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
        chargingBackgroundRenderer.sharedMaterial = spriteRenderer.sharedMaterial;

        Animator chargingAnimator = chargingBackgroundObject.AddComponent<Animator>();
        chargingAnimator.runtimeAnimatorController = chargingBackgroundController;
        chargingAnimator.updateMode = visualAnimator != null ? visualAnimator.updateMode : AnimatorUpdateMode.Normal;
        chargingAnimator.cullingMode = visualAnimator != null ? visualAnimator.cullingMode : AnimatorCullingMode.CullUpdateTransforms;
    }

    void SetDashingAnimation(bool dashing)
    {
        if (visualAnimator == null)
            return;

        if (dashing && dashingAnimationController != null)
        {
            visualAnimator.runtimeAnimatorController = dashingAnimationController;
            MatchRegularVisualSize(spriteRenderer, dashingScaleMultiplier);
            RotateVisualToDirection(dashDirection);
        }
        else if (!dashing && swimAnimationController != null)
        {
            visualAnimator.runtimeAnimatorController = swimAnimationController;
            if (spriteRenderer != null)
                spriteRenderer.transform.localScale = Vector3.one * visualScale;
            ResetVisualRotation();
        }
    }

    void MatchRegularVisualSize(SpriteRenderer rendererToScale, float scaleMultiplier)
    {
        if (rendererToScale == null || rendererToScale.sprite == null || regularSpriteSize.sqrMagnitude <= 0f)
            return;

        Vector2 currentSpriteSize = rendererToScale.sprite.bounds.size;
        if (currentSpriteSize.x <= 0f || currentSpriteSize.y <= 0f)
            return;

        float sizeRatio = Mathf.Min(regularSpriteSize.x / currentSpriteSize.x, regularSpriteSize.y / currentSpriteSize.y);
        rendererToScale.transform.localScale = Vector3.one * visualScale * sizeRatio * scaleMultiplier;
    }

    void RotateVisualToDirection(Vector2 direction)
    {
        if (spriteRenderer == null || direction.sqrMagnitude <= 0.0001f)
            return;

        float angle = Mathf.Atan2(direction.x < 0f ? -direction.y : direction.y, Mathf.Abs(direction.x)) * Mathf.Rad2Deg;
        spriteRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    void ResetVisualRotation()
    {
        if (spriteRenderer != null)
            spriteRenderer.transform.localRotation = Quaternion.identity;
    }
}

public static class EnemyVisualScaler
{
    private const string VisualChildName = "ScaledVisual";

    public static SpriteRenderer CreateScaledVisual(GameObject root, float visualScale)
    {
        if (root == null)
            return null;

        Transform existingVisual = root.transform.Find(VisualChildName);
        if (existingVisual != null)
            return existingVisual.GetComponent<SpriteRenderer>();

        SpriteRenderer rootRenderer = root.GetComponent<SpriteRenderer>();
        if (rootRenderer == null)
            return root.GetComponentInChildren<SpriteRenderer>();

        GameObject visualObject = new GameObject(VisualChildName);
        Transform visualTransform = visualObject.transform;
        visualTransform.SetParent(root.transform, false);
        visualTransform.localPosition = Vector3.zero;
        visualTransform.localRotation = Quaternion.identity;
        visualTransform.localScale = Vector3.one * visualScale;

        SpriteRenderer visualRenderer = visualObject.AddComponent<SpriteRenderer>();
        CopySpriteRenderer(rootRenderer, visualRenderer);
        rootRenderer.enabled = false;

        Animator rootAnimator = root.GetComponent<Animator>();
        if (rootAnimator != null)
        {
            Animator visualAnimator = visualObject.AddComponent<Animator>();
            visualAnimator.runtimeAnimatorController = rootAnimator.runtimeAnimatorController;
            visualAnimator.updateMode = rootAnimator.updateMode;
            visualAnimator.cullingMode = rootAnimator.cullingMode;
            visualAnimator.applyRootMotion = rootAnimator.applyRootMotion;
            visualAnimator.speed = rootAnimator.speed;
            rootAnimator.enabled = false;
        }

        return visualRenderer;
    }

    private static void CopySpriteRenderer(SpriteRenderer source, SpriteRenderer destination)
    {
        destination.sprite = source.sprite;
        destination.color = source.color;
        destination.flipX = source.flipX;
        destination.flipY = source.flipY;
        destination.drawMode = source.drawMode;
        destination.size = source.size;
        destination.tileMode = source.tileMode;
        destination.maskInteraction = source.maskInteraction;
        destination.spriteSortPoint = source.spriteSortPoint;
        destination.sortingLayerID = source.sortingLayerID;
        destination.sortingOrder = source.sortingOrder;
        destination.sharedMaterial = source.sharedMaterial;
    }
}
