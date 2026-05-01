using UnityEngine;

[RequireComponent(typeof(Camera))]
[DefaultExecutionOrder(10000)]
public class CameraBoundaryClamp : MonoBehaviour
{
    [Header("World Bounds")]
    [SerializeField] private float minX = -80.07f;
    [SerializeField] private float maxX = 112.06f;
    [SerializeField] private float minY = -24.65f;
    [SerializeField] private float maxY = 30.55f;

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    // Runs late so Cinemachine can move the camera first, then this clamps the final camera position.
    void LateUpdate()
    {
        ClampCameraToBounds();
    }

    void OnPreCull()
    {
        ClampCameraToBounds();
    }

    void ClampCameraToBounds()
    {
        if (cam == null || !cam.orthographic)
            return;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float cameraMinX = minX + halfWidth;
        float cameraMaxX = maxX - halfWidth;
        float cameraMinY = minY + halfHeight;
        float cameraMaxY = maxY - halfHeight;

        Vector3 position = transform.position;

        if (cameraMinX > cameraMaxX)
            position.x = (minX + maxX) * 0.5f;
        else
            position.x = Mathf.Clamp(position.x, cameraMinX, cameraMaxX);

        if (cameraMinY > cameraMaxY)
            position.y = (minY + maxY) * 0.5f;
        else
            position.y = Mathf.Clamp(position.y, cameraMinY, cameraMaxY);

        transform.position = position;
    }
}
