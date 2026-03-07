using UnityEngine;
using System;
using System.Collections;

public class PopularitySystem : MonoBehaviour
{
    [Header("Inputs")]
    public AmbienceSystem ambienceSystem;
    public int softCapGuests = 120;

    [Header("State")]
    public int influencerGuests;
    [Range(0f, 100f)] public float clubReputation;

    [Header("Weights")]
    [Range(0f, 1f)] public float ambienceWeight = 0.5f;
    [Range(0f, 1f)] public float occupancyWeight = 0.3f;
    [Range(0f, 1f)] public float influencerWeight = 0.2f;

    public event Action<float> ReputationChanged;
    private bool subscribed;
    private Coroutine subscribeRoutine;

    void Awake()
    {
        if (ambienceSystem == null)
            ambienceSystem = FindObjectOfType<AmbienceSystem>();
    }

    void OnEnable()
    {
        TrySubscribe();

        if (!subscribed)
            subscribeRoutine = StartCoroutine(SubscribeWhenGameManagerAvailable());

        RecalculateReputation();
    }

    void OnDisable()
    {
        if (subscribeRoutine != null)
        {
            StopCoroutine(subscribeRoutine);
            subscribeRoutine = null;
        }

        Unsubscribe();
    }

    public void RegisterGuest(GuestPersonality personality)
    {
        if (personality != null && personality.IsInfluencer())
            influencerGuests++;

        RecalculateReputation();
    }

    public void UnregisterGuest(GuestPersonality personality)
    {
        if (personality != null && personality.IsInfluencer())
            influencerGuests = Mathf.Max(0, influencerGuests - 1);

        RecalculateReputation();
    }

    public float GetPopularityFactor()
    {
        return Mathf.Lerp(0.7f, 1.35f, clubReputation / 100f);
    }

    void OnDataChanged(int _)
    {
        RecalculateReputation();
    }

    void OnDataChanged(float _)
    {
        RecalculateReputation();
    }

    void TrySubscribe()
    {
        if (subscribed || GameManager.Instance == null)
            return;

        GameManager.Instance.GuestCountChanged += OnDataChanged;
        GameManager.Instance.AmbienceChanged += OnDataChanged;
        subscribed = true;
    }

    void Unsubscribe()
    {
        if (!subscribed || GameManager.Instance == null)
            return;

        GameManager.Instance.GuestCountChanged -= OnDataChanged;
        GameManager.Instance.AmbienceChanged -= OnDataChanged;
        subscribed = false;
    }

    void RecalculateReputation()
    {
        int guests = GameManager.Instance != null ? GameManager.Instance.currentGuests : 0;
        float ambience01 = ambienceSystem != null ? ambienceSystem.ambience / 100f : 0f;
        float occupancy01 = softCapGuests > 0 ? Mathf.Clamp01((float)guests / softCapGuests) : 0f;
        float influencer01 = Mathf.Clamp01(influencerGuests / 12f);

        float normalized = ambience01 * ambienceWeight +
                           occupancy01 * occupancyWeight +
                           influencer01 * influencerWeight;

        normalized = Mathf.Clamp01(normalized);
        float newReputation = normalized * 100f;

        if (Mathf.Approximately(clubReputation, newReputation))
            return;

        clubReputation = newReputation;
        ReputationChanged?.Invoke(clubReputation);
    }

    IEnumerator SubscribeWhenGameManagerAvailable()
    {
        WaitForSeconds wait = new WaitForSeconds(0.25f);

        while (!subscribed)
        {
            TrySubscribe();

            if (!subscribed)
                yield return wait;
        }

        subscribeRoutine = null;
    }
}
