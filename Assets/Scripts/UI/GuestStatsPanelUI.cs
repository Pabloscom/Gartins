using TMPro;
using UnityEngine;

public class GuestStatsPanelUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI guestTypeText;
    [SerializeField] private TextMeshProUGUI thirstText;
    [SerializeField] private TextMeshProUGUI funText;
    [SerializeField] private TextMeshProUGUI socialText;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI influenceText;
    [SerializeField] private TextMeshProUGUI detailsText;
    [SerializeField] private bool showDetailedMetrics = true;
    [SerializeField] private float refreshInterval = 0.1f;

    private GuestNeeds currentNeeds;
    private GuestPersonality currentPersonality;
    private float refreshTimer;

    void Awake()
    {
        refreshInterval = Mathf.Max(0.02f, refreshInterval);
        ValidateConfiguration();
        Hide();
    }

    void OnValidate()
    {
        refreshInterval = Mathf.Max(0.02f, refreshInterval);
    }

    void Update()
    {
        if (currentNeeds == null || currentPersonality == null)
        {
            Hide();
            return;
        }

        refreshTimer -= Time.deltaTime;
        if (refreshTimer > 0f)
            return;

        refreshTimer = refreshInterval;
        RefreshTexts();
    }

    public void ShowFor(GuestNeeds needs, GuestPersonality personality)
    {
        if (needs == null || personality == null)
        {
            Hide();
            return;
        }

        currentNeeds = needs;
        currentPersonality = personality;
        refreshTimer = 0f;

        if (panelRoot != null)
            panelRoot.SetActive(true);

        RefreshTexts();
    }

    public void Hide()
    {
        currentNeeds = null;
        currentPersonality = null;
        refreshTimer = 0f;

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    void RefreshTexts()
    {
        if (currentNeeds == null || currentPersonality == null)
            return;

        if (guestTypeText != null)
            guestTypeText.text = "Tipo: " + FormatPersonality(currentPersonality.personalityType);

        if (thirstText != null)
            thirstText.text = "Sed: " + currentNeeds.thirst.ToString("0");

        if (funText != null)
            funText.text = "Diversion: " + currentNeeds.fun.ToString("0");

        if (socialText != null)
            socialText.text = "Social: " + currentNeeds.social.ToString("0");

        if (energyText != null)
            energyText.text = "Energia: " + currentNeeds.energy.ToString("0");

        if (influenceText != null)
            influenceText.text = "Influencia: " + currentPersonality.influenceScore.ToString("0.00");

        if (detailsText != null)
        {
            if (!showDetailedMetrics)
            {
                detailsText.text = string.Empty;
                return;
            }

            GuestNeedType dominantNeed = currentNeeds.GetDominantNeed(currentPersonality);

            detailsText.text =
                "Dominante: " + dominantNeed + "\n" +
                "Ganancia/s - Sed: +" + currentNeeds.thirstGainPerSecond.ToString("0.0") +
                " | Div: +" + currentNeeds.funGainPerSecond.ToString("0.0") +
                " | Soc: +" + currentNeeds.socialGainPerSecond.ToString("0.0") +
                " | Ene: +" + currentNeeds.energyGainPerSecond.ToString("0.0") + "\n" +
                "Pesos - Sed: x" + currentPersonality.thirstWeight.ToString("0.00") +
                " | Div: x" + currentPersonality.funWeight.ToString("0.00") +
                " | Soc: x" + currentPersonality.socialWeight.ToString("0.00") +
                " | Ene: x" + currentPersonality.energyWeight.ToString("0.00") + "\n" +
                "Alivios - Bebida Sed: -" + currentNeeds.drinkThirstRelief.ToString("0") +
                " | Baile Div: -" + currentNeeds.danceFunRelief.ToString("0") +
                " | Charla Soc: -" + currentNeeds.talkSocialRelief.ToString("0");
        }
    }

    string FormatPersonality(GuestPersonalityType type)
    {
        switch (type)
        {
            case GuestPersonalityType.PartyAnimal:
                return "Party Animal";
            default:
                return type.ToString();
        }
    }

    public void Configure(
        GameObject newPanelRoot,
        TextMeshProUGUI newGuestTypeText,
        TextMeshProUGUI newThirstText,
        TextMeshProUGUI newFunText,
        TextMeshProUGUI newSocialText,
        TextMeshProUGUI newEnergyText,
        TextMeshProUGUI newInfluenceText,
        TextMeshProUGUI newDetailsText = null)
    {
        panelRoot = newPanelRoot;
        guestTypeText = newGuestTypeText;
        thirstText = newThirstText;
        funText = newFunText;
        socialText = newSocialText;
        energyText = newEnergyText;
        influenceText = newInfluenceText;
        detailsText = newDetailsText;
        ValidateConfiguration();
    }

    void ValidateConfiguration()
    {
        if (panelRoot == null)
            Debug.LogWarning("GuestStatsPanelUI: panelRoot no esta asignado.");

        if (showDetailedMetrics && detailsText == null)
            Debug.LogWarning("GuestStatsPanelUI: detailsText no esta asignado y no se mostraran metricas detalladas.");
    }
}
