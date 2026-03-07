using UnityEngine;

public class GroupSpawnSystem : MonoBehaviour
{
    [Header("Group Size")]
    [Range(2, 4)] public int minGroupSize = 2;
    [Range(2, 4)] public int maxGroupSize = 4;

    [Header("Chance")]
    [Range(0f, 1f)] public float baseGroupChance = 0.35f;
    [Range(0f, 1f)] public float ambienceInfluence = 0.25f;
    [Range(0f, 1f)] public float popularityInfluence = 0.25f;
    [Range(0f, 1f)] public float influencerInfluence = 0.15f;

    public int GetSpawnCount(float ambience01, float popularity01, int influencerGuests)
    {
        if (maxGroupSize < minGroupSize)
            maxGroupSize = minGroupSize;

        float influencer01 = Mathf.Clamp01(influencerGuests / 10f);
        float groupChance = baseGroupChance +
                            ambience01 * ambienceInfluence +
                            popularity01 * popularityInfluence +
                            influencer01 * influencerInfluence;

        groupChance = Mathf.Clamp01(groupChance);

        if (Random.value > groupChance)
            return 1;

        return Random.Range(minGroupSize, maxGroupSize + 1);
    }
}
