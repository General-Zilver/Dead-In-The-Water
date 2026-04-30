using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private Sprite lootedSprite;

    private SpriteRenderer spriteRenderer;
    private bool isLooted = false;

    // Read-only access to the chest index and its required key index
    public int ChestIndex { get; private set; }
    public int RequiredKeyIndex { get; private set; }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Assign the chest its index, link it to its matching key, and register with the game manager
    public void Initialize(int index)
    {
        ChestIndex = index;
        RequiredKeyIndex = index;

        if (spriteRenderer != null && closedSprite != null)
            spriteRenderer.sprite = closedSprite;

        if (TreasureGameManager.Instance != null)
            TreasureGameManager.Instance.RegisterChest(this);
        else
            Debug.LogWarning("TreasureChest: TreasureGameManager not found during Initialize.");
    }

    // Update the chest sprite based on whether the matching key has been collected
    public void RefreshState()
    {
        if (isLooted) return;

        if (TreasureGameManager.Instance != null && TreasureGameManager.Instance.HasKey(RequiredKeyIndex))
        {
            if (spriteRenderer != null && openSprite != null)
                spriteRenderer.sprite = openSprite;
        }
        else
        {
            if (spriteRenderer != null && closedSprite != null)
                spriteRenderer.sprite = closedSprite;
        }
    }

    // Open the chest when the player touches it, but only if they have the right key
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLooted) return;
        if (!other.CompareTag("Player")) return;
        if (TreasureGameManager.Instance == null) return;
        if (!TreasureGameManager.Instance.HasKey(RequiredKeyIndex)) return;

        isLooted = true;

        if (spriteRenderer != null && lootedSprite != null)
            spriteRenderer.sprite = lootedSprite;

        TreasureGameManager.Instance.RegisterChestLooted(ChestIndex);
    }
}
