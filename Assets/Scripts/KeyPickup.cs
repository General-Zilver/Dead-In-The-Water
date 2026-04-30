using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    // Set this in the Inspector for manually placed keys, or call Initialize at runtime
    public int keyIndex;

    private bool collected = false;

    // Assign the key index when spawning this key from code
    public void Initialize(int index)
    {
        keyIndex = index;
    }

    // Collect the key when the player touches it and notify the game manager
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;
        if (TreasureGameManager.Instance == null)
        {
            Debug.LogWarning("KeyPickup: TreasureGameManager not found.");
            return;
        }

        collected = true;
        TreasureGameManager.Instance.CollectKey(keyIndex);
        Destroy(gameObject);
    }
}
