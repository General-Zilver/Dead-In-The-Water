using UnityEngine;
using System.Collections;

public class JellyfishHarpoonTarget : MonoBehaviour
{
    [Header("Death Animation")]
    [SerializeField] private RuntimeAnimatorController deathAnimationController;
    [SerializeField] private float deathAnimationDuration = 2f;

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

        SpawnDeathAnimation();
        SetOriginalRenderersVisible(false);
        SetCollidersEnabled(false);
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

    private void SpawnDeathAnimation()
    {
        if (deathAnimationController == null)
            return;

        SpriteRenderer sourceRenderer = GetComponentInChildren<SpriteRenderer>();
        if (sourceRenderer == null)
            return;

        GameObject deathObject = new GameObject(gameObject.name + " Death Animation");
        deathObject.transform.position = sourceRenderer.transform.position;
        deathObject.transform.rotation = sourceRenderer.transform.rotation;
        deathObject.transform.localScale = sourceRenderer.transform.lossyScale;

        SpriteRenderer deathRenderer = deathObject.AddComponent<SpriteRenderer>();
        deathRenderer.sprite = sourceRenderer.sprite;
        deathRenderer.color = sourceRenderer.color;
        deathRenderer.flipX = sourceRenderer.flipX;
        deathRenderer.flipY = sourceRenderer.flipY;
        deathRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        deathRenderer.sortingOrder = sourceRenderer.sortingOrder;
        deathRenderer.sharedMaterial = sourceRenderer.sharedMaterial;

        Animator animator = deathObject.AddComponent<Animator>();
        animator.runtimeAnimatorController = deathAnimationController;

        DeathAnimationFade fade = deathObject.AddComponent<DeathAnimationFade>();
        fade.Initialize(deathAnimationDuration, sourceRenderer.bounds.size);
    }

    private void SetOriginalRenderersVisible(bool visible)
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = visible;
    }

    private void SetCollidersEnabled(bool enabled)
    {
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = enabled;
    }
}
