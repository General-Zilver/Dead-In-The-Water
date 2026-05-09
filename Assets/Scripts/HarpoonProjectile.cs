using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class HarpoonProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 18f;
    [SerializeField] private float maxRange = 35f;
    [SerializeField] private float ropeDuration = 3f;
    [SerializeField] private int enemyDamage = 1;

    private Vector2 direction;
    private PlayerController player;
    private GrappleRopeVisual ropeVisual;
    private Vector2 startPosition;
    private Rigidbody2D rb;
    private Collider2D col;
    private bool hasHit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    // Set travel direction, player reference, rope visual, and rotate the sprite to face the direction
    public void Initialize(Vector2 dir, PlayerController playerCtrl, GrappleRopeVisual rope)
    {
        direction = dir.normalized;
        player = playerCtrl;
        ropeVisual = rope;
        startPosition = rb.position;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    // Move the harpoon forward and destroy it if it exceeds max range
    private void FixedUpdate()
    {
        if (hasHit) return;

        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);

        if (Vector2.Distance(rb.position, startPosition) >= maxRange)
            Destroy(gameObject);
    }

    // Kill enemies on hit, or pull the player toward PullNPC targets, with jellyfish getting special delayed death
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        if (other.CompareTag("Player")) return;

        if (other.CompareTag("Enemy"))
        {
            // Stop this projectile from damaging multiple colliders on the same enemy.
            hasHit = true;
            if (col != null)
                col.enabled = false;

            // Search parent first, then children, so the component can live on either the root or a child object
            JellyfishHarpoonTarget jellyfishTarget = other.GetComponentInParent<JellyfishHarpoonTarget>();
            if (jellyfishTarget == null)
                jellyfishTarget = other.GetComponentInChildren<JellyfishHarpoonTarget>();

            if (jellyfishTarget != null)
            {
                Debug.Log("OnHarpooned called on enemy jellyfish.");
                jellyfishTarget.OnHarpooned();
                Destroy(gameObject);
                return;
            }

            Enemy_Health health = other.GetComponentInParent<Enemy_Health>();
            if (health == null)
                health = other.GetComponentInChildren<Enemy_Health>();

            if (health != null)
            {
                health.TakeDamage(enemyDamage, player);
                if (health.IsBeingUsedForSawSharkRide)
                {
                    if (ropeVisual != null)
                        ropeVisual.ShowRope(health.transform);

                    StartCoroutine(HideRopeAndDestroy());
                    return;
                }
            }
            else
            {
                Destroy(other.gameObject);
            }

            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("PullNPC"))
        {
            Debug.Log("PullNPC hit by harpoon.");

            // Search parent first, then children, so the component can live on either the root or a child object
            JellyfishHarpoonTarget jellyfishTarget = other.GetComponentInParent<JellyfishHarpoonTarget>();
            if (jellyfishTarget == null)
                jellyfishTarget = other.GetComponentInChildren<JellyfishHarpoonTarget>();

            Debug.Log("JellyfishHarpoonTarget found: " + (jellyfishTarget != null));
            Debug.Log("ropeVisual is null: " + (ropeVisual == null));

            if (jellyfishTarget != null)
            {
                // Lock the harpoon in place and prevent any further collision handling
                hasHit = true;
                if (col != null)
                    col.enabled = false;

                bool pullStarted = player != null && player.StartPull(jellyfishTarget.transform, () =>
                {
                    if (jellyfishTarget != null)
                        jellyfishTarget.DespawnNow();
                });

                Debug.Log("OnHarpooned called on PullNPC jellyfish.");
                jellyfishTarget.OnHarpooned();

                if (!pullStarted)
                {
                    Debug.Log("PullNPC grapple blocked because another grapple is active.");
                    Destroy(gameObject);
                    return;
                }

                if (ropeVisual != null)
                {
                    ropeVisual.ShowRope(jellyfishTarget.transform);
                    Debug.Log("Grapple rope shown.");
                }

                Debug.Log("PullNPC latched.");
                StartCoroutine(HideRopeAndDestroy());
                return;
            }

            // Fallback for PullNPC objects that are not jellyfish
            if (player != null)
                player.StartPull(other.transform);

            Destroy(gameObject);
            return;
        }
    }

    // Wait until the player finishes the pull phase, then hide the rope and destroy the harpoon
    // ropeDuration acts as a safety timeout so this cannot run forever
    private IEnumerator HideRopeAndDestroy()
    {
        float timer = 0f;
        float allowedDuration = ropeDuration;
        if (player != null && player.IsSawSharkPulling)
            allowedDuration = ropeDuration + 1f;

        while (player != null && player.IsPulling && timer < allowedDuration)
        {
            yield return null;
            timer += Time.deltaTime;
        }

        bool timedOutStillPulling = player != null && player.IsPulling;

        if (ropeVisual != null)
        {
            ropeVisual.HideRope();
            Debug.Log("Grapple rope hidden.");
        }

        if (timedOutStillPulling)
        {
            Debug.Log("Grapple pull timed out. Cancelling grapple state.");
            player.CancelGrapple();
        }

        Destroy(gameObject);
    }
}
