using UnityEngine;

public static class Normalization
{
    public static float Sigmoid(float val)
    {
        return Mathf.Clamp(val / (1f + Mathf.Abs(val)), -1f, 1f);
    }

    public static Vector3 Sigmoid(Vector3 v)
    {
        v.x = Sigmoid(v.x);
        v.y = Sigmoid(v.y);
        v.z = Sigmoid(v.z);
        return v;
    }
}
