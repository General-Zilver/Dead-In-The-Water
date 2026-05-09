using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    // Set this in the Inspector for manually placed keys, or call Initialize at runtime
    public int keyIndex;
    [SerializeField] private float collectDelay = 2f;
    [SerializeField] private float lockedAlpha = 0.45f;

    private bool collected = false;
    private float collectibleAtTime;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;

    private void Awake()
    {
        collectibleAtTime = Time.time + collectDelay;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
            originalColors[i] = spriteRenderers[i].color;

        SetAlpha(lockedAlpha);
    }

    private void Update()
    {
        if (Time.time >= collectibleAtTime)
            RestoreColors();
    }

    // Assign the key index when spawning this key from code
    public void Initialize(int index)
    {
        keyIndex = index;
    }

    public void Initialize(int index, Sprite keySprite)
    {
        Initialize(index);

        if (spriteRenderer != null && keySprite != null)
            spriteRenderer.sprite = keySprite;
    }

    // Collect the key when the player touches it and notify the game manager
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollect(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryCollect(other);
    }

    private void TryCollect(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;
        if (Time.time < collectibleAtTime) return;
        if (TreasureGameManager.Instance == null)
        {
            Debug.LogWarning("KeyPickup: TreasureGameManager not found.");
            return;
        }

        collected = true;
        TreasureGameManager.Instance.CollectKey(keyIndex);
        Destroy(gameObject);
    }

    private void SetAlpha(float alpha)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            Color color = originalColors[i];
            color.a *= alpha;
            spriteRenderers[i].color = color;
        }
    }

    private void RestoreColors()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
            spriteRenderers[i].color = originalColors[i];
    }
}
