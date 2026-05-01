using UnityEngine;

public class AirBubblePickup : MonoBehaviour
{
    public float airPerSecond = 20f;
    public float totalAirAvailable = 100f;
    public float minAnimationSpeed = 0.9f;
    public float maxAnimationSpeed = 1.1f;

    private float remainingAir;
    private AirBubbleSpawner spawner;
    private int spawnIndex = -1;
    private bool depleted;
    private Animator animator;

    void Awake()
    {
        remainingAir = totalAirAvailable;
        animator = GetComponent<Animator>();
        RandomizeAnimation();
    }

    void RandomizeAnimation()
    {
        if (animator == null)
            return;

        animator.speed = Random.Range(minAnimationSpeed, maxAnimationSpeed);
        animator.Play(0, 0, Random.Range(0f, 1f));
    }

    public void Initialize(AirBubbleSpawner bubbleSpawner, int bubbleSpawnIndex)
    {
        spawner = bubbleSpawner;
        spawnIndex = bubbleSpawnIndex;
        remainingAir = totalAirAvailable;
        depleted = false;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (depleted || remainingAir <= 0f)
            return;

        PlayerAirController playerAir = other.GetComponentInParent<PlayerAirController>();
        if (playerAir == null)
            return;

        float amountToGive = Mathf.Min(airPerSecond * Time.deltaTime, remainingAir);
        playerAir.AddAir(amountToGive);
        remainingAir -= amountToGive;

        if (remainingAir <= 0f)
        {
            depleted = true;
            if (spawner != null)
                spawner.BubbleDepleted(spawnIndex);

            Destroy(gameObject);
        }
    }
}
