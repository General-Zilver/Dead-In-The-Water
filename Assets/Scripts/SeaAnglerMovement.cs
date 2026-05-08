using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SeaAnglerMovement : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 6.5f;

    [SerializeField] private float lookedAtDotThreshold = 0.7f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private PlayerAim playerAim;

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

        Vector2 directionToPlayer = ((Vector2)player.position - rb.position).normalized;

        if (PlayerIsLookingAtSeaAngler())
        {
            rb.linearVelocity = Vector2.zero;

            // Face away from the player while frozen.
            Vector2 awayFromPlayer = (rb.position - (Vector2)player.position).normalized;
            FaceDirection(awayFromPlayer);
            return;
        }

        rb.linearVelocity = directionToPlayer * moveSpeed;
        FaceDirection(directionToPlayer);
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
