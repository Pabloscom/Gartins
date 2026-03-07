using UnityEngine;

public enum GuestNeedType
{
    Thirst,
    Fun,
    Social,
    Energy
}

public class GuestNeeds : MonoBehaviour
{
    [Header("Need Pressure (0-100)")]
    [Range(0f, 100f)] public float thirst = 20f;
    [Range(0f, 100f)] public float fun = 15f;
    [Range(0f, 100f)] public float social = 15f;
    [Range(0f, 100f)] public float energy = 10f;

    [Header("Need Gain / sec")]
    public float thirstGainPerSecond = 2.5f;
    public float funGainPerSecond = 1.6f;
    public float socialGainPerSecond = 1.3f;
    public float energyGainPerSecond = 0.9f;
    [Range(0f, 100f)] public float exhaustedThreshold = 95f;

    [Header("Activity Relief")]
    public float drinkThirstRelief = 55f;
    public float danceFunRelief = 30f;
    public float danceSocialRelief = 18f;
    public float talkSocialRelief = 35f;
    public float talkEnergyRelief = 12f;

    public bool IsExhausted => energy >= exhaustedThreshold;

    public void Tick(float deltaTime)
    {
        if (deltaTime <= 0f)
            return;

        thirst = ClampNeed(thirst + thirstGainPerSecond * deltaTime);
        fun = ClampNeed(fun + funGainPerSecond * deltaTime);
        social = ClampNeed(social + socialGainPerSecond * deltaTime);
        energy = ClampNeed(energy + energyGainPerSecond * deltaTime);
    }

    public void OnDrinkServed()
    {
        thirst = ClampNeed(thirst - drinkThirstRelief);
        energy = ClampNeed(energy + 4f);
    }

    public void OnDance()
    {
        fun = ClampNeed(fun - danceFunRelief);
        social = ClampNeed(social - danceSocialRelief);
        energy = ClampNeed(energy + 7f);
    }

    public void OnTalk()
    {
        social = ClampNeed(social - talkSocialRelief);
        fun = ClampNeed(fun - 8f);
        energy = ClampNeed(energy - talkEnergyRelief);
    }

    public GuestNeedType GetDominantNeed(GuestPersonality personality)
    {
        float thirstScore = GetWeightedNeed(thirst, GuestNeedType.Thirst, personality);
        float funScore = GetWeightedNeed(fun, GuestNeedType.Fun, personality);
        float socialScore = GetWeightedNeed(social, GuestNeedType.Social, personality);
        float energyScore = GetWeightedNeed(energy, GuestNeedType.Energy, personality);

        GuestNeedType dominant = GuestNeedType.Thirst;
        float best = thirstScore;

        if (funScore > best)
        {
            dominant = GuestNeedType.Fun;
            best = funScore;
        }

        if (socialScore > best)
        {
            dominant = GuestNeedType.Social;
            best = socialScore;
        }

        if (energyScore > best)
            dominant = GuestNeedType.Energy;

        return dominant;
    }

    float GetWeightedNeed(float value, GuestNeedType needType, GuestPersonality personality)
    {
        float weight = personality != null ? personality.GetWeight(needType) : 1f;
        return value * weight;
    }

    float ClampNeed(float value)
    {
        return Mathf.Clamp(value, 0f, 100f);
    }
}
