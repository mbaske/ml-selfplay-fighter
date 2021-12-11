using UnityEngine;
using System;

[CreateAssetMenu(fileName = "JointSettings", menuName = "Scriptable Objects/Create Joint Settings", order = 1)]
public class JointSettings : ScriptableObject
{
    [Serializable]
    public class Joint
    {
        public string Name;
        public TwistAxis TwistAxis;

        public Vector3 MinSwingTwist;
        public Vector3 MaxSwingTwist;
        [HideInInspector]
        public Vector3 Extent;
        [HideInInspector]
        public Vector3 Center;

        public void CommitConstraints()
        {
            Extent = (MaxSwingTwist - MinSwingTwist) * 0.5f;
            Center = MaxSwingTwist - Extent;
        }

        [Space]
        public bool DrawGizmos;
        public bool ObserveMotion;
        public Bounds SelfBounds;
        public Bounds OpponentBounds;
    }

    // Shared by all joint drives.
    public float SpringDamping = 100;
    public float SpringStiffness = 5000;
    public float SpringForceLimit = 100000;

    public Joint[] Joints;


    public bool GetIsDirty()
    {
        bool tmp = m_IsDirty;
        m_IsDirty = false;
        return tmp;
    }

    private bool m_IsDirty;

    private void OnValidate()
    {
        m_IsDirty = true;
    }
}