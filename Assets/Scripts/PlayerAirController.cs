using UnityEngine;

public class PlayerAirController : MonoBehaviour
{
    private float maxAir;
    private float currentAir;
    private bool outOfAirLogged;

    void Start()
    {
        if (DifficultyManager.Instance != null)
        {
            maxAir = DifficultyManager.Instance.AirDuration;
        }
        else
        {
            maxAir = 60f;
            Debug.LogWarning("PlayerAirController: DifficultyManager not found. Using fallback air duration.");
        }

        currentAir = maxAir;

        if(HudManager.Instance != null)
        {
            HudManager.Instance.UpdateAir(1f);
        }

    }

    void Update()
    {
        if (currentAir <= 0f)
            return;

        currentAir = Mathf.Max(0f, currentAir - Time.deltaTime);

        if(HudManager.Instance != null)
        {
            HudManager.Instance.UpdateAir(GetCurrentAirPercent());
        }

        if (currentAir <= 0f && !outOfAirLogged)
        {
            outOfAirLogged = true;
            Debug.Log("PlayerAirController: player is out of air.");

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.GameOver(false);
            }
        }
    }

    public void AddAir(float amount)
    {
        if (amount <= 0f)
            return;

        currentAir = Mathf.Clamp(currentAir + amount, 0f, maxAir);

        if (currentAir > 0f)
            outOfAirLogged = false;
    }

    public float GetCurrentAirPercent()
    {
        if (maxAir <= 0f)
            return 0f;

        return currentAir / maxAir;
    }
}
