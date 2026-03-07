using TMPro;
using UnityEngine;

public class UpgradeStatsPanelUI : MonoBehaviour
{
    private const float TitleFontSize = 34f;
    private const float DetailFontSize = 28f;

    private enum UpgradeFocus
    {
        None,
        Bar,
        Music
    }

    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI barLevelText;
    [SerializeField] private TextMeshProUGUI barCostText;
    [SerializeField] private TextMeshProUGUI barBenefitText;
    [SerializeField] private TextMeshProUGUI musicLevelText;
    [SerializeField] private TextMeshProUGUI musicCostText;
    [SerializeField] private TextMeshProUGUI musicBenefitText;
    [SerializeField] private TextMeshProUGUI contextText;

    private AmbienceSystem ambienceSystem;
    private PopularitySystem popularitySystem;
    private UpgradeFocus currentFocus = UpgradeFocus.None;
    [SerializeField] private float refreshInterval = 0.2f;
    private float refreshTimer;

    void Awake()
    {
        if (panelRoot == null)
            panelRoot = gameObject;

        EnsureContextText();
        ApplyReadableFontSizes();
        Hide();
    }

    void OnValidate()
    {
        refreshInterval = Mathf.Max(0.02f, refreshInterval);
    }

    void Update()
    {
        if (ambienceSystem == null)
            ambienceSystem = FindObjectOfType<AmbienceSystem>();

        if (popularitySystem == null)
            popularitySystem = FindObjectOfType<PopularitySystem>();

        if (currentFocus == UpgradeFocus.None)
            return;

        refreshTimer -= Time.deltaTime;
        if (refreshTimer > 0f)
            return;

        refreshTimer = refreshInterval;
        RefreshTexts();
    }

    public void Configure(
        GameObject newPanelRoot,
        TextMeshProUGUI newTitleText,
        TextMeshProUGUI newBarLevelText,
        TextMeshProUGUI newBarCostText,
        TextMeshProUGUI newBarBenefitText,
        TextMeshProUGUI newMusicLevelText,
        TextMeshProUGUI newMusicCostText,
        TextMeshProUGUI newMusicBenefitText,
        TextMeshProUGUI newContextText = null)
    {
        panelRoot = newPanelRoot;
        titleText = newTitleText;
        barLevelText = newBarLevelText;
        barCostText = newBarCostText;
        barBenefitText = newBarBenefitText;
        musicLevelText = newMusicLevelText;
        musicCostText = newMusicCostText;
        musicBenefitText = newMusicBenefitText;
        contextText = newContextText;
        EnsureContextText();
        ApplyReadableFontSizes();
    }

    public void ShowBarDetails()
    {
        currentFocus = UpgradeFocus.Bar;
        SetPanelVisibility(true);
        SetDetailVisibility(showBar: true, showMusic: false);
        refreshTimer = 0f;
        RefreshTexts();
    }

    public void ShowMusicDetails()
    {
        currentFocus = UpgradeFocus.Music;
        SetPanelVisibility(true);
        SetDetailVisibility(showBar: false, showMusic: true);
        refreshTimer = 0f;
        RefreshTexts();
    }

    public void Hide()
    {
        currentFocus = UpgradeFocus.None;
        SetPanelVisibility(false);
    }

    void RefreshTexts()
    {
        if (currentFocus == UpgradeFocus.None)
            return;

        if (ambienceSystem == null)
        {
            if (titleText != null)
                titleText.text = currentFocus == UpgradeFocus.Bar ? "Mejora: Bar" : "Mejora: Musica";

            if (currentFocus == UpgradeFocus.Bar)
            {
                if (barLevelText != null)
                    barLevelText.text = "Nivel: -";
                if (barCostText != null)
                    barCostText.text = "Costo: -";
                if (barBenefitText != null)
                    barBenefitText.text = "Beneficio: -";
            }
            else if (currentFocus == UpgradeFocus.Music)
            {
                if (musicLevelText != null)
                    musicLevelText.text = "Nivel: -";
                if (musicCostText != null)
                    musicCostText.text = "Costo: -";
                if (musicBenefitText != null)
                    musicBenefitText.text = "Beneficio: -";
            }

            return;
        }

        int maxLevel = ambienceSystem.MaxLevel;

        if (currentFocus == UpgradeFocus.Bar)
            RefreshBarTexts(maxLevel);
        else if (currentFocus == UpgradeFocus.Music)
            RefreshMusicTexts(maxLevel);
    }

