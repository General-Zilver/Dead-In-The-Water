using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Stores the selected difficulty across scenes
    // 0 = Easy, 1 = Medium, 2 = Hard
    public static int selectedDifficulty = 0;

    public void StartGame()
    {
        // Load the game scene when the player clicks "Start"
        SceneManager.LoadScene("GameScene");
    }

    // Called from the difficulty buttons
    public void SetEasy()
    {
        selectedDifficulty = 0;
        HighlightDifficulty(0);
    }

    public void SetMedium()
    {
        selectedDifficulty = 1;
        HighlightDifficulty(1);
    }

    public void SetHard()
    {
        selectedDifficulty = 2;
        HighlightDifficulty(2);
    }

    // Optional: quit the game
    public void QuitGame()
    {
        Application.Quit();
    }

    void HighlightDifficulty(int level)
    {
        
        Debug.Log("Difficulty set to: " + level);
    }
}