using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SharkMovement : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

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
        rb.linearVelocity = directionToPlayer * moveSpeed;
        FaceDirection(directionToPlayer);
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
