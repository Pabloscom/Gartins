using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public float interactRadius = 1.5f;
    public float upgradeRadius = 2f;
    public Transform barUpgradePoint;
    public Transform musicUpgradePoint;

    private InputAction interactAction;
    private AmbienceSystem ambienceSystem;
    private TimeSystem timeSystem;
    private readonly Collider2D[] interactionBuffer = new Collider2D[32];

    void Awake()
    {
        interactAction = GameInput.Instance.InteractAction;
        ambienceSystem = FindObjectOfType<AmbienceSystem>();
        timeSystem = FindObjectOfType<TimeSystem>();
        ResolveUpgradePoints();
    }

    void OnEnable()
    {
        if (interactAction == null)
            interactAction = GameInput.Instance.InteractAction;

        if (interactAction != null)
            interactAction.started += OnInteractPressed;
    }

    void OnDisable()
    {
        if (interactAction != null)
            interactAction.started -= OnInteractPressed;
    }

    void OnInteractPressed(InputAction.CallbackContext _)
    {
        if (timeSystem == null)
            timeSystem = FindObjectOfType<TimeSystem>();

        if (timeSystem != null)
        {
            if (timeSystem.clubOpen)
            {
                TryServeDrink();
                return;
            }

            TryUpgradeSystems();
            return;
        }

        bool upgraded = TryUpgradeSystems();
        if (!upgraded)
            TryServeDrink();
    }

    bool TryUpgradeSystems()
    {
        if (ambienceSystem == null)
            ambienceSystem = FindObjectOfType<AmbienceSystem>();

        if (ambienceSystem == null)
            return false;

        ResolveUpgradePoints();

        bool nearBar = IsNearUpgradePoint(barUpgradePoint);
        bool nearMusic = IsNearUpgradePoint(musicUpgradePoint);

        if (!nearBar && !nearMusic)
            return false;

        bool upgradeBar;

        if (nearBar && nearMusic)
        {
            float barDistance = Vector2.Distance(transform.position, barUpgradePoint.position);
            float musicDistance = Vector2.Distance(transform.position, musicUpgradePoint.position);
            upgradeBar = barDistance <= musicDistance;
        }
        else
            upgradeBar = nearBar;

        bool upgraded = upgradeBar ? ambienceSystem.TryUpgradeBar() : ambienceSystem.TryUpgradeMusic();

        if (upgraded)
            Debug.Log("Level UP");

        return upgraded;
    }

    bool IsNearUpgradePoint(Transform upgradePoint)
    {
        if (upgradePoint == null)
            return false;

        return Vector2.Distance(transform.position, upgradePoint.position) <= upgradeRadius;
    }

    void ResolveUpgradePoints()
    {
        if (barUpgradePoint == null)
            barUpgradePoint = ScenePointResolver.FindTransform("Bar Point", "BarPoint");

        if (musicUpgradePoint == null)
            musicUpgradePoint = ScenePointResolver.FindTransform("DancePoint", "Dance Point", "MusicPoint", "Music Point");
    }

    void TryServeDrink()
    {
        int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, interactRadius, interactionBuffer);

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D col = interactionBuffer[i];
            if (col == null)
                continue;

            GuestMovement guest = col.GetComponent<GuestMovement>();

            if (guest == null)
                guest = col.GetComponentInParent<GuestMovement>();

            if (guest == null && col.attachedRigidbody != null)
                guest = col.attachedRigidbody.GetComponent<GuestMovement>();

            if (guest != null && guest.waitingForDrink)
            {
                guest.ServeDrink();
                return;
            }
        }
    }
}
