using UnityEngine;

public class DeathAnimationFade : MonoBehaviour
{
    private SpriteRenderer[] spriteRenderers;
    private Color[] startColors;
    private Animator animator;
    private float duration = 2f;
    private float timer;
    private bool animationFrozen;
    private bool matchTargetSize;
    private Vector2 targetSize;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        startColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
            startColors[i] = spriteRenderers[i].color;
    }

    public void Initialize(float fadeDuration)
    {
        duration = Mathf.Max(0.01f, fadeDuration);
    }

    public void Initialize(float fadeDuration, Vector2 sourceVisualSize)
    {
        duration = Mathf.Max(0.01f, fadeDuration);
        targetSize = sourceVisualSize;
        matchTargetSize = sourceVisualSize.x > 0f && sourceVisualSize.y > 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        float progress = Mathf.Clamp01(timer / duration);
        SetAlpha(1f - progress);

        if (progress >= 1f)
            Destroy(gameObject);
    }

    private void LateUpdate()
    {
        MatchSourceVisualSize();
        FreezeAnimatorOnLastFrame();
    }

    private void FreezeAnimatorOnLastFrame()
    {
        if (animationFrozen || animator == null)
            return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.normalizedTime < 1f || animator.IsInTransition(0))
            return;

        animator.Play(stateInfo.fullPathHash, 0, 1f);
        animator.Update(0f);
        animator.speed = 0f;
        animationFrozen = true;
    }

    private void MatchSourceVisualSize()
    {
        if (!matchTargetSize || spriteRenderers == null || spriteRenderers.Length == 0)
            return;

        Bounds currentBounds = spriteRenderers[0].bounds;
        for (int i = 1; i < spriteRenderers.Length; i++)
            currentBounds.Encapsulate(spriteRenderers[i].bounds);

        if (currentBounds.size.x <= 0f || currentBounds.size.y <= 0f)
            return;

        float scaleFactor = Mathf.Min(targetSize.x / currentBounds.size.x, targetSize.y / currentBounds.size.y);
        if (scaleFactor > 0f && !float.IsInfinity(scaleFactor) && !float.IsNaN(scaleFactor))
            transform.localScale *= scaleFactor;
    }

    private void SetAlpha(float alpha)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            Color color = startColors[i];
            color.a *= alpha;
            spriteRenderers[i].color = color;
        }
    }
}
