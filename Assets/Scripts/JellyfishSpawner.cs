using UnityEngine;
using System.Collections.Generic;

public class JellyfishSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject bluePullJellyfishPrefab;
    [SerializeField] private GameObject redEnemyJellyfishPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnIntervalMin = 2f;
    [SerializeField] private float spawnIntervalMax = 5f;
    [SerializeField] private int maxAlive = 9;
    [Range(0f, 1f)]
    [SerializeField] private float blueSpawnChance = 1f;

    [Header("Map Bounds")]
    [SerializeField] private float mapMinX = -107f;
    [SerializeField] private float mapMaxX = 138f;

    [Header("Spawn Lanes")]
    // Each entry is a fixed Y position where jellyfish can swim across the screen
    // Having fixed lanes prevents jellyfish from stacking up at the same Y value
    [SerializeField] private float[] spawnYLanes = { -16f, -12f, -8f, -4f, 0f, 4f, 8f, 12f, 16f, 20f, 24f };

    [Header("Player View Exclusion")]
    [SerializeField] private float viewHalfWidth = 30f;
    [SerializeField] private float viewOffsetYMin = -12f;
    [SerializeField] private float viewOffsetYMax = 22f;

    private Transform player;
    private int aliveCount = 0;
    private float spawnTimer = 0f;
    private float nextSpawnInterval;

    // Stores the index of each lane that currently has a living jellyfish in it
    // Using the index rather than the Y float value makes adding and removing entries reliable
    private HashSet<int> occupiedLaneIndexes = new HashSet<int>();

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        nextSpawnInterval = Random.Range(spawnIntervalMin, spawnIntervalMax);
    }

    // Count down to next spawn and attempt to place a jellyfish when ready
    private void Update()
    {
        if (player == null) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= nextSpawnInterval && aliveCount < maxAlive)
        {
            TrySpawn();
            spawnTimer = 0f;
            nextSpawnInterval = Random.Range(spawnIntervalMin, spawnIntervalMax);
        }
    }

    // Build the list of free lanes then try up to 20 times to find one outside the player view box
    private void TrySpawn()
    {
        List<int> available = new List<int>();
        for (int i = 0; i < spawnYLanes.Length; i++)
        {
            if (!occupiedLaneIndexes.Contains(i))
                available.Add(i);
        }

        if (available.Count == 0) return;

        for (int attempt = 0; attempt < 20; attempt++)
        {
            int laneIdx = available[Random.Range(0, available.Count)];
            float spawnY = spawnYLanes[laneIdx];

            bool fromLeft = Random.value < 0.5f;
            float spawnX = fromLeft ? mapMinX : mapMaxX;
            Vector2 candidate = new Vector2(spawnX, spawnY);

            if (IsInPlayerView(candidate))
                continue;

            SpawnAt(candidate, fromLeft, laneIdx);
            return;
        }
    }

    // Check whether a position falls inside the estimated player view box
    private bool IsInPlayerView(Vector2 pos)
    {
        float px = player.position.x;
        float py = player.position.y;

        return pos.x > px - viewHalfWidth && pos.x < px + viewHalfWidth
            && pos.y > py + viewOffsetYMin && pos.y < py + viewOffsetYMax;
    }

    // Pick a prefab, spawn the jellyfish, and mark the lane as occupied until it dies or swims away
    private void SpawnAt(Vector2 position, bool fromLeft, int laneIdx)
    {
        bool spawnBlue = Random.value <= blueSpawnChance;
        GameObject prefab = spawnBlue ? bluePullJellyfishPrefab : redEnemyJellyfishPrefab;
        if (prefab == null)
            prefab = spawnBlue ? redEnemyJellyfishPrefab : bluePullJellyfishPrefab;
        if (prefab == null) return;

        GameObject jellyfish = Instantiate(prefab, position, Quaternion.identity);
        JellyfishMover mover = jellyfish.GetComponent<JellyfishMover>();
        if (mover != null)
        {
            Vector2 direction = fromLeft ? Vector2.right : Vector2.left;
            float despawnX = fromLeft ? mapMaxX : mapMinX;
            mover.Initialize(direction, despawnX, this, laneIdx);
        }

        occupiedLaneIndexes.Add(laneIdx);
        aliveCount++;
    }

    // Called by JellyfishMover on destroy, releases the lane index so the next jellyfish can use that Y row
    public void OnJellyfishDespawned(int laneIdx)
    {
        aliveCount = Mathf.Max(0, aliveCount - 1);
        occupiedLaneIndexes.Remove(laneIdx);
    }
}
