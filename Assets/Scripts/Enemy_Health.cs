using UnityEngine;

public class Enemy_Health : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;

    [HideInInspector]
    public Enemy_Spawner spawner;

    private int currentHealth;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        currentHealth = maxHealth;
        
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (spawner != null)
            spawner.EnemyDied();

        // Tell the game manager an enemy died so it can check if a key should now spawn
        if (TreasureGameManager.Instance != null)
            TreasureGameManager.Instance.RegisterEnemyKilled(transform.position);

        Destroy(gameObject);
    }

}
