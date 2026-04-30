using UnityEngine;
using System.Collections;

public class JellyfishHarpoonTarget : MonoBehaviour
{
    private bool isDying = false;

    // Stop this jellyfish moving, then destroy it after a short delay
    public void OnHarpooned()
    {
        if (isDying) return;
        isDying = true;

        JellyfishMover mover = GetComponent<JellyfishMover>();
        if (mover != null)
            mover.StopMoving();

        StartCoroutine(DieAfterDelay());
    }

    // Wait one second then destroy the jellyfish
    private IEnumerator DieAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
