using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class GuestInspectorBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureInspector()
    {
        GuestStatsPanelUI panel = Object.FindObjectOfType<GuestStatsPanelUI>();
        if (panel == null)
            panel = CreateDefaultPanel();

        GuestClickInspector inspector = Object.FindObjectOfType<GuestClickInspector>();
        if (inspector == null)
        {
            GameObject inspectorObject = new GameObject("GuestClickInspector");
            inspector = inspectorObject.AddComponent<GuestClickInspector>();
        }

        inspector.Configure(panel, Camera.main);
        EnsureEventSystem();
    }

    static GuestStatsPanelUI CreateDefaultPanel()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
            canvas = CreateCanvas();

        GameObject panelObject = new GameObject("GuestStatsPanel", typeof(RectTransform), typeof(Image), typeof(GuestStatsPanelUI));
        panelObject.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(20f, -20f);
        panelRect.sizeDelta = new Vector2(520f, 300f);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.72f);

        TextMeshProUGUI typeText = CreateLabel(panelRect, "TypeText", new Vector2(14f, -14f), 25f);
        TextMeshProUGUI thirstText = CreateLabel(panelRect, "ThirstText", new Vector2(14f, -46f), 22f);
        TextMeshProUGUI funText = CreateLabel(panelRect, "FunText", new Vector2(14f, -74f), 22f);
        TextMeshProUGUI socialText = CreateLabel(panelRect, "SocialText", new Vector2(14f, -102f), 22f);
        TextMeshProUGUI energyText = CreateLabel(panelRect, "EnergyText", new Vector2(14f, -130f), 22f);
        TextMeshProUGUI influenceText = CreateLabel(panelRect, "InfluenceText", new Vector2(14f, -158f), 22f);
        TextMeshProUGUI detailsText = CreateLabel(panelRect, "DetailsText", new Vector2(14f, -194f), 18f, 96f);

        GuestStatsPanelUI panelUI = panelObject.GetComponent<GuestStatsPanelUI>();
        panelUI.Configure(panelObject, typeText, thirstText, funText, socialText, energyText, influenceText, detailsText);
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

    static TextMeshProUGUI CreateLabel(RectTransform parent, string objectName, Vector2 anchoredPosition, float fontSize = 22f, float height = 22f)
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

    static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null)
            return;

        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }
}
