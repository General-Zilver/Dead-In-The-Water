using UnityEngine;
using System.Collections;

public class Enemy_Spawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject sharkPrefab;
    public GameObject sawSharkPrefab;
    public GameObject seaAnglerPrefab;
    public GameObject swordFishPrefab;
    public int maxEnemies = 5;      // Maximum number of enemies
    public float spawnRadius = 15f; // Radius within which enemies will spawn
    public float minSpawnDistance = 8f; // Min distance so they don't spawn on top of player


    [Header("Spawn Rate ")]
    public float spawnInterval = 3f; // Time interval between spawns
    public float keyHeldRate = 1f; // Time interval becomes faster when key is in players inventory
    [SerializeField] private bool logSpawnRolls = false;

    [HideInInspector]
    public bool playerHasKey = false;

    private Transform player;
    private float spawnTimer = 0f;
    private int currentEnemyCount = 0;
    private bool suddenDeathActive = false;


    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // Override Inspector values with difficulty-based values if DifficultyManager is present
        if (DifficultyManager.Instance != null)
        {
            spawnInterval = DifficultyManager.Instance.SpawnRate;
            keyHeldRate   = DifficultyManager.Instance.KeyHeldSpawnRate;
            maxEnemies    = DifficultyManager.Instance.MaxEnemies;
        }
    }

    void Update()
    {
        if (player == null) return;

        // During sudden death, halve the spawn interval and double the enemy cap
        float currentRate = suddenDeathActive ? spawnInterval * 0.5f : (playerHasKey ? keyHeldRate : spawnInterval);
        spawnTimer += Time.deltaTime;

        int enemyCap = suddenDeathActive ? maxEnemies * 2 : maxEnemies;
        bool underLimit = currentEnemyCount < enemyCap;
        if (spawnTimer >= currentRate && underLimit)
        {
            SpawnEnemy();
            spawnTimer = 0f;
        }

    }

    void SpawnEnemy()
    {   
        Vector2 spawnPosition = GetSpawnPosition();
        if (spawnPosition == Vector2.zero) return;

        GameObject prefabToSpawn = ChooseEnemyPrefab();
        if (prefabToSpawn == null)
            return;

        GameObject enemy = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

        currentEnemyCount++;

        Enemy_Health health = enemy.GetComponent<Enemy_Health>();
        if (health == null)
            health = enemy.GetComponentInChildren<Enemy_Health>();

        if (health != null)
            health.spawner = this;

    }

    GameObject ChooseEnemyPrefab()
    {
        EnemySpawnWeights weights = DifficultyManager.Instance != null
            ? DifficultyManager.Instance.CurrentEnemyWeights
            : new EnemySpawnWeights
            {
                sharkWeight = 85f,
                sawSharkWeight = 5f,
                seaAnglerWeight = 5f,
                swordFishWeight = 5f
            };

        float sharkWeight = Mathf.Max(0f, weights.sharkWeight);
        float sawSharkWeight = Mathf.Max(0f, weights.sawSharkWeight);
        float seaAnglerWeight = Mathf.Max(0f, weights.seaAnglerWeight);
        float swordFishWeight = Mathf.Max(0f, weights.swordFishWeight);
        float totalWeight = sharkWeight + sawSharkWeight + seaAnglerWeight + swordFishWeight;

        if (totalWeight <= 0f)
        {
            Debug.LogWarning("Enemy_Spawner: enemy spawn weights are all zero. Falling back to Shark.");
            return sharkPrefab;
        }

        float roll = Random.Range(0f, totalWeight);
        GameObject selectedPrefab;
        string selectedName;

        if (roll < sharkWeight)
        {
            selectedPrefab = sharkPrefab;
            selectedName = "Shark";
        }
        else if (roll < sharkWeight + sawSharkWeight)
        {
            selectedPrefab = sawSharkPrefab;
            selectedName = "SawShark";
        }
        else if (roll < sharkWeight + sawSharkWeight + seaAnglerWeight)
        {
            selectedPrefab = seaAnglerPrefab;
            selectedName = "SeaAngler";
        }
        else
        {
            selectedPrefab = swordFishPrefab;
            selectedName = "SwordFish";
        }

        if (selectedPrefab != null)
        {
            if (logSpawnRolls)
                Debug.Log(
                    "Enemy_Spawner rolled " + roll.ToString("0.00") +
                    " / " + totalWeight.ToString("0.00") +
                    " on difficulty " + (DifficultyManager.Instance != null ? DifficultyManager.Instance.difficulty : -1) +
                    ", spawning " + selectedName +
                    " with weights Shark=" + sharkWeight +
                    ", SawShark=" + sawSharkWeight +
                    ", SeaAngler=" + seaAnglerWeight +
                    ", SwordFish=" + swordFishWeight);

            return selectedPrefab;
        }

        Debug.LogWarning("Enemy_Spawner: selected " + selectedName + " prefab is missing. Falling back to Shark.");
        if (sharkPrefab == null)
            Debug.LogWarning("Enemy_Spawner: sharkPrefab is also missing, so no enemy can spawn.");

        return sharkPrefab;
    }
    public void EnemyDied()
    {
        currentEnemyCount = Mathf.Max(0, currentEnemyCount - 1);
    }

    // Called by TreasureGameManager when a key is collected, doubles cap and halves interval
    public void StartSuddenDeath()
    {
        suddenDeathActive = true;
        spawnTimer = 0f;
        Debug.Log("Enemy_Spawner: sudden death active.");
    }

    // Called by TreasureGameManager when a chest is looted, returns spawning to normal
    public void StopSuddenDeath()
    {
        suddenDeathActive = false;
        Debug.Log("Enemy_Spawner: sudden death deactivated.");
    }

    Vector2 GetSpawnPosition()
    {

        for (int i = 0; i < 10; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(minSpawnDistance, spawnRadius);
 
            Vector2 candidate = new Vector2(
                player.position.x + Mathf.Cos(angle) * distance,
                player.position.y + Mathf.Sin(angle) * distance
            );
 
            Collider2D hit = Physics2D.OverlapCircle(candidate, 0.5f);
            if (hit == null)
                return candidate;
        }
 
        return Vector2.zero;
    }

    void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, spawnRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, minSpawnDistance);
    }


}
