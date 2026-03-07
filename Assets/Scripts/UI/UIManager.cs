using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public RectTransform ambienteFill;
    public RectTransform ambienteBackground;

    private float maxWidth;
    private bool subscribed;
    private Coroutine subscribeRoutine;

    void Start()
    {
        if (ambienteBackground != null)
            maxWidth = ambienteBackground.rect.width;

        TrySubscribe();
        if (!subscribed)
            subscribeRoutine = StartCoroutine(SubscribeWhenGameManagerAvailable());

        RefreshAmbience();
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

        GameManager.Instance.AmbienceChanged += OnAmbienceChanged;
        subscribed = true;
    }

    void Unsubscribe()
    {
        if (!subscribed || GameManager.Instance == null)
            return;

        GameManager.Instance.AmbienceChanged -= OnAmbienceChanged;
        subscribed = false;
    }

    void OnAmbienceChanged(float _)
    {
        RefreshAmbience();
    }

    void RefreshAmbience()
    {
        if (ambienteFill == null || GameManager.Instance == null)
            return;

        float porcentaje = Mathf.Clamp01(GameManager.Instance.ambiente / 100f);
        ambienteFill.sizeDelta = new Vector2(maxWidth * porcentaje, ambienteFill.sizeDelta.y);
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
