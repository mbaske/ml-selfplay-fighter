using UnityEngine;
using System.Collections.Generic;

public class TrackingController : MonoBehaviour
{
    [HideInInspector]
    public TransformTracker[] Trackers;

    protected JointSettings m_Settings;

    // Initialize, Reset and Update methods are invoked via
    // ObservationController, who utilizes the tracking controller.

    public void Initialize()
    {
        m_Settings = FindObjectOfType<SettingsProvider>().JointSettings;
        var tfList = transform.GetComponentsInChildren<Transform>();

        int n = m_Settings.Joints.Length;
        Trackers = new TransformTracker[n];

        for (int i = 0; i < n; i++)
        {
            string name = m_Settings.Joints[i].Name;

            foreach (var tf in tfList)
            {
                if (tf.name.Contains(name))
                {
                    Trackers[i] = tf.GetComponent<TransformTracker>();
                    break;
                }
            }
        }
    }

    public void ManagedReset()
    {
        for (int i = 0; i < Trackers.Length; i++)
        {
            Trackers[i].ManagedReset();
        }
    }

    public void ManagedUpdate(float deltaTime)
    {
        for (int i = 0; i < Trackers.Length; i++)
        {
            Trackers[i].ManagedUpdate(deltaTime);
        }
    }


    // Get/Set all rotations, used for demo recorder.

    public void GetRotations(List<float> list)
    {
        list.Clear();

        for (int i = 0; i < Trackers.Length; i++)
        {
            var settings = m_Settings.Joints[i];
            Vector3 eulers = JointUtil.RotationToSwingTwist(
                settings, Trackers[i].LocalRotation);
            eulers = JointUtil.NormalizeSwingTwist(settings, eulers);
            list.Add(eulers.x);
            list.Add(eulers.y);
            list.Add(eulers.z);
        }
    }

    public void SetRotations(float[] list)
    {
        var settings = m_Settings.Joints;

        for (int i = 0, j = 0; i < Trackers.Length; i++)
        {
            Vector3 eulers = new Vector3(list[j++], list[j++], list[j++]);
            eulers = JointUtil.DeNormalizeSwingTwist(settings[i], eulers);
            Trackers[i].transform.localRotation
                = JointUtil.SwingTwistToRotation(settings[i], eulers);
        }
    }
}
