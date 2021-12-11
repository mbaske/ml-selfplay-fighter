using UnityEngine;
using System;

public class Hand : TransformTracker
{
    public event Action<Collision, Hand> PunchEvent;
    public event Action<Collision, Hand> BlockEvent;

    private string m_TargetTag;
    private string m_BlockTag;


    private void Awake()
    {
        var cmp = GetComponentInParent<TagsAndLayers>();

        if (cmp != null)
        {
            m_BlockTag = cmp.BlockTag;
            m_TargetTag = cmp.TargetTag;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag(m_TargetTag))
        {
            PunchEvent?.Invoke(collision, this);
        }
        else if (collision.collider.CompareTag(m_BlockTag))
        {
            BlockEvent?.Invoke(collision, this);
        }
    }
}
