using UnityEngine;
using TMPro;

public class ClockUI : MonoBehaviour
{
    public TimeSystem timeSystem;
    public TextMeshProUGUI clockText;
    private int lastShownHours = -1;
    private int lastShownMinutes = -1;
    private float nextResolveAttemptTime;

    void Update()
    {
        if (clockText == null)
            return;

        if (timeSystem == null && Time.time >= nextResolveAttemptTime)
        {
            timeSystem = FindObjectOfType<TimeSystem>();
            nextResolveAttemptTime = Time.time + 0.5f;
        }

        if (timeSystem == null)
            return;

        float hour = timeSystem.currentHour;
        int hours = Mathf.FloorToInt(hour);
        int minutes = Mathf.FloorToInt((hour - hours) * 60f);

        if (hours == lastShownHours && minutes == lastShownMinutes)
            return;

        lastShownHours = hours;
        lastShownMinutes = minutes;
        clockText.text = string.Format("{0:00}:{1:00}", hours, minutes);
    }
}
