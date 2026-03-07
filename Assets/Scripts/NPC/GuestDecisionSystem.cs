using UnityEngine;

public enum GuestIntent
{
    GoToBar,
    GoToDance,
    GoToSocial,
    LeaveClub
}

public class GuestDecisionSystem : MonoBehaviour
{
    [Header("Decision Thresholds")]
    [Range(0f, 100f)] public float leaveByEnergyThreshold = 92f;
    [Range(0f, 100f)] public float leaveByStayThreshold = 95f;

    public GuestIntent Decide(GuestNeeds needs, GuestPersonality personality, float stayProgress01)
    {
        if (needs == null)
            return RandomIntent();

        if (needs.energy >= leaveByEnergyThreshold || stayProgress01 >= leaveByStayThreshold / 100f)
            return GuestIntent.LeaveClub;

        GuestNeedType dominantNeed = needs.GetDominantNeed(personality);

        switch (dominantNeed)
        {
            case GuestNeedType.Thirst:
                return GuestIntent.GoToBar;

            case GuestNeedType.Fun:
                return GuestIntent.GoToDance;

            case GuestNeedType.Social:
                return GuestIntent.GoToSocial;

            case GuestNeedType.Energy:
                return GuestIntent.GoToSocial;

            default:
                return RandomIntent();
        }
    }

    GuestIntent RandomIntent()
    {
        float roll = Random.value;

        if (roll < 0.33f)
            return GuestIntent.GoToDance;

        if (roll < 0.66f)
            return GuestIntent.GoToBar;

        return GuestIntent.GoToSocial;
    }
}
