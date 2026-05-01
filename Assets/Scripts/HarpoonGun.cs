using UnityEngine;

public class HarpoonGun : MonoBehaviour
{
    [SerializeField] private GameObject harpoonPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform aimPivot;
    [SerializeField] private float cooldown = 1.75f;
    [SerializeField] private GrappleRopeVisual grappleRopeVisual;

    private float lastFireTime = -999f;
    private PlayerController playerController;

    // Grab the PlayerController from a parent object
    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();

        // A duplicate HarpoonGun on the same object would spawn two harpoons from one click.
        HarpoonGun[] gunsOnThisObject = GetComponents<HarpoonGun>();
        if (gunsOnThisObject.Length > 1 && gunsOnThisObject[0] != this)
        {
            Debug.LogWarning("Duplicate HarpoonGun found on " + gameObject.name + ". Disabling the extra one.");
            enabled = false;
        }
    }

    // Check for left click each frame and fire if cooldown has passed
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= lastFireTime + cooldown)
            Fire();
    }

    // Spawn the harpoon at the fire point aimed in the direction of the pivot
    private void Fire()
    {
        lastFireTime = Time.time;

        Vector2 fireDir = aimPivot != null ? (Vector2)aimPivot.right : Vector2.right;
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        GameObject harpoon = Instantiate(harpoonPrefab, spawnPos, Quaternion.identity);
        HarpoonProjectile proj = harpoon.GetComponent<HarpoonProjectile>();
        if (proj != null)
            proj.Initialize(fireDir, playerController, grappleRopeVisual);
    }
}
