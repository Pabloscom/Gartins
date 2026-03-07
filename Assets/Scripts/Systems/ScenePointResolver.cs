using UnityEngine;

public static class ScenePointResolver
{
    public static Transform FindTransform(params string[] names)
    {
        if (names == null)
            return null;

        for (int i = 0; i < names.Length; i++)
        {
            string pointName = names[i];
            if (string.IsNullOrWhiteSpace(pointName))
                continue;

            GameObject pointObject = GameObject.Find(pointName);
            if (pointObject != null)
                return pointObject.transform;
        }

        return null;
    }
}
