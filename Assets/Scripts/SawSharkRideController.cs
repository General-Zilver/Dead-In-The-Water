using UnityEngine;

public class SawSharkRideController : MonoBehaviour
{
    [SerializeField] private float rideDuration = 8f;
    [SerializeField] private Vector3 playerOffset = Vector3.zero;

    private Transform player;
    private PlayerGrappleState grappleState;
    private float rideTimer;
    private SpriteRenderer[] playerRenderers;
    private bool[] playerRendererStartStates;
    private SpriteRenderer[] rideRenderers;
    private Vector3 lastPlayerPosition;
    private bool facingLeft;

    public void Initialize(Transform playerTransform, PlayerGrappleState playerGrappleState)
    {
        player = playerTransform;
        grappleState = playerGrappleState;
        rideTimer = rideDuration;
        rideRenderers = GetComponentsInChildren<SpriteRenderer>();
        lastPlayerPosition = player != null ? player.position : transform.position;
        HidePlayerVisuals();
    }

    void Update()
    {
        if (player == null)
        {
            FinishRide();
            return;
        }

        transform.position = player.position + playerOffset;
        UpdateRideFacing();

        rideTimer -= Time.deltaTime;
        if (rideTimer <= 0f)
            FinishRide();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            return;

        if (!other.CompareTag("Enemy"))
            return;

        Enemy_Health enemyHealth = other.GetComponentInParent<Enemy_Health>();
        if (enemyHealth == null)
            enemyHealth = other.GetComponentInChildren<Enemy_Health>();

        if (enemyHealth != null)
            enemyHealth.Die();
        else
            Destroy(other.gameObject);
    }

    void FinishRide()
    {
        RestorePlayerVisuals();

        if (grappleState != null)
            grappleState.EndGrapple();

        Destroy(gameObject);
    }

    void HidePlayerVisuals()
    {
        if (player == null)
            return;

        playerRenderers = player.GetComponentsInChildren<SpriteRenderer>();
        playerRendererStartStates = new bool[playerRenderers.Length];

        for (int i = 0; i < playerRenderers.Length; i++)
        {
            playerRendererStartStates[i] = playerRenderers[i].enabled;
            playerRenderers[i].enabled = false;
        }
    }

    void RestorePlayerVisuals()
    {
        if (playerRenderers == null || playerRendererStartStates == null)
            return;

        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i] != null)
                playerRenderers[i].enabled = playerRendererStartStates[i];
        }
    }

    void UpdateRideFacing()
    {
        Vector3 playerMovement = player.position - lastPlayerPosition;
        lastPlayerPosition = player.position;

        if (Mathf.Abs(playerMovement.x) > 0.01f)
            facingLeft = playerMovement.x < 0f;

        if (rideRenderers == null)
            return;

        for (int i = 0; i < rideRenderers.Length; i++)
        {
            if (rideRenderers[i] != null)
                rideRenderers[i].flipX = facingLeft;
        }
    }
}
