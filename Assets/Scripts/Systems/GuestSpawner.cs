using System.Collections;
using UnityEngine;

public class GuestSpawner : MonoBehaviour
{
    public GameObject guestPrefab;
    public Transform spawnPoint;

    public int maxGuests = 120;

    public float spawnCheckInterval = 1f;
    public float waveInterval = 20f;
    public float intraGroupSpawnDelay = 0.35f;
    public int openingGuaranteedGuests = 2;
    public float emptyClubRespawnDelay = 3f;

    public TimeSystem timeSystem;
    public BarQueueSystem barQueueSystem;
    public AmbienceSystem ambienceSystem;
    public PopularitySystem popularitySystem;
    public GroupSpawnSystem groupSpawnSystem;

    private bool wasClubOpen;
    private float nextEmptyClubRespawnTime;
    private GameObject spawnPrefab;
    private GameObject runtimeGuestTemplate;
    private WaitForSeconds spawnCheckWait;
    private WaitForSeconds waveWait;
    private WaitForSeconds intraGroupWait;
    private float cachedSpawnCheckInterval = -1f;
    private float cachedWaveInterval = -1f;
    private float cachedIntraGroupSpawnDelay = -1f;

    void Start()
    {
        ResolveDependencies();

        if (!ValidateDependencies())
        {
            enabled = false;
            return;
        }

        ResolveSpawnPrefab();
        RefreshWaitInstructions(force: true);

        wasClubOpen = timeSystem != null && timeSystem.clubOpen;
        nextEmptyClubRespawnTime = 0f;

        StartCoroutine(SpawnRoutine());
        StartCoroutine(WaveRoutine());
    }

    void OnValidate()
    {
        maxGuests = Mathf.Max(1, maxGuests);
        spawnCheckInterval = Mathf.Max(0.05f, spawnCheckInterval);
        waveInterval = Mathf.Max(0.1f, waveInterval);
        intraGroupSpawnDelay = Mathf.Max(0f, intraGroupSpawnDelay);
        openingGuaranteedGuests = Mathf.Max(1, openingGuaranteedGuests);
        emptyClubRespawnDelay = Mathf.Max(0f, emptyClubRespawnDelay);
        RefreshWaitInstructions(force: false);
    }

    void OnDestroy()
    {
        if (runtimeGuestTemplate != null)
            Destroy(runtimeGuestTemplate);
    }

