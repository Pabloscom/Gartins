using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int currentGuests;
    public float ambiente;

    public int money = 50;

    public event Action<int> MoneyChanged;
    public event Action<int> GuestCountChanged;
    public event Action<float> AmbienceChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        NotifyState();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void AddMoney(int amount)
    {
        money += amount;
        MoneyChanged?.Invoke(money);

        Debug.Log("Money: " + money);
    }

    public bool TrySpendMoney(int amount)
    {
        if (amount <= 0)
            return true;

        if (money < amount)
            return false;

        money -= amount;
        MoneyChanged?.Invoke(money);
        return true;
    }

    public void SetAmbiente(float value)
    {
        float clamped = Mathf.Clamp(value, 0f, 100f);
        if (Mathf.Approximately(ambiente, clamped))
            return;

        ambiente = clamped;
        AmbienceChanged?.Invoke(ambiente);
    }

    public void AddGuest()
    {
        currentGuests++;
        GuestCountChanged?.Invoke(currentGuests);
    }

    public void RemoveGuest()
    {
        currentGuests = Mathf.Max(0, currentGuests - 1);
        GuestCountChanged?.Invoke(currentGuests);
    }

    void NotifyState()
    {
        MoneyChanged?.Invoke(money);
        GuestCountChanged?.Invoke(currentGuests);
        AmbienceChanged?.Invoke(ambiente);
    }
}
