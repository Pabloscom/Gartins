using UnityEngine;

public class ActionBubble : MonoBehaviour
{
    public float lifetime = 2f;
    public float floatSpeed = 0.5f;

    private SpriteRenderer spriteRenderer;

    private bool autoDestroy = true;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Sprite icon, bool destroyAutomatically = true, float customLifetime = -1f)
    {
        if (spriteRenderer != null)
            spriteRenderer.sprite = icon;

        autoDestroy = destroyAutomatically;

        if (autoDestroy)
        {
            float destroyAfter = customLifetime > 0f ? customLifetime : lifetime;
            Destroy(gameObject, destroyAfter);
        }
    }

    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
    }
}
