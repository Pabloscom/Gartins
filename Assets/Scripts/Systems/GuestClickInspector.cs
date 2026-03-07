using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GuestClickInspector : MonoBehaviour
{
    [SerializeField] private Camera worldCamera;
    [SerializeField] private LayerMask guestLayerMask = ~0;
    [SerializeField] private GuestStatsPanelUI guestStatsPanel;

    void Awake()
    {
        if (worldCamera == null)
            worldCamera = Camera.main;

        if (guestStatsPanel == null)
            guestStatsPanel = FindObjectOfType<GuestStatsPanelUI>();
    }

    void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (worldCamera == null)
            worldCamera = Camera.main;

        if (guestStatsPanel == null)
            guestStatsPanel = FindObjectOfType<GuestStatsPanelUI>();

        if (worldCamera == null)
            return;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 world = worldCamera.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, 0f));

        Collider2D hit = Physics2D.OverlapPoint(world, guestLayerMask);
        if (hit == null)
        {
            guestStatsPanel?.Hide();
            return;
        }

        GuestNeeds needs = ResolveFromCollider<GuestNeeds>(hit);
        GuestPersonality personality = ResolveFromCollider<GuestPersonality>(hit);

        if (needs != null && personality != null)
            guestStatsPanel?.ShowFor(needs, personality);
        else
            guestStatsPanel?.Hide();
    }

    T ResolveFromCollider<T>(Collider2D col) where T : Component
    {
        if (col == null)
            return null;

        T component = col.GetComponent<T>();
        if (component != null)
            return component;

        component = col.GetComponentInParent<T>();
        if (component != null)
            return component;

        if (col.attachedRigidbody != null)
            component = col.attachedRigidbody.GetComponent<T>();

        return component;
    }

    public void Configure(GuestStatsPanelUI panel, Camera cameraRef = null)
    {
        guestStatsPanel = panel;

        if (cameraRef != null)
            worldCamera = cameraRef;
        else if (worldCamera == null)
            worldCamera = Camera.main;
    }
}
