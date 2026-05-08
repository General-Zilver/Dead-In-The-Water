using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [SerializeField] 
    private int damageAmount = 1;
    [SerializeField]
    private float damageCooldown = 1f;

    private float nextAllowedDamageTime;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    private void TryDamagePlayer(GameObject other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerGrappleState grappleState = other.GetComponent<PlayerGrappleState>();
        if (grappleState != null && grappleState.IsCollisionImmune)
            return;

        if (Time.time < nextAllowedDamageTime)
            return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health == null)
            return;

        health.TakeDamage(damageAmount);
        nextAllowedDamageTime = Time.time + damageCooldown;
    }
}
