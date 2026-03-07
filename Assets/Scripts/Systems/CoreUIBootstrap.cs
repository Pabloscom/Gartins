using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class CoreUIBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureCoreUI()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
            canvas = CreateCanvas();

        EnsureMoneyText(canvas);
        EnsureStartPartyText(canvas);
    }

    static void EnsureMoneyText(Canvas canvas)
    {
        if (canvas == null)
            return;

        MoneyUI moneyUI = Object.FindObjectOfType<MoneyUI>();
        TextMeshProUGUI moneyLabel = null;

        if (moneyUI != null)
        {
            moneyLabel = moneyUI.moneyText;
            if (moneyLabel == null)
                moneyLabel = moneyUI.GetComponent<TextMeshProUGUI>();

            if (moneyLabel == null)
                moneyLabel = moneyUI.GetComponentInChildren<TextMeshProUGUI>(true);
        }
        else
        {
            GameObject moneyObject = new GameObject("MoneyText", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(MoneyUI));
            moneyObject.transform.SetParent(canvas.transform, false);
            ConfigureRect(
                moneyObject.GetComponent<RectTransform>(),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -20f),
                new Vector2(360f, 46f));

            moneyUI = moneyObject.GetComponent<MoneyUI>();
            moneyLabel = moneyObject.GetComponent<TextMeshProUGUI>();
        }

        if (moneyLabel == null)
            return;

        ConfigureMoneyLabel(moneyLabel);
        moneyUI.Configure(moneyLabel);
    }

    static void EnsureStartPartyText(Canvas canvas)
    {
        if (canvas == null)
            return;

        TimeSystem timeSystem = Object.FindObjectOfType<TimeSystem>();
        if (timeSystem == null)
            return;

        GameObject startObject = timeSystem.startText;
        if (startObject == null)
        {
            startObject = GameObject.Find("StartPartyText");

            if (startObject == null)
            {
                startObject = new GameObject("StartPartyText", typeof(RectTransform), typeof(TextMeshProUGUI));
                startObject.transform.SetParent(canvas.transform, false);
            }

            timeSystem.startText = startObject;
        }

        TextMeshProUGUI startLabel = startObject.GetComponent<TextMeshProUGUI>();
        if (startLabel == null)
            startLabel = startObject.AddComponent<TextMeshProUGUI>();

        ConfigureRect(
            startObject.GetComponent<RectTransform>(),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, 140f),
            new Vector2(760f, 64f));

        startLabel.text = "Presiona X para empezar fiesta";
        startLabel.fontSize = 38f;
        startLabel.color = new Color(1f, 0.96f, 0.75f, 1f);
        startLabel.alignment = TextAlignmentOptions.Center;
        startLabel.enableWordWrapping = true;

        startObject.SetActive(!timeSystem.clubOpen);
    }

    static void ConfigureMoneyLabel(TextMeshProUGUI moneyLabel)
    {
        moneyLabel.fontSize = 40f;
        moneyLabel.color = Color.white;
        moneyLabel.alignment = TextAlignmentOptions.Center;
        moneyLabel.enableWordWrapping = false;
    }

    static void ConfigureRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        if (rect == null)
            return;

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
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
}
