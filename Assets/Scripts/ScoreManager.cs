using UnityEngine;

public class ScoreManager : MonoBehaviour
{

    public static ScoreManager Instance;
    private int currentScore = 0;

    void Awake()
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

    void Start()
    {
        currentScore = 0;
        UpdateScoreUI();
    }

    public void AddScore(int points)
    {
        currentScore += points;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (HudManager.Instance != null)
        {
            HudManager.Instance.UpdateScore(currentScore);
        }
    }




}
