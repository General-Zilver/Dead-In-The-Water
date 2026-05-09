using UnityEngine;
using System.Collections.Generic;

public class TreasureGameManager : MonoBehaviour
{
    public static TreasureGameManager Instance;

    [Header("Key Spawning")]
    [SerializeField] private KeyPickup keyPrefab;

    private int requiredCount;

    // Which key/chest pair the player is currently working toward (0, 1, 2...)
    private int currentObjectiveIndex = 0;

    // Kills accumulated toward the current key, resets when a chest is looted
    private int killsTowardCurrentKey = 0;

    // True while a key is sitting in the world waiting to be picked up
    private bool keyWaitingToBeCollected = false;

    // True after the final key is collected, kills no longer count
    private bool suddenDeathActive = false;

    private HashSet<int> collectedKeyIndexes = new HashSet<int>();
    private HashSet<int> lootedChestIndexes = new HashSet<int>();
    private List<TreasureChest> registeredChests = new List<TreasureChest>();

    // Set up the singleton
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // Read required count from DifficultyManager after it has finished its own Awake
    private void Start()
    {
        if (DifficultyManager.Instance != null)
            requiredCount = DifficultyManager.Instance.KeysRequired;
        else
            requiredCount = 1;

        Debug.Log("TreasureGameManager: requires " + requiredCount + " keys and chests.");
        UpdateKeyUI();
    }

    // Called by Enemy_Health when a real enemy dies
    public void RegisterEnemyKilled(Vector3 deathPosition)
    {
        if (suddenDeathActive)
        {
            Debug.Log("Kill ignored because sudden death is active.");
            return;
        }

        if (keyWaitingToBeCollected)
        {
            Debug.Log("Kill ignored because key is waiting.");
            return;
        }

        if (currentObjectiveIndex >= requiredCount)
            return;

        killsTowardCurrentKey++;

        int required = DifficultyManager.Instance != null
            ? DifficultyManager.Instance.KillsRequiredForNextKey
            : 12;

        Debug.Log("Kill counted toward key " + currentObjectiveIndex + ": " + killsTowardCurrentKey + "/" + required);

        if (killsTowardCurrentKey >= required)
        {
            SpawnKey(currentObjectiveIndex, deathPosition);
            keyWaitingToBeCollected = true;
            Debug.Log("Key " + currentObjectiveIndex + " spawned. Kill counting paused until chest " + currentObjectiveIndex + " is looted.");
        }
    }

    // Instantiate the key prefab at the enemy death position
    private void SpawnKey(int keyIndex, Vector3 spawnPosition)
    {
        if (keyPrefab == null)
        {
            Debug.LogWarning("TreasureGameManager: keyPrefab is not assigned.");
            return;
        }

        KeyPickup key = Instantiate(keyPrefab, spawnPosition, Quaternion.identity);
        key.Initialize(keyIndex);
        Debug.Log("Key " + keyIndex + " spawned at " + spawnPosition);
    }

    // Called by KeyPickup when the player touches a key
    public void CollectKey(int keyIndex)
    {
        // Only accept the key that matches the current objective
        if (keyIndex != currentObjectiveIndex) return;
        if (collectedKeyIndexes.Contains(keyIndex)) return;

        collectedKeyIndexes.Add(keyIndex);
        keyWaitingToBeCollected = false;
        UpdateKeyUI();

        Debug.Log("Key " + keyIndex + " collected. Chest " + keyIndex + " opened.");

        // Tell the matching chest to switch to its Open sprite
        foreach (TreasureChest chest in registeredChests)
        {
            if (chest != null && chest.RequiredKeyIndex == keyIndex)
                chest.RefreshState();
        }

        // Every key triggers sudden death until the matching chest is looted
        Debug.Log("Key " + keyIndex + " collected. Sudden death started.");
        StartSuddenDeath();
    }

    // Returns true if the player has already collected the key with this index
    public bool HasKey(int keyIndex)
    {
        return collectedKeyIndexes.Contains(keyIndex);
    }

    // Called by TreasureChest on Initialize so this manager knows about every chest in the scene
    public void RegisterChest(TreasureChest chest)
    {
        if (!registeredChests.Contains(chest))
            registeredChests.Add(chest);
    }

    // Called by TreasureChest when the player opens it
    public void RegisterChestLooted(int chestIndex)
    {
        // Only accept the chest that matches the current objective
        if (chestIndex != currentObjectiveIndex) return;
        if (lootedChestIndexes.Contains(chestIndex)) return;

        lootedChestIndexes.Add(chestIndex);

        // Reset kill counter and advance to the next key/chest pair
        killsTowardCurrentKey = 0;
        currentObjectiveIndex++;

        if (lootedChestIndexes.Count >= requiredCount)
        {
            Debug.Log("Chest " + chestIndex + " looted. All objectives complete.");
            WinGame();
        }
        else
        {
            Debug.Log("Chest " + chestIndex + " looted. Starting next key cycle.");
            StopSuddenDeath();
        }
    }

    // Tell the enemy spawner to ramp up and stop counting kills toward the next key
    private void StartSuddenDeath()
    {
        suddenDeathActive = true;
        Enemy_Spawner spawner = FindFirstObjectByType<Enemy_Spawner>();
        if (spawner != null)
            spawner.StartSuddenDeath();
        else
            Debug.LogWarning("TreasureGameManager: could not find Enemy_Spawner for sudden death.");
    }

    // Return spawning to normal and allow kill counting to resume for the next key
    private void StopSuddenDeath()
    {
        suddenDeathActive = false;
        Enemy_Spawner spawner = FindFirstObjectByType<Enemy_Spawner>();
        if (spawner != null)
            spawner.StopSuddenDeath();
        else
            Debug.LogWarning("TreasureGameManager: could not find Enemy_Spawner to stop sudden death.");
    }

    // Called when all required chests have been looted
    private void WinGame()
    {
        Debug.Log("Player wins!");
        // Uncomment the line below to freeze the game on win
        // Time.timeScale = 0f;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.GameOver(true);
        }
    }

    private void UpdateKeyUI()
    {
        if (HudManager.Instance != null)
            HudManager.Instance.UpdateKeysCollected(collectedKeyIndexes.Count, requiredCount);
    }
}
