using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    private static GameInput instance;

    public static GameInput Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameInput>();

                if (instance == null)
                {
                    GameObject go = new GameObject("GameInput");
                    instance = go.AddComponent<GameInput>();
                }
            }

            return instance;
        }
    }

    private PlayerInputActions actions;

    public InputAction MoveAction => actions.Player.Move;
    public InputAction InteractAction => actions.Player.Interact;
    public InputAction StartPartyAction => actions.Player.StartParty;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (actions == null)
            actions = new PlayerInputActions();
    }

    void OnEnable()
    {
        actions?.Enable();
    }

    void OnDisable()
    {
        actions?.Disable();
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;

        actions?.Dispose();
        actions = null;
    }
}
