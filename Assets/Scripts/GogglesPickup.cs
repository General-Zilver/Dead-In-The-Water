using UnityEngine;

public class GogglesPickup : MonoBehaviour
{
    [SerializeField] private int bonusHealthAmount = 1;
    [SerializeField] private int maxBonusHealth = 1;
    [SerializeField] private float lifetime = 8f;

    private bool collected;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

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
        if (collected)
            return;

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null || !playerHealth.CompareTag("Player"))
            return;

        collected = true;
        playerHealth.GrantBonusHealth(bonusHealthAmount, maxBonusHealth);
        Debug.Log("GogglesPickup: player collected bonus health.");
        Destroy(gameObject);
    }
}
