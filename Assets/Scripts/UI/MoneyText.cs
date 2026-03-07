using TMPro;
using UnityEngine;
using System.Collections;

public class MoneyUI : MonoBehaviour
{
    public TextMeshProUGUI moneyText;
    private bool subscribed;
    private Coroutine subscribeRoutine;

    void OnEnable()
    {
        EnsureTextReference();
        TrySubscribe();

        if (!subscribed)
            subscribeRoutine = StartCoroutine(SubscribeWhenGameManagerAvailable());

        RefreshMoney();
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

    void TrySubscribe()
    {
        if (subscribed || GameManager.Instance == null)
            return;

        GameManager.Instance.MoneyChanged += OnMoneyChanged;
        subscribed = true;
    }

    void Unsubscribe()
    {
        if (!subscribed || GameManager.Instance == null)
            return;

        GameManager.Instance.MoneyChanged -= OnMoneyChanged;
        subscribed = false;
    }

    void OnMoneyChanged(int _)
    {
        RefreshMoney();
    }

    void RefreshMoney()
    {
        if (moneyText == null || GameManager.Instance == null)
            return;

        moneyText.text = "$ " + GameManager.Instance.money;
    }

    public void Configure(TextMeshProUGUI textReference)
    {
        moneyText = textReference;
        RefreshMoney();
    }

    void EnsureTextReference()
    {
        if (moneyText != null)
            return;

        moneyText = GetComponent<TextMeshProUGUI>();
        if (moneyText == null)
            moneyText = GetComponentInChildren<TextMeshProUGUI>(true);
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
