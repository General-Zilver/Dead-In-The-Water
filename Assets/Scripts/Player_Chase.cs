using UnityEngine;

public sealed class EnemyFollow : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 5f;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        // 1. Calculate direction to player (horizontal only)
        Vector2 direction = (Vector2)player.position - (Vector2)transform.position;
        direction.Normalize();

        // 2. Move toward player
        transform.position = Vector2.MoveTowards(
            transform.position,
            player.position,
            moveSpeed * Time.deltaTime
        );

        // 3. Flip sprite to face the player instead of rotating
        if (direction.x < 0)
            spriteRenderer.flipX = true;  // facing left
        else
            spriteRenderer.flipX = false; // facing right
    }
}