    void Update()
    {
        if (timeSystem == null || GameManager.Instance == null)
            return;

        bool isClubOpen = timeSystem.clubOpen;

        if (isClubOpen && !wasClubOpen)
        {
            int guaranteed = Mathf.Max(1, openingGuaranteedGuests);
            StartCoroutine(SpawnGroup(guaranteed));
            nextEmptyClubRespawnTime = Time.time + Mathf.Max(0f, emptyClubRespawnDelay);
        }

        wasClubOpen = isClubOpen;
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            RefreshWaitInstructions(force: false);
            yield return spawnCheckWait;

            if (!CanSpawn())
                continue;

            if (GameManager.Instance.currentGuests <= 0 && Time.time >= nextEmptyClubRespawnTime)
            {
                SpawnGuest();
                nextEmptyClubRespawnTime = Time.time + Mathf.Max(0f, emptyClubRespawnDelay);
                continue;
            }

            float crowdFactor = (float)GameManager.Instance.currentGuests / maxGuests;
            float ambienceFactor = GameManager.Instance.ambiente / 100f;
            float popularityFactor = popularitySystem != null ? popularitySystem.GetPopularityFactor() : 1f;
            float spawnChance = 0.15f * ambienceFactor * (1f - crowdFactor) * popularityFactor;
            spawnChance = Mathf.Clamp01(spawnChance);

            if (Random.value < spawnChance)
            {
                int spawnCount = GetSpawnCount();
                yield return SpawnGroup(spawnCount);
            }
        }
    }

    IEnumerator WaveRoutine()
    {
        while (true)
        {
            RefreshWaitInstructions(force: false);
            yield return waveWait;

            if (!CanSpawn())
                continue;

            float ambienceFactor = GameManager.Instance.ambiente / 100f;
            int minWave = Mathf.Max(1, Mathf.RoundToInt(Mathf.Lerp(1f, 3f, ambienceFactor)));
            int maxWave = Mathf.Max(minWave, Mathf.RoundToInt(Mathf.Lerp(3f, 8f, ambienceFactor)));
            int guestsToSpawn = Random.Range(minWave, maxWave + 1);

            yield return SpawnGroup(guestsToSpawn);
        }
    }

    IEnumerator SpawnGroup(int totalToSpawn)
    {
        int remaining = totalToSpawn;

        while (remaining > 0 && CanSpawn())
        {
            int chunk = Mathf.Min(remaining, GetSpawnCount());

            for (int i = 0; i < chunk; i++)
            {
                if (!CanSpawn())
                    yield break;

                SpawnGuest();
                if (intraGroupWait != null)
                    yield return intraGroupWait;
            }

            remaining -= chunk;
        }
    }

    bool CanSpawn()
    {
        if (spawnPrefab == null || spawnPoint == null || timeSystem == null || GameManager.Instance == null)
            return false;

        if (!timeSystem.clubOpen)
            return false;

        if (maxGuests <= 0)
            return false;

        return GameManager.Instance.currentGuests < maxGuests;
    }

    void SpawnGuest()
    {
        if (!CanSpawn())
            return;

        GameObject guestObject = Instantiate(spawnPrefab, spawnPoint.position, Quaternion.identity);
        guestObject.SetActive(true);

        GuestPersonality personality = guestObject.GetComponent<GuestPersonality>();
        if (personality == null)
            personality = guestObject.AddComponent<GuestPersonality>();

        if (guestObject.GetComponent<GuestNeeds>() == null)
            guestObject.AddComponent<GuestNeeds>();

        if (guestObject.GetComponent<GuestDecisionSystem>() == null)
            guestObject.AddComponent<GuestDecisionSystem>();

        GuestMovement guestMovement = guestObject.GetComponent<GuestMovement>();
        if (guestMovement == null)
        {
            Destroy(guestObject);
            return;
        }

        guestMovement.Configure(timeSystem, barQueueSystem, ambienceSystem, popularitySystem);

        popularitySystem?.RegisterGuest(personality);
        GameManager.Instance.AddGuest();
    }

    void ResolveSpawnPrefab()
    {
        spawnPrefab = guestPrefab;

        if (guestPrefab == null || !guestPrefab.scene.IsValid())
            return;

        bool sourceWasActive = guestPrefab.activeSelf;
        if (sourceWasActive)
            guestPrefab.SetActive(false);

        runtimeGuestTemplate = Instantiate(guestPrefab, transform);
        runtimeGuestTemplate.name = guestPrefab.name + "_SpawnTemplate";
        runtimeGuestTemplate.SetActive(false);
        spawnPrefab = runtimeGuestTemplate;

        if (sourceWasActive)
            Debug.LogWarning("GuestSpawner: guestPrefab estaba referenciando un objeto de escena activo. Se desactivo y se creo un template runtime para el spawn.");
    }

    int GetSpawnCount()
    {
        if (groupSpawnSystem == null)
            return 1;

        float ambience01 = GameManager.Instance != null ? GameManager.Instance.ambiente / 100f : 0f;
        float popularity01 = popularitySystem != null ? popularitySystem.clubReputation / 100f : 0f;
        int influencerGuests = popularitySystem != null ? popularitySystem.influencerGuests : 0;

        return Mathf.Max(1, groupSpawnSystem.GetSpawnCount(ambience01, popularity01, influencerGuests));
    }

    void ResolveDependencies()
    {
        if (timeSystem == null)
            timeSystem = FindObjectOfType<TimeSystem>();

        if (barQueueSystem == null)
            barQueueSystem = FindObjectOfType<BarQueueSystem>();

        if (ambienceSystem == null)
            ambienceSystem = FindObjectOfType<AmbienceSystem>();

        if (popularitySystem == null)
            popularitySystem = FindObjectOfType<PopularitySystem>();

        if (groupSpawnSystem == null)
            groupSpawnSystem = FindObjectOfType<GroupSpawnSystem>();
    }

    bool ValidateDependencies()
    {
        bool valid = true;

        if (timeSystem == null)
        {
            Debug.LogError("GuestSpawner: falta referencia a TimeSystem.");
            valid = false;
        }

        if (ambienceSystem == null)
        {
            Debug.LogError("GuestSpawner: falta referencia a AmbienceSystem.");
            valid = false;
        }

        if (popularitySystem == null)
        {
            Debug.LogError("GuestSpawner: falta referencia a PopularitySystem.");
            valid = false;
        }
        else if (ambienceSystem != null && popularitySystem.ambienceSystem == null)
        {
            popularitySystem.ambienceSystem = ambienceSystem;
        }

        if (groupSpawnSystem == null)
        {
            Debug.LogError("GuestSpawner: falta referencia a GroupSpawnSystem.");
            valid = false;
        }

        if (barQueueSystem == null)
            Debug.LogWarning("GuestSpawner: BarQueueSystem no asignado. Los guests iran directo al bar si no encuentran cola.");

        return valid;
    }

    void RefreshWaitInstructions(bool force)
    {
        float safeSpawnCheckInterval = Mathf.Max(0.05f, spawnCheckInterval);
        float safeWaveInterval = Mathf.Max(0.1f, waveInterval);
        float safeIntraGroupSpawnDelay = Mathf.Max(0f, intraGroupSpawnDelay);

        if (force || !Mathf.Approximately(cachedSpawnCheckInterval, safeSpawnCheckInterval))
        {
            cachedSpawnCheckInterval = safeSpawnCheckInterval;
            spawnCheckWait = new WaitForSeconds(safeSpawnCheckInterval);
        }

        if (force || !Mathf.Approximately(cachedWaveInterval, safeWaveInterval))
        {
            cachedWaveInterval = safeWaveInterval;
            waveWait = new WaitForSeconds(safeWaveInterval);
        }

        if (force || !Mathf.Approximately(cachedIntraGroupSpawnDelay, safeIntraGroupSpawnDelay))
        {
            cachedIntraGroupSpawnDelay = safeIntraGroupSpawnDelay;
            intraGroupWait = safeIntraGroupSpawnDelay > 0f ? new WaitForSeconds(safeIntraGroupSpawnDelay) : null;
        }
    }
}