    void RefreshBarTexts(int maxLevel)
    {
        int barLevel = ambienceSystem.barLevel;
        bool barAtMax = barLevel >= maxLevel;
        int barCurrentReturn = ambienceSystem.GetDrinkReturnForLevel(barLevel);
        int barNextReturn = ambienceSystem.GetDrinkReturnForLevel(barLevel + 1);

        if (titleText != null)
            titleText.text = "Mejora: Bar";

        if (barLevelText != null)
            barLevelText.text = "Nivel: " + barLevel + "/" + maxLevel;

        if (barCostText != null)
            barCostText.text = barAtMax ? "Costo: MAX" : "Costo: $" + ambienceSystem.GetBarUpgradeCost();

        if (barBenefitText != null)
        {
            if (barAtMax)
                barBenefitText.text = "Beneficio: bebida $" + barCurrentReturn + " (MAX)";
            else
                barBenefitText.text = "Beneficio: bebida $" + barCurrentReturn + " (sig: $" + barNextReturn + ")";
        }

        RefreshContextText("Bar", barAtMax);
    }

    void RefreshMusicTexts(int maxLevel)
    {
        int musicLevel = ambienceSystem.danceLevel;
        bool musicAtMax = musicLevel >= maxLevel;
        int musicCurrentAmbience = ambienceSystem.GetDanceAmbienceForLevel(musicLevel);
        int musicNextAmbience = ambienceSystem.GetDanceAmbienceForLevel(musicLevel + 1);

        if (titleText != null)
            titleText.text = "Mejora: Musica";

        if (musicLevelText != null)
            musicLevelText.text = "Nivel: " + musicLevel + "/" + maxLevel;

        if (musicCostText != null)
            musicCostText.text = musicAtMax ? "Costo: MAX" : "Costo: $" + ambienceSystem.GetDanceUpgradeCost();

        if (musicBenefitText != null)
        {
            if (musicAtMax)
                musicBenefitText.text = "Beneficio: ambiente +" + musicCurrentAmbience + " (MAX)";
            else
                musicBenefitText.text = "Beneficio: ambiente +" + musicCurrentAmbience + " (sig: +" + musicNextAmbience + ")";
        }

        RefreshContextText("Musica", musicAtMax);
    }

    void SetPanelVisibility(bool visible)
    {
        if (panelRoot != null)
            panelRoot.SetActive(visible);
    }

    void SetDetailVisibility(bool showBar, bool showMusic)
    {
        SetTextGroupVisibility(barLevelText, barCostText, barBenefitText, showBar);
        SetTextGroupVisibility(musicLevelText, musicCostText, musicBenefitText, showMusic);
    }

    void SetTextGroupVisibility(TextMeshProUGUI line1, TextMeshProUGUI line2, TextMeshProUGUI line3, bool visible)
    {
        if (line1 != null)
            line1.gameObject.SetActive(visible);

        if (line2 != null)
            line2.gameObject.SetActive(visible);

        if (line3 != null)
            line3.gameObject.SetActive(visible);
    }

    void ApplyReadableFontSizes()
    {
        SetFontSize(titleText, TitleFontSize);
        SetFontSize(barLevelText, DetailFontSize);
        SetFontSize(barCostText, DetailFontSize);
        SetFontSize(barBenefitText, DetailFontSize);
        SetFontSize(musicLevelText, DetailFontSize);
        SetFontSize(musicCostText, DetailFontSize);
        SetFontSize(musicBenefitText, DetailFontSize);
        SetFontSize(contextText, 18f);
    }

    void SetFontSize(TextMeshProUGUI text, float size)
    {
        if (text == null)
            return;

        if (text.fontSize < size)
            text.fontSize = size;
    }

    void RefreshContextText(string focusName, bool atMaxLevel)
    {
        if (contextText == null || ambienceSystem == null)
            return;

        string ambiencePart = "Ambiente Club: " + ambienceSystem.ambience + "/100";
        string reputationPart = popularitySystem != null
            ? "Reputacion: " + popularitySystem.clubReputation.ToString("0.0")
            : "Reputacion: -";
        string upgradePart = atMaxLevel ? focusName + " al maximo" : "Se puede mejorar " + focusName;

        contextText.text = ambiencePart + " | " + reputationPart + "\n" + upgradePart;
    }

    void EnsureContextText()
    {
        if (contextText != null || panelRoot == null)
            return;

        RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
        if (panelRect == null)
            return;

        GameObject contextObject = new GameObject("ContextText", typeof(RectTransform), typeof(TextMeshProUGUI));
        contextObject.transform.SetParent(panelRect, false);

        RectTransform contextRect = contextObject.GetComponent<RectTransform>();
        contextRect.anchorMin = new Vector2(0f, 1f);
        contextRect.anchorMax = new Vector2(1f, 1f);
        contextRect.pivot = new Vector2(0f, 1f);
        contextRect.anchoredPosition = new Vector2(16f, -210f);
        contextRect.sizeDelta = new Vector2(-28f, 72f);

        contextText = contextObject.GetComponent<TextMeshProUGUI>();
        contextText.fontSize = 18f;
        contextText.color = Color.white;
        contextText.alignment = TextAlignmentOptions.Left;
        contextText.enableWordWrapping = true;
    }
}
