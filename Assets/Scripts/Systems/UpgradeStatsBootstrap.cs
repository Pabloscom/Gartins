using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class UpgradeStatsBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureUpgradePanel()
    {
        UpgradeStatsPanelUI panel = Object.FindObjectOfType<UpgradeStatsPanelUI>();
        if (panel == null)
            panel = CreateDefaultPanel();

        if (panel == null)
            return;

        UpgradeClickInspector inspector = Object.FindObjectOfType<UpgradeClickInspector>();
        if (inspector == null)
        {
            GameObject inspectorObject = new GameObject("UpgradeClickInspector");
            inspector = inspectorObject.AddComponent<UpgradeClickInspector>();
        }

        inspector.Configure(panel, Camera.main);
    }

    static UpgradeStatsPanelUI CreateDefaultPanel()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
            canvas = CreateCanvas();

        GameObject panelObject = new GameObject("UpgradeStatsPanel", typeof(RectTransform), typeof(Image), typeof(UpgradeStatsPanelUI));
        panelObject.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.anchoredPosition = new Vector2(-20f, -20f);
        panelRect.sizeDelta = new Vector2(620f, 300f);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.72f);

        TextMeshProUGUI titleText = CreateLabel(panelRect, "TitleText", new Vector2(16f, -14f), 34f);
        TextMeshProUGUI barLevelText = CreateLabel(panelRect, "BarLevelText", new Vector2(16f, -68f), 28f);
        TextMeshProUGUI barCostText = CreateLabel(panelRect, "BarCostText", new Vector2(16f, -112f), 28f);
        TextMeshProUGUI barBenefitText = CreateLabel(panelRect, "BarBenefitText", new Vector2(16f, -156f), 28f);
        TextMeshProUGUI musicLevelText = CreateLabel(panelRect, "MusicLevelText", new Vector2(16f, -68f), 28f);
        TextMeshProUGUI musicCostText = CreateLabel(panelRect, "MusicCostText", new Vector2(16f, -112f), 28f);
        TextMeshProUGUI musicBenefitText = CreateLabel(panelRect, "MusicBenefitText", new Vector2(16f, -156f), 28f);
        TextMeshProUGUI contextText = CreateLabel(panelRect, "ContextText", new Vector2(16f, -210f), 18f, 72f);

        UpgradeStatsPanelUI panelUI = panelObject.GetComponent<UpgradeStatsPanelUI>();
        panelUI.Configure(
            panelObject,
            titleText,
            barLevelText,
            barCostText,
            barBenefitText,
            musicLevelText,
            musicCostText,
            musicBenefitText,
            contextText);
        panelUI.Hide();

        return panelUI;
    }

    static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }

    static TextMeshProUGUI CreateLabel(RectTransform parent, string objectName, Vector2 anchoredPosition, float fontSize, float height = 36f)
    {
        GameObject labelObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(parent, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 1f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.pivot = new Vector2(0f, 1f);
        labelRect.anchoredPosition = anchoredPosition;
        labelRect.sizeDelta = new Vector2(-28f, height);

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.text = string.Empty;
        label.fontSize = fontSize;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Left;
        label.enableWordWrapping = true;

        return label;
    }
}
