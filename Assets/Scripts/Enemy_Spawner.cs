using UnityEngine;
using System.Collections;

public class Enemy_Spawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;  // Prefab of the enemy you want to spawn
    public int maxEnemies = 5;      // Maximum number of enemies to spawn
    public float spawnRadius = 15f; // Radius within which enemies will spawn
    public float minSpawnDistance = 8f; // Min distance so they don't spawn on top of player


    [Header("Spawn Rate ")]
    public float spawnInterval = 3f; // Time interval between spawns
    public float keyHeldRate = 1f; // Time interval becomes faster when key is in players inventory

    [HideInInspector]
    public bool playerHasKey = false; // This should be set to true when the player picks up the key

    private Transform player;
    private float spawnTimer = 0f;
    private int currentEnemyCount = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (playerHasKey == null) return;

        float currentRate = playerHasKey ? keyHeldRate : spawnInterval;
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= currentRate && currentEnemyCount < maxEnemies)
        {
            SpawnEnemy();
            spawnTimer = 0f;
        }

    }

    void SpawnEnemy()
    {   
        Vector2 spawnPosition = GetSpawnPosition();
        if (spawnPosition == Vector2.zero) return;

        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        currentEnemyCount++;

        // EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        // if (health != null)
        // {
        //     health.spawner = this;
        // }

    }
    public void EnemyDied()
    {
        currentEnemyCount = Mathf.Max(0, currentEnemyCount - 1);
    }

    Vector2 GetSpawnPosition()
    {
        // Try up to 10 times to find a valid spawn point
        for (int i = 0; i < 10; i++)
        {
            // Pick a random angle and distance from the player
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(minSpawnDistance, spawnRadius);
 
            Vector2 candidate = new Vector2(
                player.position.x + Mathf.Cos(angle) * distance,
                player.position.y + Mathf.Sin(angle) * distance
            );
 
            // Make sure it's not inside a wall (no collider hit)
            Collider2D hit = Physics2D.OverlapCircle(candidate, 0.5f);
            if (hit == null)
                return candidate;
        }
 
        return Vector2.zero; // Couldn't find a valid spot
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
