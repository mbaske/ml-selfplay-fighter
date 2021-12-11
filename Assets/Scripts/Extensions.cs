using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    // https://answers.unity.com/questions/799429/transformfindstring-no-longer-finds-grandchild.html
    public static Transform DeepFind(this Transform parent, string name)
    {
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(parent);

        while (queue.Count > 0)
        {
            var c = queue.Dequeue();

            if (c.name == name)
            {
                return c;
            }

            foreach (Transform t in c)
            {
                queue.Enqueue(t);
            }
        }

        return null;
    }
}
