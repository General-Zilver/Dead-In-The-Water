using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HudManager : MonoBehaviour
{
    public static HudManager Instance;

    [Header("Air Bar")]
    public Slider airSlider;
    public Image airFill; // The image that represents the filled portion of the air bar
    public Color airFullColor = new Color(0f, 1f, 0f, 1f); // Green color for full air
    public Color airLowColor = new Color(1f, 0f, 0f, 1f); // Red color for low air
    public float lowAirThreshold = 0.33f; // The percentage of air at which the color changes to red, .33 cause scoba diver rule

    [Header("Health")]
    public TextMeshProUGUI healthText;
    public Image[] hearticons; // Image elements for the health hearts

    [Header ("Score")]
    public TextMeshProUGUI scoreText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    // Air

    public void UpdateAir(float normalizedValue)
    {
        if (airSlider != null)
        {
            airSlider.value = normalizedValue;
        }

        if (airFill != null)
        {
            airFill.color = Color.Lerp(airLowColor, airFullColor, normalizedValue);
        }
    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = "HP: " + currentHealth + " / " + maxHealth;
        }

        if (hearticons != null &&  hearticons.Length > 0)
        {
            for (int i = 0; i < hearticons.Length; i++)
            {
                if (hearticons[i] != null)
                {
                    hearticons[i].enabled = i < currentHealth;
                }
            }

        }


    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString("N0");
        }


    }





}
