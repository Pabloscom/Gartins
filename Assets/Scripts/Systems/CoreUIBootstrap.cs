using TMPro;
using UnityEngine;

public static class CoreUIBootstrap
{
    public static bool TryWire(
        MoneyUI moneyUI,
        TextMeshProUGUI moneyLabel,
        TimeSystem timeSystem,
        GameObject startText)
    {
        bool moneyOk = TryWireMoneyText(moneyUI, moneyLabel);
        bool startTextOk = TryWireStartPartyText(timeSystem, startText);
        return moneyOk && startTextOk;
    }

    static bool TryWireMoneyText(MoneyUI moneyUI, TextMeshProUGUI moneyLabel)
    {
        if (moneyUI == null)
        {
            Debug.LogError("CoreUIBootstrap: No se encontro MoneyUI en escena.");
            return false;
        }

        TextMeshProUGUI resolvedLabel = moneyLabel != null ? moneyLabel : moneyUI.moneyText;
        if (resolvedLabel == null)
            resolvedLabel = moneyUI.GetComponent<TextMeshProUGUI>();

        if (resolvedLabel == null)
            resolvedLabel = moneyUI.GetComponentInChildren<TextMeshProUGUI>(true);

        if (resolvedLabel == null)
        {
            Debug.LogError("CoreUIBootstrap: MoneyUI no tiene TextMeshProUGUI asignado.");
            return false;
        }

        ConfigureMoneyLabel(resolvedLabel);
        moneyUI.Configure(resolvedLabel);
        return true;
    }

    static bool TryWireStartPartyText(TimeSystem timeSystem, GameObject startText)
    {
        if (timeSystem == null)
        {
            Debug.LogError("CoreUIBootstrap: No se encontro TimeSystem en escena.");
            return false;
        }

        GameObject startObject = startText != null ? startText : timeSystem.startText;
        if (startObject == null)
        {
            Debug.LogError("CoreUIBootstrap: TimeSystem.startText no esta asignado.");
            return false;
        }

        TextMeshProUGUI startLabel = startObject.GetComponent<TextMeshProUGUI>();
        if (startLabel == null)
        {
            Debug.LogError("CoreUIBootstrap: StartPartyText no tiene TextMeshProUGUI.");
            return false;
        }

        timeSystem.startText = startObject;

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
        return true;
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
}
