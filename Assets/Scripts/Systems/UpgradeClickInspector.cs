using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UpgradeClickInspector : MonoBehaviour
{
    [SerializeField] private Camera worldCamera;
    [SerializeField] private UpgradeStatsPanelUI upgradeStatsPanel;
    [SerializeField] private Transform barUpgradePoint;
    [SerializeField] private Transform musicUpgradePoint;
    [SerializeField] private float clickRadius = 1.2f;

    void Awake()
    {
        ResolveReferences();
    }

    void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        ResolveReferences();

        if (worldCamera == null || upgradeStatsPanel == null)
            return;

        Vector2 clickPosition = worldCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        bool barClicked = IsClicked(barUpgradePoint, clickPosition);
        bool musicClicked = IsClicked(musicUpgradePoint, clickPosition);

        if (!barClicked && !musicClicked)
        {
            upgradeStatsPanel.Hide();
            return;
        }

        if (barClicked && musicClicked)
        {
            float barDistance = Vector2.Distance(clickPosition, barUpgradePoint.position);
            float musicDistance = Vector2.Distance(clickPosition, musicUpgradePoint.position);
            if (barDistance <= musicDistance)
                upgradeStatsPanel.ShowBarDetails();
            else
                upgradeStatsPanel.ShowMusicDetails();
            return;
        }

        if (barClicked)
            upgradeStatsPanel.ShowBarDetails();
        else
            upgradeStatsPanel.ShowMusicDetails();
    }

    bool IsClicked(Transform point, Vector2 clickPosition)
    {
        if (point == null)
            return false;

        return Vector2.Distance(clickPosition, point.position) <= clickRadius;
    }

    void ResolveReferences()
    {
        if (worldCamera == null)
            worldCamera = Camera.main;

        if (upgradeStatsPanel == null)
            upgradeStatsPanel = FindObjectOfType<UpgradeStatsPanelUI>();

        if (barUpgradePoint == null)
            barUpgradePoint = ScenePointResolver.FindTransform("Bar Point", "BarPoint");

        if (musicUpgradePoint == null)
            musicUpgradePoint = ScenePointResolver.FindTransform("DancePoint", "Dance Point", "MusicPoint", "Music Point");
    }

    public void Configure(UpgradeStatsPanelUI panel, Camera cameraRef = null)
    {
        upgradeStatsPanel = panel;

        if (cameraRef != null)
            worldCamera = cameraRef;
        else if (worldCamera == null)
            worldCamera = Camera.main;
    }
}
