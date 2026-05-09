using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    
    [Header("UI Panels")]
    public GameObject winPanel;
    public GameObject losePanel;
    public TextMeshProUGUI[] finalScoreText;

    private bool isGameActive = true;

    void Awake()
    {
        Instance = this;
        winPanel.SetActive(false);
        losePanel.SetActive(false);

    }

    public void GameOver(bool won)
    {
        if(!isGameActive) return;
        isGameActive = false;

        Time.timeScale = 0f;

        if (won)
        {
            winPanel.SetActive(true);
        
        }
        else
        {
            losePanel.SetActive(true);
        }

        if (finalScoreText != null && ScoreManager.Instance != null)
        {

            string scoreString = "Final Score: " + HudManager.Instance.scoreText.text;

            foreach (TextMeshProUGUI textElement in finalScoreText)
            {
                if (textElement != null)
                {
                    textElement.text = scoreString;
                }
            }
        }

    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }





}
