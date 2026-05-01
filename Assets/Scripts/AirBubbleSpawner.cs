using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirBubbleSpawner : MonoBehaviour
{
    private enum BubbleSpotState
    {
        Available,
        Active,
        Cooldown,
        Exhausted
    }

    public GameObject airBubblePrefab;
    public int totalGeneratedSpots = 16;
    public float minSpacingBetweenBubbles = 10f;
    public float borderPadding = 10f;
    public float minX = -107f;
    public float maxX = 138f;
    public float minY = -18f;
    public float maxY = 28f;
    public float respawnDelay = 20f;

    [SerializeField] private int maxGenerationAttempts = 1000;

    private readonly List<Vector2> generatedPositions = new List<Vector2>();
    private readonly List<BubbleSpotState> spotStates = new List<BubbleSpotState>();
    private readonly Dictionary<int, GameObject> activeBubbleInstances = new Dictionary<int, GameObject>();
    private bool loggedNoSpotsRemaining;

    void Start()
    {
        GenerateBubblePositions();
        SpawnInitialBubbles();
    }

    void GenerateBubblePositions()
    {
        generatedPositions.Clear();
        spotStates.Clear();

        float paddedMinX = minX + borderPadding;
        float paddedMaxX = maxX - borderPadding;
        float paddedMinY = minY + borderPadding;
        float paddedMaxY = maxY - borderPadding;

        if (paddedMinX > paddedMaxX || paddedMinY > paddedMaxY)
        {
            Debug.LogWarning("AirBubbleSpawner: border padding leaves no valid bubble spawn area.");
            return;
        }

        int attempts = 0;
        while (generatedPositions.Count < totalGeneratedSpots && attempts < maxGenerationAttempts)
        {
            attempts++;

            Vector2 candidate = new Vector2(
                Random.Range(paddedMinX, paddedMaxX),
                Random.Range(paddedMinY, paddedMaxY)
            );

            if (!IsFarEnoughFromOtherBubbles(candidate))
                continue;

            generatedPositions.Add(candidate);
            spotStates.Add(BubbleSpotState.Available);
        }

        Debug.Log("AirBubbleSpawner: generated bubble position count: " + generatedPositions.Count);

        if (generatedPositions.Count < totalGeneratedSpots)
        {
            Debug.LogWarning("AirBubbleSpawner: only generated " + generatedPositions.Count + " valid bubble positions out of " + totalGeneratedSpots + ".");
        }
    }

    bool IsFarEnoughFromOtherBubbles(Vector2 candidate)
    {
        for (int i = 0; i < generatedPositions.Count; i++)
        {
            if (Vector2.Distance(candidate, generatedPositions[i]) < minSpacingBetweenBubbles)
                return false;
        }

        return true;
    }

    void SpawnInitialBubbles()
    {
        TryFillActiveBubbles();
    }

    int GetDifficultyIndex()
    {
        if (DifficultyManager.Instance == null)
            return 0;

        return Mathf.Clamp(DifficultyManager.Instance.difficulty, 0, 2);
    }

    int GetMaxActiveBubbles()
    {
        int difficulty = GetDifficultyIndex();

        if (difficulty == 0)
            return 12;

        if (difficulty == 1)
            return 8;

        if (difficulty == 2)
            return 4;

        return 12;
    }

    public void BubbleDepleted(int spawnIndex)
    {
        if (spawnIndex < 0 || spawnIndex >= spotStates.Count)
            return;

        Debug.Log("AirBubbleSpawner: bubble depleted at index " + spawnIndex + ".");

        activeBubbleInstances.Remove(spawnIndex);

        int difficulty = GetDifficultyIndex();
        if (difficulty == 2)
        {
            spotStates[spawnIndex] = BubbleSpotState.Exhausted;
            Debug.Log("AirBubbleSpawner: hard bubble spot exhausted at index " + spawnIndex + ".");
        }
        else
        {
            spotStates[spawnIndex] = BubbleSpotState.Cooldown;
        }

        StartCoroutine(HandleBubbleCooldown(spawnIndex));
    }

    IEnumerator HandleBubbleCooldown(int spawnIndex)
    {
        yield return new WaitForSeconds(respawnDelay);

        Debug.Log("AirBubbleSpawner: bubble cooldown finished at index " + spawnIndex + ".");

        int difficulty = GetDifficultyIndex();
        if (difficulty == 0)
        {
            if (spotStates[spawnIndex] == BubbleSpotState.Cooldown)
                spotStates[spawnIndex] = BubbleSpotState.Available;

            TryFillActiveBubbles();
        }
        else if (difficulty == 1)
        {
            if (spotStates[spawnIndex] == BubbleSpotState.Cooldown)
                spotStates[spawnIndex] = BubbleSpotState.Available;

            TryFillActiveBubbles();
        }
        else
        {
            TryFillActiveBubbles();
        }
    }

    void TryFillActiveBubbles()
    {
        int maxActiveBubbles = GetMaxActiveBubbles();

        while (activeBubbleInstances.Count < maxActiveBubbles)
        {
            List<int> availableIndices = GetAvailableIndices();
            if (availableIndices.Count == 0)
            {
                if (!loggedNoSpotsRemaining)
                {
                    loggedNoSpotsRemaining = true;
                    Debug.Log("AirBubbleSpawner: no bubble spots remaining.");
                }

                return;
            }

            int randomAvailableIndex = availableIndices[Random.Range(0, availableIndices.Count)];
            SpawnBubbleAtIndex(randomAvailableIndex);
        }
    }

    List<int> GetAvailableIndices()
    {
        List<int> availableIndices = new List<int>();

        for (int i = 0; i < spotStates.Count; i++)
        {
            if (spotStates[i] == BubbleSpotState.Available)
                availableIndices.Add(i);
        }

        return availableIndices;
    }

    void SpawnBubbleAtIndex(int spawnIndex)
    {
        if (airBubblePrefab == null)
        {
            Debug.LogWarning("AirBubbleSpawner: airBubblePrefab is not assigned.");
            return;
        }

        if (spawnIndex < 0 || spawnIndex >= generatedPositions.Count)
            return;

        if (activeBubbleInstances.ContainsKey(spawnIndex))
            return;

        if (spotStates[spawnIndex] == BubbleSpotState.Exhausted)
            return;

        GameObject bubble = Instantiate(airBubblePrefab, generatedPositions[spawnIndex], Quaternion.identity);
        AirBubblePickup pickup = bubble.GetComponent<AirBubblePickup>();
        if (pickup != null)
            pickup.Initialize(this, spawnIndex);
        else
            Debug.LogWarning("AirBubbleSpawner: spawned bubble is missing AirBubblePickup.");

        activeBubbleInstances[spawnIndex] = bubble;
        spotStates[spawnIndex] = BubbleSpotState.Active;
        loggedNoSpotsRemaining = false;

        Debug.Log("AirBubbleSpawner: bubble spawned at index " + spawnIndex + ".");
    }
}
