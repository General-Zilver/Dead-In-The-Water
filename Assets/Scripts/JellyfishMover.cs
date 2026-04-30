using UnityEngine;

public class JellyfishMover : MonoBehaviour
{
    [SerializeField] public float speed = 1f;
    [SerializeField] public float bobAmplitude = 0.3f;
    [SerializeField] public float bobFrequency = 1f;
    [SerializeField] private SpriteRenderer visualRenderer;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private float despawnX;
    private JellyfishSpawner spawner;
    private float startY;
    private float bobTime;
    private bool initialized = false;
    private bool stopped = false;
    private bool notifiedSpawner = false;
    private int laneIndex;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Set direction, despawn threshold, spawner reference, lane index, and flip sprite if moving left
    public void Initialize(Vector2 direction, float despawnXPos, JellyfishSpawner jellyfishSpawner, int assignedLaneIndex)
    {
        moveDirection = direction.normalized;
        despawnX = despawnXPos;
        spawner = jellyfishSpawner;
        startY = transform.position.y;
        bobTime = Random.Range(0f, Mathf.PI * 2f);
        laneIndex = assignedLaneIndex;
        initialized = true;

        if (visualRenderer != null)
            visualRenderer.flipX = moveDirection.x < 0;
    }

    // Freeze the jellyfish in place when hit by a harpoon
    public void StopMoving()
    {
        stopped = true;
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    // Notify the spawner once, passing the lane index so the spawner can free that slot for a new jellyfish
    public void NotifySpawner()
    {
        if (notifiedSpawner) return;
        notifiedSpawner = true;
        if (spawner != null)
            spawner.OnJellyfishDespawned(laneIndex);
    }

    // OnDestroy fires whether the jellyfish swam off screen or was killed by a harpoon
    private void OnDestroy()
    {
        NotifySpawner();
    }

    // Move the jellyfish forward with bobbing and check for despawn boundary
    private void FixedUpdate()
    {
        if (!initialized || stopped) return;

        bobTime += Time.fixedDeltaTime;
        float bobOffset = Mathf.Sin(bobTime * bobFrequency * Mathf.PI * 2f) * bobAmplitude;

        Vector2 newPos = rb.position;
        newPos.x += moveDirection.x * speed * Time.fixedDeltaTime;
        newPos.y = startY + bobOffset;
        rb.MovePosition(newPos);

        bool pastBoundary = moveDirection.x > 0 ? rb.position.x >= despawnX : rb.position.x <= despawnX;
        if (pastBoundary)
        {
            NotifySpawner();
            Destroy(gameObject);
        }
    }
}
