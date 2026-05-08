using UnityEngine;

public class Enemy_Health : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;

    [Header("SawShark Ride")]
    [SerializeField] private GameObject sawSharkRidePrefab;

    [Header("Score Popup")]
    [SerializeField] private GameObject scorePopupPrefab;
    [SerializeField] private Vector3 scorePopupOffset = Vector3.zero;

    [HideInInspector]
    public Enemy_Spawner spawner;

    private int currentHealth;
    private bool isDead;
    private PlayerController pendingSawSharkRidePlayer;

    public bool IsBeingUsedForSawSharkRide { get; private set; }

    // Set starting health as soon as the enemy is created.
    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        TakeDamage(amount, null);
    }

    public void TakeDamage(int amount, PlayerController damageSourcePlayer)
    {
        if (isDead)
            return;

        if (amount <= 0)
            return;

        currentHealth -= amount;
        Debug.Log(gameObject.name + " took " + amount + " damage. Health: " + currentHealth + "/" + maxHealth);

        if (currentHealth <= 0)
        {
            if (damageSourcePlayer != null && TryStartSawSharkPullAndRide(damageSourcePlayer))
                return;

            Die();
        }
    }

    bool TryStartSawSharkPullAndRide(PlayerController player)
    {
        EnemyIdentity identity = GetComponent<EnemyIdentity>();
        if (identity == null)
            identity = GetComponentInChildren<EnemyIdentity>();

        if (identity == null || identity.enemyType != EnemyType.SawShark)
            return false;

        if (sawSharkRidePrefab == null)
        {
            Debug.LogWarning("Enemy_Health: sawSharkRidePrefab is not assigned. SawShark will die normally.");
            return false;
        }

        if (!player.StartSawSharkPull(transform, StartSawSharkRide))
            return false;

        isDead = true;
        IsBeingUsedForSawSharkRide = true;
        pendingSawSharkRidePlayer = player;
        StopEnemyMovementForSawSharkPull();

        if (spawner != null)
            spawner.EnemyDied();

        if (TreasureGameManager.Instance != null)
            TreasureGameManager.Instance.RegisterEnemyKilled(transform.position);

        AwardScoreAndSpawnPopup();

        Debug.Log("SawShark defeated. Pulling player to SawShark before starting ride.");
        return true;
    }

    void StartSawSharkRide()
    {
        PlayerController player = pendingSawSharkRidePlayer;
        PlayerGrappleState grappleState = player != null ? player.GetComponent<PlayerGrappleState>() : null;

        if (player == null)
        {
            if (grappleState != null)
                grappleState.EndGrapple();

            Destroy(gameObject);
            return;
        }

        GameObject rideObject = Instantiate(sawSharkRidePrefab, player.transform.position, Quaternion.identity);
        SawSharkRideController rideController = rideObject.GetComponent<SawSharkRideController>();
        if (rideController != null)
        {
            rideController.Initialize(player.transform, grappleState);
        }
        else
        {
            Debug.LogWarning("Enemy_Health: sawSharkRidePrefab is missing SawSharkRideController.");
            if (grappleState != null)
                grappleState.EndGrapple();
        }

        Destroy(gameObject);
    }

    void StopEnemyMovementForSawSharkPull()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] != this)
                behaviours[i].enabled = false;
        }
    }

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (spawner != null)
            spawner.EnemyDied();

        // Tell the game manager an enemy died so it can check if a key should now spawn
        if (TreasureGameManager.Instance != null)
            TreasureGameManager.Instance.RegisterEnemyKilled(transform.position);

        AwardScoreAndSpawnPopup();

        Destroy(gameObject);
    }

    void AwardScoreAndSpawnPopup()
    {
        EnemyIdentity identity = GetComponent<EnemyIdentity>();
        int points = (identity != null) ? identity.scoreValue : 50;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(points);
        }

        SpawnScorePopup();
    }

    void SpawnScorePopup()
    {
        if (scorePopupPrefab == null)
            return;

        Instantiate(scorePopupPrefab, transform.position + scorePopupOffset, Quaternion.identity);
    }
}
