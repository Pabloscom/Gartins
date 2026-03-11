using UnityEngine;

public static class UpgradeStatsBootstrap
{
    public static bool TryWire(
        UpgradeClickInspector inspector,
        UpgradeStatsPanelUI panel,
        Camera worldCamera)
    {
        if (panel == null)
        {
            Debug.LogError("UpgradeStatsBootstrap: No se encontro UpgradeStatsPanelUI en escena.");
            return false;
        }

        if (inspector == null)
        {
            Debug.LogError("UpgradeStatsBootstrap: No se encontro UpgradeClickInspector en escena.");
            return false;
        }

        inspector.Configure(panel, worldCamera != null ? worldCamera : Camera.main);
        return true;
    }
}
