using UnityEngine;
using UnityEngine.EventSystems;

public static class GuestInspectorBootstrap
{
    public static bool TryWire(
        GuestClickInspector inspector,
        GuestStatsPanelUI panel,
        Camera worldCamera,
        EventSystem eventSystem)
    {
        if (panel == null)
        {
            Debug.LogError("GuestInspectorBootstrap: No se encontro GuestStatsPanelUI en escena.");
            return false;
        }

        if (inspector == null)
        {
            Debug.LogError("GuestInspectorBootstrap: No se encontro GuestClickInspector en escena.");
            return false;
        }

        inspector.Configure(panel, worldCamera != null ? worldCamera : Camera.main);
        return EnsureEventSystem(eventSystem);
    }

    static bool EnsureEventSystem(EventSystem eventSystem)
    {
        if (eventSystem != null)
            return true;

        Debug.LogError("GuestInspectorBootstrap: No se encontro EventSystem en escena.");
        return false;
    }
}
