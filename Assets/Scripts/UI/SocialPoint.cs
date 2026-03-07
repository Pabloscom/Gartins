using UnityEngine;

public class SocialPoint : MonoBehaviour
{
    public int maxUsers = 3;
    public float radius = 1.5f;

    private int currentUsers = 0;

    public bool HasSpace()
    {
        return currentUsers < maxUsers;
    }

    public void Enter()
    {
        currentUsers++;
    }

    public void Leave()
    {
        currentUsers = Mathf.Max(0, currentUsers - 1);
    }

    public Vector2 GetRandomPosition()
    {
        Vector2 randomOffset = Random.insideUnitCircle * radius;
        return (Vector2)transform.position + randomOffset;
    }
}