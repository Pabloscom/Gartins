using UnityEngine;
using System.Collections.Generic;

public class BarQueueSystem : MonoBehaviour
{
    public Transform[] queuePoints;

    private readonly List<GuestMovement> queue = new List<GuestMovement>();

    public int MaxQueue => queuePoints != null ? queuePoints.Length : 0;

    public bool HasSpace()
    {
        CleanupQueue();
        return MaxQueue > 0 && queue.Count < MaxQueue;
    }

    public Vector2 JoinQueue(GuestMovement guest)
    {
        if (TryJoinQueue(guest, out Vector2 queuePosition))
            return queuePosition;

        return Vector2.zero;
    }

    public bool TryJoinQueue(GuestMovement guest, out Vector2 queuePosition)
    {
        queuePosition = Vector2.zero;

        if (guest == null || MaxQueue <= 0)
            return false;

        CleanupQueue();

        int existingIndex = queue.IndexOf(guest);
        if (existingIndex >= 0)
        {
            queuePosition = GetQueuePointPosition(existingIndex);
            return true;
        }

        if (queue.Count >= MaxQueue)
            return false;

        queue.Add(guest);
        UpdateQueuePositions();
        queuePosition = GetQueuePointPosition(queue.Count - 1);

        return true;
    }

    public bool IsFirst(GuestMovement guest)
    {
        CleanupQueue();

        if (guest == null || queue.Count == 0)
            return false;

        return queue[0] == guest;
    }

    public void LeaveQueue(GuestMovement guest)
    {
        if (guest == null)
            return;

        int index = queue.IndexOf(guest);
        if (index < 0)
            return;

        queue.RemoveAt(index);
        UpdateQueuePositions();
    }

    void UpdateQueuePositions()
    {
        CleanupQueue();

        if (queuePoints == null || queuePoints.Length == 0)
            return;

        int max = Mathf.Min(queue.Count, queuePoints.Length);
        for (int i = 0; i < max; i++)
        {
            if (queuePoints[i] != null)
                queue[i].UpdateQueuePosition(queuePoints[i].position);
        }
    }

    void CleanupQueue()
    {
        for (int i = queue.Count - 1; i >= 0; i--)
        {
            if (queue[i] == null)
                queue.RemoveAt(i);
        }
    }

    Vector2 GetQueuePointPosition(int index)
    {
        if (queuePoints == null || queuePoints.Length == 0)
            return Vector2.zero;

        int safeIndex = Mathf.Clamp(index, 0, queuePoints.Length - 1);
        Transform queuePoint = queuePoints[safeIndex];
        return queuePoint != null ? (Vector2)queuePoint.position : Vector2.zero;
    }
}
