using System.Collections;
using UnityEngine;

public class PlayerOxygenBubbles : MonoBehaviour
{
    [SerializeField] private RuntimeAnimatorController bubblesAnimatorController;
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 3.85f, 0f);
    [SerializeField] private Vector3 localScale = new Vector3(0.75f, 0.75f, 1f);
    [SerializeField] private string sortingLayerName = "Player";
    [SerializeField] private int sortingOrder = 100;
    [SerializeField] private float animationSpeed = 1.25f;
    [SerializeField] private float delayBetweenLoops = 4f;

    private Transform bubblesTransform;
    private Animator bubblesAnimator;
    private SpriteRenderer bubblesRenderer;

    private void Awake()
    {
        CreateBubblesVisual();
    }

    private void LateUpdate()
    {
        if (bubblesTransform != null)
            bubblesTransform.localPosition = localOffset;
    }

    private void CreateBubblesVisual()
    {
        if (bubblesAnimatorController == null)
            return;

        GameObject bubblesObject = new GameObject("Oxygen Bubbles");
        bubblesObject.transform.SetParent(transform, false);
        bubblesObject.transform.localPosition = localOffset;
        bubblesObject.transform.localRotation = Quaternion.identity;
        bubblesObject.transform.localScale = localScale;

        SpriteRenderer spriteRenderer = bubblesObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingLayerName = sortingLayerName;
        spriteRenderer.sortingOrder = sortingOrder;
        bubblesRenderer = spriteRenderer;

        Animator animator = bubblesObject.AddComponent<Animator>();
        animator.runtimeAnimatorController = bubblesAnimatorController;
        animator.speed = animationSpeed;

        bubblesAnimator = animator;
        bubblesTransform = bubblesObject.transform;
        StartCoroutine(PlayBubblesWithDelay());
    }

    private IEnumerator PlayBubblesWithDelay()
    {
        if (bubblesAnimator == null)
            yield break;

        float clipLength = GetBubbleClipLength();
        float playbackDuration = animationSpeed > 0f ? clipLength / animationSpeed : clipLength;

        while (true)
        {
            if (bubblesRenderer != null)
                bubblesRenderer.enabled = true;

            bubblesAnimator.enabled = true;
            bubblesAnimator.Play(0, 0, 0f);

            yield return new WaitForSeconds(playbackDuration);

            bubblesAnimator.enabled = false;
            if (bubblesRenderer != null)
                bubblesRenderer.enabled = false;

            yield return new WaitForSeconds(delayBetweenLoops);
        }
    }

    private float GetBubbleClipLength()
    {
        if (bubblesAnimatorController == null || bubblesAnimatorController.animationClips == null || bubblesAnimatorController.animationClips.Length == 0)
            return 1f;

        return Mathf.Max(0.01f, bubblesAnimatorController.animationClips[0].length);
    }
}
