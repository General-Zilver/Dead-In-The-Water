using UnityEngine;

public class FloatingScorePopup : MonoBehaviour
{
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private float floatDistance = 1f;

    private SpriteRenderer[] spriteRenderers;
    private Color[] startColors;
    private Vector3 startPosition;
    private float timer;

    void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        startColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
            startColors[i] = spriteRenderers[i].color;
    }

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float progress = lifetime > 0f ? Mathf.Clamp01(timer / lifetime) : 1f;

        transform.position = startPosition + Vector3.up * (floatDistance * progress);
        SetAlpha(1f - progress);

        if (progress >= 1f)
            Destroy(gameObject);
    }

    void SetAlpha(float alpha)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            Color color = startColors[i];
            color.a *= alpha;
            spriteRenderers[i].color = color;
        }
    }
}
