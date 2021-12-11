using UnityEngine;

// Maps animations to physics.
// Used for testing rotation conversions 
// and tweaking joint stiffness/damping.

public class PhysicsTest : MonoBehaviour
{
    [SerializeField]
    private int m_Interval = 6;
    private int m_StepCount;
    private Vector3[] m_Prev;
    private Vector3[] m_Next;

    private enum MappingType
    {
        LocalEulers, SwingTwist, NormalizedSwingTwist
    }
    [SerializeField]
    private MappingType m_Type;
    private JointSettings m_Settings;

    [Space]
    [SerializeField]
    private TrackingController m_AnimatedRig;
    [SerializeField]
    private JointController m_PhysicsRig;
    [SerializeField]
    private Transform m_BoxingRing;


    private void Awake()
    {
        m_Settings = FindObjectOfType<SettingsProvider>().JointSettings;

        m_AnimatedRig.Initialize();
        m_PhysicsRig.Initialize();

        int n = m_AnimatedRig.Trackers.Length;
        m_Prev = new Vector3[n];
        m_Next = new Vector3[n];

        var detectors = m_BoxingRing.GetComponentsInChildren
            <BoxingRingCollisionDetector>();
        foreach (var detector in detectors)
        {
            detector.CollisionEvent += OnAgentDown;
        }
    }

    private void OnAgentDown(string detectorTag, string agentTag)
    {
        m_PhysicsRig.ManagedReset();
    }

    private void FixedUpdate()
    {
        if (m_Settings.GetIsDirty())
        {
            m_PhysicsRig.ApplySpringSettings();
        }

        m_AnimatedRig.ManagedUpdate(Time.fixedDeltaTime);
        
        var settings = m_Settings.Joints;
        var trackers = m_AnimatedRig.Trackers;
        int step = m_StepCount % m_Interval;

        if (step == 0)
        {
            for (int i = 0; i < trackers.Length; i++)
            {
                m_Prev[i] = m_Next[i];
                ApplyToJoint(i, m_Prev[i]);

                var q = trackers[i].LocalRotation;
                var s = settings[i];

                switch (m_Type)
                {
                    case MappingType.LocalEulers:
                        m_Next[i] = JointUtil.WrapEulers(q.eulerAngles);
                        break;

                    case MappingType.SwingTwist:
                        m_Next[i] = JointUtil.RotationToSwingTwist(s, q);
                        break;

                    case MappingType.NormalizedSwingTwist:
                        m_Next[i] = JointUtil.NormalizeSwingTwist(s,
                            JointUtil.RotationToSwingTwist(s, q));
                        break;
                }
            }
        }
        else
        {
            float t = step / (float)m_Interval;

            for (int i = 0; i < trackers.Length; i++)
            {
                ApplyToJoint(i, Vector3.Lerp(m_Prev[i], m_Next[i], t));
            }
        }

        m_StepCount++;
    }

    private void ApplyToJoint(int jointIndex, Vector3 eulers)
    {
        switch (m_Type)
        {
            case MappingType.LocalEulers:
                m_PhysicsRig.ApplyLocalEulers(jointIndex, eulers);
                break;

            case MappingType.SwingTwist:
                m_PhysicsRig.ApplySwingTwist(jointIndex, eulers);
                break;

            case MappingType.NormalizedSwingTwist:
                m_PhysicsRig.ApplyNormalizedSwingTwist(jointIndex, eulers);
                break;
        }
    }
}
