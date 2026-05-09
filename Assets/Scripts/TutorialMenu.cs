using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialMenu : MonoBehaviour
{
    private static readonly Color TutorialPanelBlue = new Color(0.02f, 0.34f, 0.55f, 0.92f);
    private static readonly Color ButtonHoverBlue = new Color(0.18039216f, 0.47843137f, 0.70980394f, 1f);
    private static readonly Color ButtonPressedBlue = new Color(0.050980393f, 0.18039216f, 0.27058825f, 1f);
    private const float ThumbnailSize = 240f;
    private const string GridInstructionText = "Click a tutorial to zoom in";
    private const string FullscreenInstructionText = "Any button or click takes you back";

    private enum TutorialState
    {
        MainMenu,
        Grid,
        Fullscreen
    }

    [Header("Main Menu")]
    [SerializeField] private Button tutorialsButton;

    [Header("Grid View")]
    [SerializeField] private GameObject gridPanel;
    [SerializeField] private Image gridDimBackground;
    [SerializeField] private Button backButton;
    [SerializeField] private Transform thumbnailContent;
    [SerializeField] private Button thumbnailButtonPrefab;
    [SerializeField] private TextMeshProUGUI gridInstructionText;

    [Header("Fullscreen View")]
    [SerializeField] private GameObject fullscreenPanel;
    [SerializeField] private Image fullscreenBackground;
    [SerializeField] private Image fullscreenImage;
    [SerializeField] private TextMeshProUGUI fullscreenInstructionText;

    private TutorialState currentState = TutorialState.MainMenu;
    private int fullscreenOpenedFrame = -1;

    private void Awake()
    {
        if (tutorialsButton != null)
            tutorialsButton.onClick.AddListener(ShowGrid);

        if (backButton != null)
            backButton.onClick.AddListener(ShowMainMenu);

        ConfigureLayout();
        BuildThumbnailGrid();
        ShowMainMenu();
    }

    private void Update()
    {
        if (currentState == TutorialState.Fullscreen)
        {
            if (Time.frameCount != fullscreenOpenedFrame && AnyButtonOrClickDown())
                ShowGrid();

            return;
        }

        if (currentState == TutorialState.Grid && Input.GetKeyDown(KeyCode.Escape))
            ShowMainMenu();
    }

    private void BuildThumbnailGrid()
    {
        if (thumbnailContent == null || thumbnailButtonPrefab == null)
            return;

        for (int i = thumbnailContent.childCount - 1; i >= 0; i--)
            Destroy(thumbnailContent.GetChild(i).gameObject);

        Sprite[] tutorialSprites = Resources.LoadAll<Sprite>("Tutorials");
        for (int i = 0; i < tutorialSprites.Length; i++)
            CreateThumbnail(tutorialSprites[i]);
    }

    private void CreateThumbnail(Sprite tutorialSprite)
    {
        Button thumbnailButton = Instantiate(thumbnailButtonPrefab, thumbnailContent);
        thumbnailButton.gameObject.SetActive(true);

        LayoutElement layoutElement = thumbnailButton.GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = thumbnailButton.gameObject.AddComponent<LayoutElement>();

        layoutElement.preferredWidth = ThumbnailSize;
        layoutElement.preferredHeight = ThumbnailSize;
        layoutElement.flexibleWidth = 0f;
        layoutElement.flexibleHeight = 0f;

        Image thumbnailImage = thumbnailButton.GetComponent<Image>();
        if (thumbnailImage == null)
            thumbnailImage = thumbnailButton.GetComponentInChildren<Image>();

        if (thumbnailImage != null)
        {
            thumbnailImage.sprite = tutorialSprite;
            thumbnailImage.preserveAspect = true;
        }

        thumbnailButton.onClick.AddListener(() => ShowFullscreen(tutorialSprite));
    }

    private void ShowMainMenu()
    {
        currentState = TutorialState.MainMenu;

        if (gridPanel != null)
            gridPanel.SetActive(false);

        if (fullscreenPanel != null)
            fullscreenPanel.SetActive(false);

        if (gridInstructionText != null)
            gridInstructionText.text = GridInstructionText;
    }

    private void ShowGrid()
    {
        currentState = TutorialState.Grid;

        if (gridPanel != null)
            gridPanel.SetActive(true);

        if (fullscreenPanel != null)
            fullscreenPanel.SetActive(false);
    }

    private void ShowFullscreen(Sprite tutorialSprite)
    {
        currentState = TutorialState.Fullscreen;
        fullscreenOpenedFrame = Time.frameCount;

        if (gridPanel != null)
            gridPanel.SetActive(false);

        if (fullscreenPanel != null)
            fullscreenPanel.SetActive(true);

        if (fullscreenImage != null)
        {
            fullscreenImage.sprite = tutorialSprite;
            fullscreenImage.preserveAspect = true;
        }

        if (fullscreenInstructionText != null)
            fullscreenInstructionText.text = FullscreenInstructionText;
    }

    private bool AnyButtonOrClickDown()
    {
        return Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2);
    }

    private void ConfigureLayout()
    {
        ConfigureGridPanel();
        ConfigureFullscreenPanel();
    }

    private void ConfigureGridPanel()
    {
        if (gridPanel == null)
            return;

        StretchToParent(gridPanel.GetComponent<RectTransform>());

        if (gridDimBackground == null)
        {
            Transform dimTransform = gridPanel.transform.Find("DimBackground");
            if (dimTransform != null)
                gridDimBackground = dimTransform.GetComponent<Image>();
        }

        if (gridDimBackground != null)
        {
            StretchToParent(gridDimBackground.GetComponent<RectTransform>());
            Color color = gridDimBackground.color;
            color.a = 0.6f;
            gridDimBackground.color = color;
        }

        ScrollRect scrollRect = gridPanel.GetComponentInChildren<ScrollRect>(true);
        if (scrollRect != null)
        {
            RectTransform scrollTransform = scrollRect.GetComponent<RectTransform>();
            SetAnchoredRect(scrollTransform, new Vector2(0.04f, 0.1f), new Vector2(0.96f, 0.82f), Vector2.zero, Vector2.zero);
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            Image scrollBackground = scrollRect.GetComponent<Image>();
            if (scrollBackground != null)
                scrollBackground.color = TutorialPanelBlue;
        }

        ConfigureGridInstructionText();

        if (thumbnailContent != null)
        {
            GridLayoutGroup gridLayout = thumbnailContent.GetComponent<GridLayoutGroup>();
            if (gridLayout == null)
                gridLayout = thumbnailContent.gameObject.AddComponent<GridLayoutGroup>();

            gridLayout.cellSize = new Vector2(ThumbnailSize, ThumbnailSize);
            gridLayout.spacing = new Vector2(28f, 28f);
            gridLayout.padding = new RectOffset(24, 24, 24, 24);
            gridLayout.childAlignment = TextAnchor.UpperCenter;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 4;

            ContentSizeFitter fitter = thumbnailContent.GetComponent<ContentSizeFitter>();
            if (fitter == null)
                fitter = thumbnailContent.gameObject.AddComponent<ContentSizeFitter>();

            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        if (backButton != null)
        {
            RectTransform backTransform = backButton.GetComponent<RectTransform>();
            SetAnchoredRect(backTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 52.5f), new Vector2(320f, 88f));
            ApplyMenuButtonHoverColors(backButton);
        }
    }

    private void ConfigureFullscreenPanel()
    {
        if (fullscreenPanel == null)
            return;

        StretchToParent(fullscreenPanel.GetComponent<RectTransform>());

        if (fullscreenBackground == null)
        {
            Image[] images = fullscreenPanel.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] != fullscreenImage)
                {
                    fullscreenBackground = images[i];
                    break;
                }
            }
        }

        if (fullscreenBackground != null)
        {
            StretchToParent(fullscreenBackground.GetComponent<RectTransform>());
            fullscreenBackground.color = Color.black;
        }

        if (fullscreenImage != null)
        {
            SetAnchoredRect(fullscreenImage.GetComponent<RectTransform>(), new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.88f), Vector2.zero, Vector2.zero);
            fullscreenImage.preserveAspect = true;
        }

        if (fullscreenInstructionText != null)
        {
            SetAnchoredRect(fullscreenInstructionText.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -40f), new Vector2(-80f, 60f));
            fullscreenInstructionText.alignment = TextAlignmentOptions.Center;
            fullscreenInstructionText.text = FullscreenInstructionText;
        }
    }

    private void ConfigureGridInstructionText()
    {
        if (gridPanel == null)
            return;

        if (gridInstructionText == null)
        {
            GameObject textObject = new GameObject("GridInstructionText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(gridPanel.transform, false);
            gridInstructionText = textObject.GetComponent<TextMeshProUGUI>();
        }

        SetAnchoredRect(gridInstructionText.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -54f), new Vector2(-80f, 72f));
        gridInstructionText.alignment = TextAlignmentOptions.Center;
        gridInstructionText.fontSize = 36f;
        gridInstructionText.color = Color.white;
        gridInstructionText.raycastTarget = false;
        if (fullscreenInstructionText != null && fullscreenInstructionText.font != null)
            gridInstructionText.font = fullscreenInstructionText.font;

        gridInstructionText.text = GridInstructionText;
    }

    private void StretchToParent(RectTransform rectTransform)
    {
        if (rectTransform == null)
            return;

        SetAnchoredRect(rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
    }

    private void SetAnchoredRect(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        if (rectTransform == null)
            return;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;
    }

    private void ApplyMenuButtonHoverColors(Button button)
    {
        if (button == null)
            return;

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = ButtonHoverBlue;
        colors.pressedColor = ButtonPressedBlue;
        colors.selectedColor = new Color(0.9607843f, 0.9607843f, 0.9607843f, 1f);
        colors.disabledColor = new Color(0.78431374f, 0.78431374f, 0.78431374f, 0.5019608f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.1f;
        button.transition = Selectable.Transition.ColorTint;
        button.colors = colors;
    }
}
