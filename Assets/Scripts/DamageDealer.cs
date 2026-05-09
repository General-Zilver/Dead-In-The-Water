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
        TryDamagePlayer(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayer(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    private void TryDamagePlayer(Collider2D other)
    {
        if (other == null)
            return;

        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health == null || !health.CompareTag("Player"))
            return;

        PlayerGrappleState grappleState = health.GetComponent<PlayerGrappleState>();
        if (grappleState != null && grappleState.IsCollisionImmune)
            return;

        if (Time.time < nextAllowedDamageTime)
            return;

        health.TakeDamage(damageAmount);
        nextAllowedDamageTime = Time.time + damageCooldown;
    }
}
