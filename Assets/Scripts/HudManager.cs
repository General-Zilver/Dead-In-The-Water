using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HudManager : MonoBehaviour
{
    public static HudManager Instance;
    private const float EnemyIconSize = 28f;
    private const float EnemyCountFontSize = 28f;

    [Header("Air Bar")]
    public Slider airSlider;
    public Image airFill; // The image that represents the filled portion of the air bar
    public Color airFullColor = new Color(0f, 1f, 0f, 1f); // Green color for full air
    public Color airLowColor = new Color(1f, 0f, 0f, 1f); // Red color for low air
    public float lowAirThreshold = 0.33f; // The percentage of air at which the color changes to red, .33 cause scoba diver rule

    [Header("Low Air Warning")]
    public float lowAirWarningThreshold = 0.3f;
    public float lowAirWarningPulseSpeed = 4f;
    public float lowAirWarningMaxAlpha = 0.45f;
    public float lowAirWarningEdgeThickness = 48f;
    private Image[] lowAirWarningEdges;
    private float currentAirPercent = 1f;

    [Header("Health")]
    public TextMeshProUGUI healthText;
    public Image[] hearticons; // Image elements for the health hearts

    [Header ("Score")]
    public TextMeshProUGUI scoreText;

    [Header("Objectives")]
    public TextMeshProUGUI keyText;
    public TextMeshProUGUI enemyStatsText;
    public bool createMissingObjectiveHud = true;
    private TextMeshProUGUI[] killedCountTexts;

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

    private void Update()
    {
        UpdateLowAirWarning();
    }

    // Air

    public void UpdateAir(float normalizedValue)
    {
        currentAirPercent = Mathf.Clamp01(normalizedValue);

        if (airSlider != null)
        {
            airSlider.value = currentAirPercent;
        }

        if (airFill != null)
        {
            airFill.color = Color.Lerp(airLowColor, airFullColor, currentAirPercent);
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

    private void Start()
    {
        EnsureLowAirWarningExists();

        if (createMissingObjectiveHud)
            EnsureObjectiveHudExists();

        UpdateKeysCollected(0, 0);
        UpdateEnemyStats(null, null);
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString("N0");
        }


    }

    public void UpdateKeysCollected(int collectedKeys, int requiredKeys)
    {
        if (keyText != null)
        {
            keyText.text = "x " + collectedKeys.ToString("00");
        }
    }

    public void UpdateEnemyStats(int[] aliveCounts, int[] killedCounts)
    {
        UpdateKilledCountText(EnemyType.Shark, killedCounts);
        UpdateKilledCountText(EnemyType.SawShark, killedCounts);
        UpdateKilledCountText(EnemyType.SeaAngler, killedCounts);
        UpdateKilledCountText(EnemyType.SwordFish, killedCounts);

        if (enemyStatsText != null)
            enemyStatsText.text = "";
    }

    private void EnsureLowAirWarningExists()
    {
        if (lowAirWarningEdges != null && lowAirWarningEdges.Length > 0)
            return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
            return;

        GameObject warningRoot = new GameObject("Low Air Warning", typeof(RectTransform));
        warningRoot.layer = canvas.gameObject.layer;
        warningRoot.transform.SetParent(canvas.transform, false);
        warningRoot.transform.SetAsLastSibling();

        RectTransform rootTransform = warningRoot.GetComponent<RectTransform>();
        rootTransform.anchorMin = Vector2.zero;
        rootTransform.anchorMax = Vector2.one;
        rootTransform.offsetMin = Vector2.zero;
        rootTransform.offsetMax = Vector2.zero;

        lowAirWarningEdges = new Image[4];
        lowAirWarningEdges[0] = CreateLowAirWarningEdge(warningRoot.transform, "Top", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, lowAirWarningEdgeThickness));
        lowAirWarningEdges[1] = CreateLowAirWarningEdge(warningRoot.transform, "Bottom", Vector2.zero, new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, lowAirWarningEdgeThickness));
        lowAirWarningEdges[2] = CreateLowAirWarningEdge(warningRoot.transform, "Left", Vector2.zero, new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(lowAirWarningEdgeThickness, 0f));
        lowAirWarningEdges[3] = CreateLowAirWarningEdge(warningRoot.transform, "Right", new Vector2(1f, 0f), Vector2.one, new Vector2(1f, 0.5f), new Vector2(lowAirWarningEdgeThickness, 0f));

        SetLowAirWarningAlpha(0f);
    }

    private Image CreateLowAirWarningEdge(Transform parent, string edgeName, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta)
    {
        GameObject edgeObject = new GameObject(edgeName + " Edge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        edgeObject.layer = parent.gameObject.layer;
        edgeObject.transform.SetParent(parent, false);

        RectTransform rectTransform = edgeObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = sizeDelta;

        Image image = edgeObject.GetComponent<Image>();
        image.color = new Color(1f, 0f, 0f, 0f);
        image.raycastTarget = false;

        return image;
    }

    private void UpdateLowAirWarning()
    {
        if (lowAirWarningEdges == null || lowAirWarningEdges.Length == 0)
            return;

        if (currentAirPercent > lowAirWarningThreshold)
        {
            SetLowAirWarningAlpha(0f);
            return;
        }

        float threshold = Mathf.Max(0.001f, lowAirWarningThreshold);
        float warningStrength = Mathf.InverseLerp(threshold, 0f, currentAirPercent);
        float pulse = (Mathf.Sin(Time.time * lowAirWarningPulseSpeed) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(lowAirWarningMaxAlpha * 0.35f, lowAirWarningMaxAlpha, pulse) * warningStrength;

        SetLowAirWarningAlpha(alpha);
    }

    private void SetLowAirWarningAlpha(float alpha)
    {
        for (int i = 0; i < lowAirWarningEdges.Length; i++)
        {
            if (lowAirWarningEdges[i] == null)
                continue;

            Color color = lowAirWarningEdges[i].color;
            color.a = alpha;
            lowAirWarningEdges[i].color = color;
        }
    }

    private void UpdateKilledCountText(EnemyType enemyType, int[] killedCounts)
    {
        int index = (int)enemyType;
        if (killedCountTexts == null || index < 0 || index >= killedCountTexts.Length || killedCountTexts[index] == null)
            return;

        int count = 0;
        if (killedCounts != null && index < killedCounts.Length)
            count = killedCounts[index];

        killedCountTexts[index].text = "x " + count.ToString("00");
    }

    private void EnsureObjectiveHudExists()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
            return;

        if (killedCountTexts == null || killedCountTexts.Length != System.Enum.GetValues(typeof(EnemyType)).Length)
            killedCountTexts = new TextMeshProUGUI[System.Enum.GetValues(typeof(EnemyType)).Length];

        CreateKeyCounter(canvas.transform, new Vector2(-250f, -55f));
        CreateKilledCounter(canvas.transform, "Sharks Alive", "Shark", EnemyType.Shark, new Vector2(-250f, -95f));
        CreateKilledCounter(canvas.transform, "SawSharks Alive", "SawShark", EnemyType.SawShark, new Vector2(-250f, -135f));
        CreateKilledCounter(canvas.transform, "SeaAngler Alive", "SeaAngler", EnemyType.SeaAngler, new Vector2(-250f, -175f));
        CreateKilledCounter(canvas.transform, "SwordFish Alive", "SwordFish", EnemyType.SwordFish, new Vector2(-250f, -215f));
    }

    private TextMeshProUGUI CreateHudText(Transform parent, string objectName, Vector2 anchoredPosition, Vector2 size, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.layer = parent.gameObject.layer;
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(1f, 1f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.fontSize = 20f;
        text.enableAutoSizing = true;
        text.fontSizeMin = 12f;
        text.fontSizeMax = 20f;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;

        return text;
    }

    private void CreateKeyCounter(Transform parent, Vector2 iconPosition)
    {
        CreateHudIcon(parent, "Keys Collected", "Key Count Icon", iconPosition);

        TextMeshProUGUI countText = parent.Find("Key Count")?.GetComponent<TextMeshProUGUI>();
        if (countText == null)
        {
            Vector2 textPosition = iconPosition + new Vector2(6f, 0f);
            countText = CreateHudText(parent, "Key Count", textPosition, new Vector2(90f, EnemyIconSize), TextAlignmentOptions.MidlineLeft);
            RectTransform countRectTransform = countText.GetComponent<RectTransform>();
            countRectTransform.pivot = new Vector2(0f, 0.5f);
            countRectTransform.anchoredPosition = textPosition;
            countText.fontSize = EnemyCountFontSize;
            countText.fontSizeMin = EnemyCountFontSize;
            countText.fontSizeMax = EnemyCountFontSize;
            countText.enableAutoSizing = false;
            countText.text = "x 00";
        }

        keyText = countText;
    }

    private void CreateKilledCounter(Transform parent, string sourceObjectName, string enemyName, EnemyType enemyType, Vector2 iconPosition)
    {
        CreateHudIcon(parent, sourceObjectName, enemyName + " Killed Icon", iconPosition);

        int index = (int)enemyType;
        TextMeshProUGUI countText = parent.Find(enemyName + " Killed Count")?.GetComponent<TextMeshProUGUI>();
        if (countText == null)
        {
            Vector2 textPosition = iconPosition + new Vector2(6f, 0f);
            countText = CreateHudText(parent, enemyName + " Killed Count", textPosition, new Vector2(90f, EnemyIconSize), TextAlignmentOptions.MidlineLeft);
            RectTransform countRectTransform = countText.GetComponent<RectTransform>();
            countRectTransform.pivot = new Vector2(0f, 0.5f);
            countRectTransform.anchoredPosition = textPosition;
            countText.fontSize = EnemyCountFontSize;
            countText.fontSizeMin = EnemyCountFontSize;
            countText.fontSizeMax = EnemyCountFontSize;
            countText.enableAutoSizing = false;
            countText.text = "x 00";
        }

        if (index >= 0 && index < killedCountTexts.Length)
            killedCountTexts[index] = countText;
    }

    private void CreateHudIcon(Transform parent, string sourceObjectName, string iconObjectName, Vector2 anchoredPosition)
    {
        if (parent.Find(iconObjectName) != null)
            return;

        GameObject sourceObject = GameObject.Find(sourceObjectName);
        SpriteRenderer sourceRenderer = sourceObject != null ? sourceObject.GetComponent<SpriteRenderer>() : null;
        if (sourceRenderer == null || sourceRenderer.sprite == null)
            return;

        GameObject iconObject = new GameObject(iconObjectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObject.layer = parent.gameObject.layer;
        iconObject.transform.SetParent(parent, false);

        RectTransform rectTransform = iconObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(1f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(EnemyIconSize, EnemyIconSize);

        Image image = iconObject.GetComponent<Image>();
        image.sprite = sourceRenderer.sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;

        sourceObject.name = sourceObjectName.Replace("Alive", "Killed Source").Replace("Collected", "Collected Source");
        sourceObject.SetActive(false);
    }





}
