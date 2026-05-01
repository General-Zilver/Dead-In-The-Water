using UnityEngine;
using System.Collections;

public class JellyfishHarpoonTarget : MonoBehaviour
{
    private bool isDying = false;
    private Coroutine delayedDeathCoroutine;

    // Stop this jellyfish moving, then destroy it after a short delay
    public void OnHarpooned()
    {
        if (isDying) return;
        isDying = true;

        JellyfishMover mover = GetComponent<JellyfishMover>();
        if (mover != null)
            mover.StopMoving();

        delayedDeathCoroutine = StartCoroutine(DieAfterDelay());
    }

    // Called when the player finishes the grapple pull and the speed boost begins.
    public void DespawnNow()
    {
        if (delayedDeathCoroutine != null)
            StopCoroutine(delayedDeathCoroutine);

        Destroy(gameObject);
    }

    // Wait one second then destroy the jellyfish
    private IEnumerator DieAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
