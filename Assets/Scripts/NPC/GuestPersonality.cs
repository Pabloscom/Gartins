using UnityEngine;

public enum GuestPersonalityType
{
    Drinker,
    PartyAnimal,
    Chill,
    Social,
    Influencer
}

public class GuestPersonality : MonoBehaviour
{
    [Header("Setup")]
    public bool randomizeOnAwake = true;
    public GuestPersonalityType personalityType = GuestPersonalityType.Social;

    [Header("Need Weights")]
    [Range(0.25f, 2.5f)] public float thirstWeight = 1f;
    [Range(0.25f, 2.5f)] public float funWeight = 1f;
    [Range(0.25f, 2.5f)] public float socialWeight = 1f;
    [Range(0.25f, 2.5f)] public float energyWeight = 1f;

    [Header("Popularity")]
    [Range(0f, 3f)] public float influenceScore = 0f;

    void Awake()
    {
        if (randomizeOnAwake)
            SetPersonality((GuestPersonalityType)Random.Range(0, 5));
        else
            ApplyPreset(personalityType);
    }

    public void SetPersonality(GuestPersonalityType type)
    {
        personalityType = type;
        ApplyPreset(type);
    }

    public float GetWeight(GuestNeedType needType)
    {
        switch (needType)
        {
            case GuestNeedType.Thirst:
                return thirstWeight;
            case GuestNeedType.Fun:
                return funWeight;
            case GuestNeedType.Social:
                return socialWeight;
            case GuestNeedType.Energy:
                return energyWeight;
            default:
                return 1f;
        }
    }

    public bool IsInfluencer()
    {
        return personalityType == GuestPersonalityType.Influencer;
    }

    void ApplyPreset(GuestPersonalityType type)
    {
        switch (type)
        {
            case GuestPersonalityType.Drinker:
                thirstWeight = 1.9f;
                funWeight = 0.8f;
                socialWeight = 0.7f;
                energyWeight = 0.9f;
                influenceScore = 0.1f;
                break;

            case GuestPersonalityType.PartyAnimal:
                thirstWeight = 0.9f;
                funWeight = 1.9f;
                socialWeight = 1.2f;
                energyWeight = 0.8f;
                influenceScore = 0.25f;
                break;

            case GuestPersonalityType.Chill:
                thirstWeight = 0.7f;
                funWeight = 0.9f;
                socialWeight = 1.1f;
                energyWeight = 1.5f;
                influenceScore = 0.15f;
                break;

            case GuestPersonalityType.Social:
                thirstWeight = 0.8f;
                funWeight = 1f;
                socialWeight = 1.8f;
                energyWeight = 1f;
                influenceScore = 0.2f;
                break;

            case GuestPersonalityType.Influencer:
                thirstWeight = 1f;
                funWeight = 1.6f;
                socialWeight = 1.5f;
                energyWeight = 0.9f;
                influenceScore = 1.25f;
                break;
        }
    }
}
