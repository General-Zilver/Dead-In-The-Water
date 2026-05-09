using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    private int maxHealth;
    private int currentHealth;
    private bool isDead = false;

    [Header("Damage Feedback")]
    [SerializeField] private Animator bodyAnimator;
    [SerializeField] private string damageAnimationStateName = "PlayerTakesDamage";
    [SerializeField] private string aimForwardStateName = "PlayerAimForward";
    [SerializeField] private string aimUpStateName = "PlayerAimUp";
    [SerializeField] private string aimDownStateName = "PlayerAimDown";
    [SerializeField] private float damageFeedbackDuration = 2.1f;
    [SerializeField] private float blinkInterval = 0.08f;
    [SerializeField] private float blinkAlpha = 0.35f;

    private SpriteRenderer[] blinkRenderers;
    private Color[] originalRendererColors;
    private Coroutine damageFeedbackCoroutine;
    private bool damageInvulnerable;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (bodyAnimator == null)
            bodyAnimator = GetComponentInChildren<Animator>();

        blinkRenderers = GetComponentsInChildren<SpriteRenderer>();
        originalRendererColors = new Color[blinkRenderers.Length];
        for (int i = 0; i < blinkRenderers.Length; i++)
            originalRendererColors[i] = blinkRenderers[i].color;
    }

    void Start()
    {
        if (DifficultyManager.Instance != null)
        {
            maxHealth = DifficultyManager.Instance.PlayerHealth;
        }
        else
        {
            maxHealth = 5;
            Debug.LogWarning("PlayerHealth: DifficultyManager not found! Default to 5 HP.");
        }

        currentHealth = maxHealth;
        
        UpdateHealthUI();
    }


    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        if (damageInvulnerable) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"Player took {damageAmount} damage. HP remaining: {currentHealth}");

        PlayDamageFeedback();
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }

    }

    private void PlayDamageFeedback()
    {
        if (damageFeedbackCoroutine != null)
            StopCoroutine(damageFeedbackCoroutine);

        damageFeedbackCoroutine = StartCoroutine(DamageFeedbackRoutine());
    }

    private IEnumerator DamageFeedbackRoutine()
    {
        if (bodyAnimator != null)
            bodyAnimator.Play(damageAnimationStateName, 0, 0f);

        damageInvulnerable = true;
        float elapsed = 0f;
        bool faded = false;

        while (elapsed < damageFeedbackDuration)
        {
            SetBlinkAlpha(faded ? 1f : blinkAlpha);
            faded = !faded;

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        RestoreRendererColors();
        RestoreAimAnimation();
        damageInvulnerable = false;
        damageFeedbackCoroutine = null;
    }

    private void SetBlinkAlpha(float alpha)
    {
        for (int i = 0; i < blinkRenderers.Length; i++)
        {
            Color color = originalRendererColors[i];
            color.a *= alpha;
            blinkRenderers[i].color = color;
        }
    }

    private void RestoreRendererColors()
    {
        for (int i = 0; i < blinkRenderers.Length; i++)
            blinkRenderers[i].color = originalRendererColors[i];
    }

    private void RestoreAimAnimation()
    {
        if (bodyAnimator == null)
            return;

        int aimPose = bodyAnimator.GetInteger("PlayerAimPose");
        if (aimPose > 0)
            bodyAnimator.Play(aimUpStateName, 0, 0f);
        else if (aimPose < 0)
            bodyAnimator.Play(aimDownStateName, 0, 0f);
        else
            bodyAnimator.Play(aimForwardStateName, 0, 0f);
    }

    public void Heal(int healAmount)
    {
        if (isDead) return;

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();
    }

    public void GrantBonusHealth(int amount, int maxBonusHealth)
    {
        if (isDead) return;
        if (amount <= 0) return;

        int bonusCap = maxHealth + Mathf.Max(0, maxBonusHealth);
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, bonusCap);

        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (HudManager.Instance != null)
        {
            HudManager.Instance.UpdateHealth(currentHealth, Mathf.Max(maxHealth, currentHealth));
        }



    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        damageInvulnerable = false;

        Debug.Log("Player has drowned!");

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.GameOver(false);
        }
    }

}
