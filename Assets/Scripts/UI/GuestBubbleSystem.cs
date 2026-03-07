using UnityEngine;

public class GuestBubbleSystem : MonoBehaviour
{
    public GameObject bubblePrefab;

    public Sprite drinkIcon;
    public Sprite talkIcon;
    public Sprite danceIcon;
    public float socialBubbleLifetime = 2f;

    public Transform bubblePoint;

    private GameObject currentBubble;

    public void ShowBubble(Sprite icon, bool autoDestroy = true, float customLifetime = -1f)
    {
        if (bubblePrefab == null)
            return;

        if (currentBubble != null)
            Destroy(currentBubble);

        Vector3 spawnPosition = bubblePoint != null ? bubblePoint.position : transform.position;
        currentBubble = Instantiate(bubblePrefab, spawnPosition, Quaternion.identity);
        currentBubble.SetActive(true);

        ActionBubble bubbleScript = currentBubble.GetComponent<ActionBubble>();
        if (bubbleScript != null)
            bubbleScript.Initialize(icon, autoDestroy, customLifetime);
    }

    public void Drink()
    {
        ShowBubble(drinkIcon, false); // burbuja permanente
    }

    public void Talk()
    {
        ShowBubble(talkIcon, true, socialBubbleLifetime);
    }

    public void Dance()
    {
        ShowBubble(danceIcon, true, socialBubbleLifetime);
    }

    public void ClearBubble()
    {
        if (currentBubble != null)
            Destroy(currentBubble);

        currentBubble = null;
    }
}
