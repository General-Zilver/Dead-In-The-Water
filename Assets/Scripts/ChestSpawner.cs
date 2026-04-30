using UnityEngine;
using System.Collections.Generic;

public class ChestSpawner : MonoBehaviour
{
    [SerializeField] private TreasureChest chestPrefab;
    [SerializeField] private Transform[] chestSpawnPoints;

    // Spawn the correct number of chests based on difficulty, each at a unique spawn point
    private void Start()
    {
        if (chestPrefab == null)
        {
            Debug.LogWarning("ChestSpawner: chestPrefab is not assigned.");
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
            TreasureChest chest = Instantiate(chestPrefab, spawnPoint.position, Quaternion.identity);
            chest.Initialize(chestIdx);
        }

        Debug.Log("ChestSpawner: spawned " + count + " chests.");
    }
}
