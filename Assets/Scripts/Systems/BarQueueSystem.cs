using System.Collections.Generic;
using UnityEngine;

public class BarQueueSystem : MonoBehaviour
{
    public Transform servicePoint;
    public Vector2 queueDirection = Vector2.down;
    public float slotSpacing = 0.75f;
    public int maxQueueSize = 0;

    private readonly List<GuestMovement> queue = new List<GuestMovement>();

    public bool TryJoinQueue(GuestMovement guest, out Vector2 queuePosition)
    {
        queuePosition = Vector2.zero;

        if (guest == null)
            return false;

        if (CleanupQueue())
            PushQueueTargets();

        int existingIndex = queue.IndexOf(guest);
        if (existingIndex >= 0)
        {
            queuePosition = GetQueuePosition(existingIndex, guest);
            return true;
        }

        if (maxQueueSize > 0 && queue.Count >= maxQueueSize)
            return false;

        if (servicePoint == null && guest.barPoint != null)
            servicePoint = guest.barPoint;

        queue.Add(guest);
        PushQueueTargets();
        queuePosition = GetQueuePosition(queue.Count - 1, guest);
        return true;
    }

    public void LeaveQueue(GuestMovement guest)
    {
        if (guest == null)
            return;

        int index = queue.IndexOf(guest);
        if (index < 0)
        {
            if (CleanupQueue())
                PushQueueTargets();

            return;
        }

        queue.RemoveAt(index);
        CleanupQueue();
        PushQueueTargets();
    }

    public bool IsFirst(GuestMovement guest)
    {
        if (guest == null)
            return false;

        if (CleanupQueue())
            PushQueueTargets();

        return queue.Count > 0 && queue[0] == guest;
    }

    bool CleanupQueue()
    {
        bool changed = false;

        for (int i = queue.Count - 1; i >= 0; i--)
        {
            if (queue[i] != null)
                continue;

            queue.RemoveAt(i);
            changed = true;
        }

        return changed;
    }

    void PushQueueTargets()
    {
        for (int i = 0; i < queue.Count; i++)
        {
            GuestMovement guest = queue[i];
            if (guest == null)
                continue;

            guest.UpdateQueuePosition(GetQueuePosition(i, guest));
        }
    }

    Vector2 GetQueuePosition(int index, GuestMovement guest)
    {
        Vector2 origin = GetServicePosition(guest);
        Vector2 direction = queueDirection.sqrMagnitude > 0.0001f ? queueDirection.normalized : Vector2.down;
        float safeSpacing = Mathf.Max(0.25f, slotSpacing);

        return origin + direction * safeSpacing * index;
    }

    Vector2 GetServicePosition(GuestMovement guest)
    {
        if (servicePoint != null)
            return servicePoint.position;

        if (guest != null && guest.barPoint != null)
            return guest.barPoint.position;

        return transform.position;
    }
}
