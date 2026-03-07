using UnityEngine;
using UnityEngine.InputSystem;

public class TimeSystem : MonoBehaviour
{
    [Header("Time")]
    public float currentHour = 15f;
    public float timeSpeed = 0.5f;

    [Header("State")]
    public bool partyStarted = false;
    public bool clubOpen = false;

    [Header("Maps")]
    public GameObject dayMap;
    public GameObject nightMap;

    [Header("UI")]
    public GameObject startText;

    private InputAction startPartyAction;

    void Awake()
    {
        startPartyAction = GameInput.Instance.StartPartyAction;
    }

    void OnEnable()
    {
        if (startPartyAction == null)
            startPartyAction = GameInput.Instance.StartPartyAction;

        if (startPartyAction != null)
            startPartyAction.performed += OnStartPartyPerformed;
    }

    void OnDisable()
    {
        if (startPartyAction != null)
            startPartyAction.performed -= OnStartPartyPerformed;
    }

    void OnStartPartyPerformed(InputAction.CallbackContext _)
    {
        TryStartParty();
    }

    void Update()
    {
        if (!clubOpen)
            return;

        UpdateClock();
    }

    void TryStartParty()
    {
        if (!partyStarted)
        {
            StartParty();
        }
    }

    void UpdateClock()
    {
        currentHour += Time.deltaTime * timeSpeed;

        if (currentHour >= 24f)
            currentHour = 0f;

        if (currentHour >= 5f)
        {
            CloseClub();
        }
    }

    void StartParty()
    {
        partyStarted = true;
        clubOpen = true;

        currentHour = 0f;

        if (dayMap != null)
            dayMap.SetActive(false);

        if (nightMap != null)
            nightMap.SetActive(true);

        if (startText != null)
            startText.SetActive(false);

        Debug.Log("La fiesta empezo!");
    }

    void CloseClub()
    {
        clubOpen = false;

        if (nightMap != null)
            nightMap.SetActive(false);

        if (dayMap != null)
            dayMap.SetActive(true);

        currentHour = 15f;
        partyStarted = false;

        if (startText != null)
            startText.SetActive(true);

        Debug.Log("La fiesta termino!");
    }
}
