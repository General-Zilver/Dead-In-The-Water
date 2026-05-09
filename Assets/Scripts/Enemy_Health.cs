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

    [Header("Death Animation")]
    [SerializeField] private RuntimeAnimatorController deathAnimationController;
    [SerializeField] private float deathAnimationDuration = 2f;

    [HideInInspector]
    public Enemy_Spawner spawner;

    private int currentHealth;
    private bool isDead;
    private PlayerController pendingSawSharkRidePlayer;
    private EnemyType enemyType = EnemyType.Shark;

    public bool IsBeingUsedForSawSharkRide { get; private set; }

    // Set starting health as soon as the enemy is created.
    void Awake()
    {
        currentHealth = maxHealth;

        EnemyIdentity identity = GetComponent<EnemyIdentity>();
        if (identity == null)
            identity = GetComponentInChildren<EnemyIdentity>();

        if (identity != null)
            enemyType = identity.enemyType;
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

        PlayerController sawSharkPullPlayer = player;
        if (!player.StartSawSharkPull(transform, () =>
        {
            if (this != null)
                StartSawSharkRide();
            else if (sawSharkPullPlayer != null)
                sawSharkPullPlayer.CancelGrapple();
        }))
            return false;

        isDead = true;
        IsBeingUsedForSawSharkRide = true;
        pendingSawSharkRidePlayer = player;
        StopEnemyMovementForSawSharkPull();

        if (spawner != null)
            spawner.EnemyDied(enemyType);

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

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = false;
    }

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (spawner != null)
            spawner.EnemyDied(enemyType);

        // Tell the game manager an enemy died so it can check if a key should now spawn
        if (TreasureGameManager.Instance != null)
            TreasureGameManager.Instance.RegisterEnemyKilled(transform.position);

        AwardScoreAndSpawnPopup();
        SpawnDeathAnimation();

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

    void SpawnDeathAnimation()
    {
        if (deathAnimationController == null)
            return;

        SpriteRenderer sourceRenderer = GetComponentInChildren<SpriteRenderer>();
        if (sourceRenderer == null)
            return;

        GameObject deathObject = new GameObject(gameObject.name + " Death Animation");
        deathObject.transform.position = sourceRenderer.transform.position;
        deathObject.transform.rotation = sourceRenderer.transform.rotation;
        deathObject.transform.localScale = sourceRenderer.transform.lossyScale;

        SpriteRenderer deathRenderer = deathObject.AddComponent<SpriteRenderer>();
        deathRenderer.sprite = sourceRenderer.sprite;
        deathRenderer.color = sourceRenderer.color;
        deathRenderer.flipX = sourceRenderer.flipX;
        deathRenderer.flipY = sourceRenderer.flipY;
        deathRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        deathRenderer.sortingOrder = sourceRenderer.sortingOrder;
        deathRenderer.sharedMaterial = sourceRenderer.sharedMaterial;

        Animator animator = deathObject.AddComponent<Animator>();
        animator.runtimeAnimatorController = deathAnimationController;

        DeathAnimationFade fade = deathObject.AddComponent<DeathAnimationFade>();
        fade.Initialize(deathAnimationDuration, sourceRenderer.bounds.size);
    }
}
