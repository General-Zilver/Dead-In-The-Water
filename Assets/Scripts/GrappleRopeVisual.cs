using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GrappleRopeVisual : MonoBehaviour
{
    // Assign this to the FirePoint transform so the rope always starts at the gun tip
    [SerializeField] private Transform ropeStartPoint;

    private LineRenderer lineRenderer;
    private Transform endTarget;
    private Vector3 lastEndPos;
    private bool useEndTarget = false;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
    }

    // Show the rope with the end point tracking a moving transform such as the jellyfish
    public void ShowRope(Transform endTransform)
    {
        endTarget = endTransform;
        if (endTarget != null)
            lastEndPos = endTarget.position;
        useEndTarget = true;
        lineRenderer.enabled = true;
    }

    // Show the rope with the end point fixed at a world position
    public void ShowRope(Vector3 endPoint)
    {
        lastEndPos = endPoint;
        useEndTarget = false;
        lineRenderer.enabled = true;
    }

    // Turn off the rope visual
    public void HideRope()
    {
        lineRenderer.enabled = false;
        endTarget = null;
    }

    // Update the rope positions every frame while the line renderer is on
    private void Update()
    {
        if (!lineRenderer.enabled) return;

        Vector3 startPos = ropeStartPoint != null ? ropeStartPoint.position : transform.position;

        // Keep updating the cached end position while the target exists
        // If the target was destroyed, the rope stays at its last known position
        if (useEndTarget && endTarget != null)
            lastEndPos = endTarget.position;

        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, lastEndPos);
    }
}
