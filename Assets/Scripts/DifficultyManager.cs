using UnityEngine;

[System.Serializable]
public class EnemySpawnWeights
{
    public float sharkWeight;
    public float sawSharkWeight;
    public float seaAnglerWeight;
    public float swordFishWeight;
}

// This class manages the difficulty settings for the game. 
// It uses arrays to store different values for each difficulty level (Easy, Medium, Hard).
public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance;

    [Header("Air Duration (seconds)")]
    public float[] airDuration = { 120f, 90f, 60f }; // Easy, Medium, Hard

    [Header("Player Health (hits to die)")]
    public int[] playerHealth = { 10, 5, 3 };

    [Header("Keys Required to Win")]
    public int[] keysRequired = { 1, 2, 3 };

    [Header("Enemy Settings")]
    public float[] spawnRate  = { 6f, 4f, 2f };
    public float[] keyHeldSpawnRate = { 2f, 1f, 0.5f };
    public int[]   maxEnemies = { 6, 8, 10 };

    [Header("Enemy Spawn Weights")]
    public EnemySpawnWeights easyEnemyWeights = new EnemySpawnWeights
    {
        sharkWeight = 65f,
        sawSharkWeight = 11f,
        seaAnglerWeight = 12f,
        swordFishWeight = 12f
    };

    public EnemySpawnWeights mediumEnemyWeights = new EnemySpawnWeights
    {
        sharkWeight = 55f,
        sawSharkWeight = 11f,
        seaAnglerWeight = 17f,
        swordFishWeight = 17f
    };

    public EnemySpawnWeights hardEnemyWeights = new EnemySpawnWeights
    {
        sharkWeight = 30f,
        sawSharkWeight = 11f,
        seaAnglerWeight = 29f,
        swordFishWeight = 30f
    };

    [Header("Kills Required Per Key")]
    public int[] killsRequiredPerKey = { 12, 12, 12 };

    [Header("Whirlpools")]
    public bool[] whirlpoolsEnabled = { false, true, true };
    public bool[] whirlpoolsMoving = { false, false, true };

    [HideInInspector] public int difficulty;

    public float AirDuration => airDuration[difficulty];
    public int PlayerHealth => playerHealth[difficulty];
    public int KeysRequired => keysRequired[difficulty];
    public float SpawnRate          => spawnRate[difficulty];
    public float KeyHeldSpawnRate   => keyHeldSpawnRate[difficulty];
    public int   MaxEnemies         => maxEnemies[difficulty];
    public int   KillsRequiredForNextKey => killsRequiredPerKey[difficulty];
    public bool WhirlpoolsEnabled => whirlpoolsEnabled[difficulty];
    public bool WhirlpoolsMoving => whirlpoolsMoving[difficulty];
    public EnemySpawnWeights CurrentEnemyWeights
    {
        get
        {
            if (difficulty == 1)
                return mediumEnemyWeights;

            if (difficulty == 2)
                return hardEnemyWeights;

            return easyEnemyWeights;
        }
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        difficulty = MainMenu.selectedDifficulty;
        Debug.Log("Difficulty loaded: " + difficulty);
    }
}
