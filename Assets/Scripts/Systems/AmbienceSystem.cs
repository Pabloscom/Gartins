using UnityEngine;

public class AmbienceSystem : MonoBehaviour
{
    private const int BarAmbiencePerLevel = 10;
    private const int DanceAmbiencePerLevel = 15;

    [Header("Club Levels")]
    [Range(0, 3)] public int barLevel = 0;
    [Range(0, 3)] public int danceLevel = 0;
    [Range(0, 3)] public int decorLevel = 0;
    public int sofaAmbience = 5;
    public int decorAmbiencePerLevel = 8;

    [Header("Economy Tables (Level 0-3)")]
    public int[] upgradeCostsByLevel = { 10, 500, 1000, 5000 };
    public int[] drinkReturnByLevel = { 10, 30, 50, 100 };

    [Header("Result")]
    public int ambience;

    public int MaxLevel => 3;
    private bool ambienceDirty = true;
    private int lastBarLevel = -1;
    private int lastDanceLevel = -1;
    private int lastDecorLevel = -1;
    private int lastSofaAmbience = -1;

    void Start()
    {
        RecalculateIfNeeded(force: true);
    }

    void LateUpdate()
    {
        if (barLevel != lastBarLevel ||
            danceLevel != lastDanceLevel ||
            decorLevel != lastDecorLevel ||
            sofaAmbience != lastSofaAmbience)
        {
            ambienceDirty = true;
        }

        RecalculateIfNeeded(force: false);
    }

    void OnValidate()
    {
        ambienceDirty = true;
    }

    void CalculateAmbience()
    {
        int barAmbience = barLevel * BarAmbiencePerLevel;
        int danceAmbience = danceLevel * DanceAmbiencePerLevel;
        int decorAmbience = decorLevel * decorAmbiencePerLevel;

        ambience = barAmbience + danceAmbience + decorAmbience + sofaAmbience;
        ambience = Mathf.Clamp(ambience, 0, 100);

        if (GameManager.Instance != null)
            GameManager.Instance.SetAmbiente(ambience);
    }

    void RecalculateIfNeeded(bool force)
    {
        if (!force && !ambienceDirty)
            return;

        ambienceDirty = false;
        CalculateAmbience();
        lastBarLevel = barLevel;
        lastDanceLevel = danceLevel;
        lastDecorLevel = decorLevel;
        lastSofaAmbience = sofaAmbience;
    }

    int GetValueForLevel(int[] values, int level, int fallback)
    {
        if (values == null || values.Length == 0)
            return fallback;

        int index = Mathf.Clamp(level, 0, values.Length - 1);
        return values[index];
    }

    public int GetBarUpgradeCost()
    {
        return GetValueForLevel(upgradeCostsByLevel, barLevel, 10);
    }

    public int GetDanceUpgradeCost()
    {
        return GetValueForLevel(upgradeCostsByLevel, danceLevel, 10);
    }

    public int GetCurrentDrinkReturn()
    {
        return GetValueForLevel(drinkReturnByLevel, barLevel, 10);
    }

    public int GetDrinkReturnForLevel(int level)
    {
        return GetValueForLevel(drinkReturnByLevel, level, 10);
    }

    public int GetDanceAmbienceForLevel(int level)
    {
        int safeLevel = Mathf.Clamp(level, 0, MaxLevel);
        return safeLevel * DanceAmbiencePerLevel;
    }

    public int GetDecorUpgradeCost()
    {
        return GetValueForLevel(upgradeCostsByLevel, decorLevel, 10);
    }

    public bool TryUpgradeBar()
    {
        if (barLevel >= MaxLevel || GameManager.Instance == null)
            return false;

        int cost = GetBarUpgradeCost();
        if (!GameManager.Instance.TrySpendMoney(cost))
            return false;

        barLevel++;
        ambienceDirty = true;
        RecalculateIfNeeded(force: true);
        return true;
    }

    public bool TryUpgradeDance()
    {
        if (danceLevel >= MaxLevel || GameManager.Instance == null)
            return false;

        int cost = GetDanceUpgradeCost();
        if (!GameManager.Instance.TrySpendMoney(cost))
            return false;

        danceLevel++;
        ambienceDirty = true;
        RecalculateIfNeeded(force: true);
        return true;
    }

    public bool TryUpgradeDecor()
    {
        if (decorLevel >= MaxLevel || GameManager.Instance == null)
            return false;

        int cost = GetDecorUpgradeCost();
        if (!GameManager.Instance.TrySpendMoney(cost))
            return false;

        decorLevel++;
        ambienceDirty = true;
        RecalculateIfNeeded(force: true);
        return true;
    }

    public bool TryUpgradeMusic()
    {
        return TryUpgradeDance();
    }
}
