using UnityEngine;

[RequireComponent(typeof(ArticulationBodyResetter))]
public class JointController : MonoBehaviour
{
    private JointSettings m_Settings;
    private ArticulationBodyResetter m_ABResetter;

    private SphericalJoint[] m_Joints;


    public void Initialize()
    {
        m_Settings = FindObjectOfType<SettingsProvider>().JointSettings;

        m_ABResetter = GetComponent<ArticulationBodyResetter>();
        m_ABResetter.Initialize();

        var settings = m_Settings.Joints;
        int n = settings.Length;

        m_Joints = new SphericalJoint[n];

        var tfList = transform.GetComponentsInChildren<Transform>();

        for (int i = 0; i < n; i++)
        {
            string name = m_Settings.Joints[i].Name;

            foreach (var tf in tfList)
            {
                if (tf.name.Contains(name))
                {
                    m_Joints[i] = new SphericalJoint(
                        tf.GetComponent<ArticulationBody>(),
                        JointUtil.RotationToSwingTwist(
                            settings[i], tf.localRotation));
                    break;
                }
            }
        }

        ApplySpringSettings();
    }

    public void ApplySpringSettings()
    {
        for (int i = 0; i < m_Joints.Length; i++)
        {
            m_Joints[i].ApplySpringSettings(m_Settings);
        }
    }

    public void ManagedReset()
    {
        for (int i = 0; i < m_Joints.Length; i++)
        {
            m_Joints[i].ManagedReset();
        }
        m_ABResetter.ManagedReset();
    }

    // Invoked by agent.
    public void ApplyActions(float[] actions)
    {
        for (int i = 0, j = 0; i < m_Joints.Length; i++)
        {
            m_Joints[i].SetDriveTargets(
                JointUtil.NormalizedSwingTwistToDriveTargets(
                    m_Settings.Joints[i],
                    actions[j++],
                    actions[j++],
                    actions[j++]));
        }
    }


    // Invoked by PhysicsTest.

    public void ApplyLocalEulers(int jointIndex, Vector3 eulers)
    {
        eulers = JointUtil.EulersToDriveTargets(eulers);
        m_Joints[jointIndex].SetDriveTargets(eulers);
    }

    public void ApplySwingTwist(int jointIndex, Vector3 eulers)
    {
        eulers = JointUtil.SwingTwistToDriveTargets(eulers);
        m_Joints[jointIndex].SetDriveTargets(eulers);
    }

    public void ApplyNormalizedSwingTwist(int jointIndex, Vector3 eulers)
    {
        eulers = JointUtil.NormalizedSwingTwistToDriveTargets(
                m_Settings.Joints[jointIndex], eulers);
        m_Joints[jointIndex].SetDriveTargets(eulers);
    }
}
