using System;
using UnityEngine;

public class BoxingRingCollisionDetector : MonoBehaviour
{
    public event Action<string, string> CollisionEvent;

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag(TagsAndLayers.Foot))
        {
            CollisionEvent?.Invoke(tag,
                collision.transform.GetComponentInParent<JointController>().tag);
        }
    }
}
