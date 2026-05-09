using UnityEngine;
using System.Collections.Generic;

public class ChestSpawner : MonoBehaviour
{
    [SerializeField] private TreasureChest chestPrefab;
    [SerializeField] private TreasureChest[] chestPrefabs;
    [SerializeField] private Transform[] chestSpawnPoints;

    // Spawn the correct number of chests based on difficulty, each at a unique spawn point
    private void Start()
    {
        if (chestPrefab == null && (chestPrefabs == null || chestPrefabs.Length == 0))
        {
            Debug.LogWarning("ChestSpawner: no chest prefabs are assigned.");
            return;
        }

        int count = DifficultyManager.Instance != null ? DifficultyManager.Instance.KeysRequired : 1;

        if (chestSpawnPoints == null || chestSpawnPoints.Length < count)
        {
            Debug.LogWarning("ChestSpawner: not enough spawn points assigned for " + count + " chests.");
            return;
        }

        // Build a list of available spawn point indexes and pull from it without repeating
        List<int> available = new List<int>();
        for (int i = 0; i < chestSpawnPoints.Length; i++)
            available.Add(i);

        for (int chestIdx = 0; chestIdx < count; chestIdx++)
        {
            int pick = Random.Range(0, available.Count);
            int spawnPointIdx = available[pick];
            available.RemoveAt(pick);

            Transform spawnPoint = chestSpawnPoints[spawnPointIdx];
            TreasureChest prefab = GetChestPrefab(chestIdx);
            if (prefab == null)
            {
                Debug.LogWarning("ChestSpawner: no chest prefab assigned for index " + chestIdx + ".");
                continue;
            }

            TreasureChest chest = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            chest.Initialize(chestIdx);
        }

        Debug.Log("ChestSpawner: spawned " + count + " chests.");
    }

    private TreasureChest GetChestPrefab(int index)
    {
        if (chestPrefabs != null && index >= 0 && index < chestPrefabs.Length && chestPrefabs[index] != null)
            return chestPrefabs[index];

        return chestPrefab;
    }
}
