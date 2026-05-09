using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject easyButton;
    [SerializeField] private GameObject mediumButton;
    [SerializeField] private GameObject hardButton;
    [SerializeField] private GameObject easyTextButton;
    [SerializeField] private GameObject mediumTextButton;
    [SerializeField] private GameObject hardTextButton;
    [SerializeField] private Color selectedDifficultyTint = new Color(0.7f, 0.82f, 1f, 1f);
    [SerializeField] private Color selectedEasyMediumTextTint = new Color(0.78f, 0.88f, 1f, 1f);
    [SerializeField] private Color selectedHardTextTint = new Color(0.9f, 0.95f, 1f, 1f);
    [SerializeField] private Color normalDifficultyTint = Color.white;

    // Stores the selected difficulty across scenes
    // 0 = Easy, 1 = Medium, 2 = Hard
    public static int selectedDifficulty = 0;

    void Awake()
    {
        ResolveTextButtons();
    }

    void Start()
    {
        HighlightDifficulty(selectedDifficulty);
    }

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
        GameObject selectedButton = GetDifficultyButton(level);
        ApplyDifficultyTint(level);

        if (EventSystem.current != null && selectedButton != null)
            EventSystem.current.SetSelectedGameObject(selectedButton);

        Debug.Log("Difficulty set to: " + level);
    }

    GameObject GetDifficultyButton(int level)
    {
        if (level == 1)
            return mediumButton;

        if (level == 2)
            return hardButton;

        return easyButton;
    }

    void ApplyDifficultyTint(int level)
    {
        SetDifficultyButtonTint(easyButton, level == 0, selectedDifficultyTint);
        SetDifficultyButtonTint(easyTextButton, level == 0, selectedEasyMediumTextTint);
        SetDifficultyButtonTint(mediumButton, level == 1, selectedDifficultyTint);
        SetDifficultyButtonTint(mediumTextButton, level == 1, selectedEasyMediumTextTint);
        SetDifficultyButtonTint(hardButton, level == 2, selectedDifficultyTint);
        SetDifficultyButtonTint(hardTextButton, level == 2, selectedHardTextTint);
    }

    void SetDifficultyButtonTint(GameObject buttonObject, bool selected, Color selectedTint)
    {
        if (buttonObject == null)
            return;

        Button button = buttonObject.GetComponent<Button>();
        if (button == null)
            return;

        Color tint = selected ? selectedTint : normalDifficultyTint;
        ColorBlock colors = button.colors;
        colors.normalColor = tint;
        colors.selectedColor = tint;
        button.colors = colors;

        if (button.targetGraphic != null)
            button.targetGraphic.color = tint;
    }

    void ResolveTextButtons()
    {
        if (easyTextButton == null)
            easyTextButton = GameObject.Find("EasyTextButton") ?? GameObject.Find("EasyTextButtom");

        if (mediumTextButton == null)
            mediumTextButton = GameObject.Find("MediumTextButton");

        if (hardTextButton == null)
            hardTextButton = GameObject.Find("HardTextButton");
    }
}
