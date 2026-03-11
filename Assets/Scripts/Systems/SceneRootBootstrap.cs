using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class SceneRootBootstrap : MonoBehaviour
{
    [Header("Execution")]
    [SerializeField] private bool initializeOnAwake = true;
    [SerializeField] private bool requireCoreUi = false;
    [SerializeField] private bool requireInspectorUi = false;
    [SerializeField] private bool requireSpawnerDependencies = true;
    [SerializeField] private bool logSuccess = false;

    [Header("Core UI")]
    [SerializeField] private TimeSystem timeSystem;
    [SerializeField] private MoneyUI moneyUI;
    [SerializeField] private TextMeshProUGUI moneyLabel;
    [SerializeField] private GameObject startPartyText;

    [Header("Inspectors")]
    [SerializeField] private GuestClickInspector guestClickInspector;
    [SerializeField] private GuestStatsPanelUI guestStatsPanel;
    [SerializeField] private UpgradeClickInspector upgradeClickInspector;
    [SerializeField] private UpgradeStatsPanelUI upgradeStatsPanel;
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private Camera worldCamera;

    [Header("Spawner Wiring")]
    [SerializeField] private GuestSpawner guestSpawner;
    [SerializeField] private BarQueueSystem barQueueSystem;
    [SerializeField] private AmbienceSystem ambienceSystem;
    [SerializeField] private PopularitySystem popularitySystem;
    [SerializeField] private GroupSpawnSystem groupSpawnSystem;

    void Awake()
    {
        if (initializeOnAwake)
            Initialize();
    }

    [ContextMenu("Auto Resolve References")]
    public void AutoResolveReferences()
    {
        if (timeSystem == null)
            timeSystem = FindObjectOfType<TimeSystem>();

        if (moneyUI == null)
            moneyUI = FindObjectOfType<MoneyUI>();

        if (moneyLabel == null && moneyUI != null)
        {
            moneyLabel = moneyUI.moneyText;
            if (moneyLabel == null)
                moneyLabel = moneyUI.GetComponent<TextMeshProUGUI>();

            if (moneyLabel == null)
                moneyLabel = moneyUI.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (startPartyText == null && timeSystem != null)
            startPartyText = timeSystem.startText;

        if (guestClickInspector == null)
            guestClickInspector = FindObjectOfType<GuestClickInspector>();

        if (guestStatsPanel == null)
            guestStatsPanel = FindObjectOfType<GuestStatsPanelUI>();

        if (upgradeClickInspector == null)
            upgradeClickInspector = FindObjectOfType<UpgradeClickInspector>();

        if (upgradeStatsPanel == null)
            upgradeStatsPanel = FindObjectOfType<UpgradeStatsPanelUI>();

        if (eventSystem == null)
            eventSystem = FindObjectOfType<EventSystem>();

        if (worldCamera == null)
            worldCamera = Camera.main;

        if (guestSpawner == null)
            guestSpawner = FindObjectOfType<GuestSpawner>();

        if (barQueueSystem == null)
            barQueueSystem = FindObjectOfType<BarQueueSystem>();

        if (ambienceSystem == null)
            ambienceSystem = FindObjectOfType<AmbienceSystem>();

        if (popularitySystem == null)
            popularitySystem = FindObjectOfType<PopularitySystem>();

        if (groupSpawnSystem == null)
            groupSpawnSystem = FindObjectOfType<GroupSpawnSystem>();
    }

    [ContextMenu("Initialize Scene Wiring")]
    public void Initialize()
    {
        AutoResolveReferences();

        bool shouldWireCoreUi = requireCoreUi || moneyUI != null || timeSystem != null || startPartyText != null;
        bool shouldWireInspectorUi = requireInspectorUi ||
                                     guestClickInspector != null ||
                                     guestStatsPanel != null ||
                                     upgradeClickInspector != null ||
                                     upgradeStatsPanel != null;

        if (upgradeStatsPanel != null)
            upgradeStatsPanel.ConfigureDependencies(ambienceSystem, popularitySystem);

        bool coreUiOk = !shouldWireCoreUi || CoreUIBootstrap.TryWire(moneyUI, moneyLabel, timeSystem, startPartyText);
        bool guestInspectorOk = !shouldWireInspectorUi || GuestInspectorBootstrap.TryWire(guestClickInspector, guestStatsPanel, worldCamera, eventSystem);
        bool upgradeInspectorOk = !shouldWireInspectorUi || UpgradeStatsBootstrap.TryWire(upgradeClickInspector, upgradeStatsPanel, worldCamera);
        bool spawnerOk = WireSpawnerDependencies();

        bool requiredOk = (!requireCoreUi || coreUiOk) &&
                          (!requireInspectorUi || (guestInspectorOk && upgradeInspectorOk)) &&
                          (!requireSpawnerDependencies || spawnerOk);

        if (!requiredOk)
        {
            Debug.LogError("SceneRootBootstrap: faltan dependencias obligatorias en escena.");
            return;
        }

        if (logSuccess)
            Debug.Log("SceneRootBootstrap: inicializacion completada.");
    }

    bool WireSpawnerDependencies()
    {
        if (guestSpawner == null)
            return false;

        if (timeSystem != null)
            guestSpawner.timeSystem = timeSystem;

        if (barQueueSystem != null)
            guestSpawner.barQueueSystem = barQueueSystem;

        if (ambienceSystem != null)
            guestSpawner.ambienceSystem = ambienceSystem;

        if (popularitySystem != null)
            guestSpawner.popularitySystem = popularitySystem;

        if (groupSpawnSystem != null)
            guestSpawner.groupSpawnSystem = groupSpawnSystem;

        bool hasRequired = guestSpawner.timeSystem != null &&
                           guestSpawner.ambienceSystem != null &&
                           guestSpawner.popularitySystem != null &&
                           guestSpawner.groupSpawnSystem != null;

        if (!hasRequired)
            Debug.LogError("SceneRootBootstrap: GuestSpawner no tiene todas las dependencias obligatorias.");

        return hasRequired;
    }
}
