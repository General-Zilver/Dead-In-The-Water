using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] private Camera aimCamera;
    [SerializeField] private Transform aimPivot;
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private SpriteRenderer gunRenderer;
    [SerializeField] private Animator bodyAnimator;

    [SerializeField] private Vector3 rightForwardOffset = new Vector3(0.83f, 0.8f, 0f);
    [SerializeField] private Vector3 rightUpOffset      = new Vector3(0.83f, 1.1f, 0f);
    [SerializeField] private Vector3 rightDownOffset    = new Vector3(0.8f, 0.5f, 0f);

    [SerializeField] private Vector3 leftForwardOffset  = new Vector3(-0.83f, 0.8f, 0f);
    [SerializeField] private Vector3 leftUpOffset       = new Vector3(-0.83f, 1.1f, 0f);
    [SerializeField] private Vector3 leftDownOffset     = new Vector3(-0.8f, 0.5f, 0f);

    [SerializeField] private float rightUpAngle = 45f;
    [SerializeField] private float rightForwardAngle = 0f;
    [SerializeField] private float rightDownAngle = -45f;

    [SerializeField] private float leftUpAngle = 135f;
    [SerializeField] private float leftForwardAngle = 180f;
    [SerializeField] private float leftDownAngle = 225f;

    [SerializeField] private float aimUpThreshold = 0.5f;
    [SerializeField] private float aimDownThreshold = -0.5f;

    private void Update()
    {
        if (aimCamera == null || aimPivot == null)
            return;

        Vector3 mouseWorld = aimCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        bool facingLeft = mouseWorld.x < transform.position.x;

        if (bodyRenderer != null)
            bodyRenderer.flipX = facingLeft;

        float yDelta = mouseWorld.y - transform.position.y;

        int pose = 0;
        if (yDelta > aimUpThreshold)
            pose = 1;
        else if (yDelta < aimDownThreshold)
            pose = -1;

        if (bodyAnimator != null)
            bodyAnimator.SetInteger("PlayerAimPose", pose);

        Vector3 targetOffset;
        float targetAngle;

        if (!facingLeft)
        {
            if (pose == 1)
            {
                targetOffset = rightUpOffset;
                targetAngle = rightUpAngle;
            }
            else if (pose == -1)
            {
                targetOffset = rightDownOffset;
                targetAngle = rightDownAngle;
            }
            else
            {
                targetOffset = rightForwardOffset;
                targetAngle = rightForwardAngle;
            }

            if (gunRenderer != null)
            {
                gunRenderer.flipX = false;
                gunRenderer.flipY = false;
            }
        }
        else
        {
            if (pose == 1)
            {
                targetOffset = leftUpOffset;
                targetAngle = leftUpAngle;
            }
            else if (pose == -1)
            {
                targetOffset = leftDownOffset;
                targetAngle = leftDownAngle;
            }
            else
            {
                targetOffset = leftForwardOffset;
                targetAngle = leftForwardAngle;
            }

            if (gunRenderer != null)
            {
                gunRenderer.flipX = false;
                gunRenderer.flipY = true;
            }
        }

        aimPivot.localPosition = targetOffset;
        aimPivot.localRotation = Quaternion.Euler(0f, 0f, targetAngle);
    }
}