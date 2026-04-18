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
        {
            spawner.EnemyDied();
        }




        Destroy(gameObject);

    }

}
