using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    private int maxHealth;
    private int currentHealth;
    private bool isDead = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
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

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"Player took {damageAmount} damage. HP remaining: {currentHealth}");

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }

    }

    public void Heal(int healAmount)
    {
        if (isDead) return;

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (HudManager.Instance != null)
        {
            HudManager.Instance.UpdateHealth(currentHealth, maxHealth);
        }



    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Player has drowned!");

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.GameOver(false);
        }
    }

}
