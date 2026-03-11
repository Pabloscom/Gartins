using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
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
                    Debug.LogError("GameInput: no existe instancia en escena. Agrega un GameObject con GameInput.");
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
