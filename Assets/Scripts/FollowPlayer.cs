using UnityEngine;

public class CameraFollowClamp : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private float smoothSpeed = 8f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        SnapToTarget();
    }

    private void LateUpdate()
    {
        if (target == null || backgroundRenderer == null || cam == null) return;

        Vector3 desiredPosition = target.position;
        desiredPosition.z = transform.position.z;

        Bounds bgBounds = backgroundRenderer.bounds;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float minX = bgBounds.min.x + halfWidth;
        float maxX = bgBounds.max.x - halfWidth;
        float minY = bgBounds.min.y + halfHeight;
        float maxY = bgBounds.max.y - halfHeight;

        if (minX > maxX)
        {
            desiredPosition.x = bgBounds.center.x;
        }
        else
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
        }

        if (minY > maxY)
        {
            desiredPosition.y = bgBounds.center.y;
        }
        else
        {
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );
    }

    private void SnapToTarget()
    {
        if (target == null || backgroundRenderer == null || cam == null) return;

        Vector3 snapPosition = target.position;
        snapPosition.z = transform.position.z;

        Bounds bgBounds = backgroundRenderer.bounds;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float minX = bgBounds.min.x + halfWidth;
        float maxX = bgBounds.max.x - halfWidth;
        float minY = bgBounds.min.y + halfHeight;
        float maxY = bgBounds.max.y - halfHeight;

        if (minX > maxX)
            snapPosition.x = bgBounds.center.x;
        else
            snapPosition.x = Mathf.Clamp(snapPosition.x, minX, maxX);

        if (minY > maxY)
            snapPosition.y = bgBounds.center.y;
        else
            snapPosition.y = Mathf.Clamp(snapPosition.y, minY, maxY);

        transform.position = snapPosition;
    }
}