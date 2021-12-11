using System.Collections.Generic;
using UnityEngine;

// Updates settings scriptable object.
public class JointConstraintSetter : TrackingController
{
    private void Awake()
    {
        //ResetAllJointSettings();

        Initialize();
        ResetConstraints();
    }

    private void FixedUpdate()
    {
        UpdateConstraints();
    }

    private void OnApplicationQuit()
    {
        FinalizeConstraints();
    }

    private void ResetAllJointSettings()
    {
        var names = new List<string>();
        var tfList = transform.GetComponentsInChildren<Transform>();

        for (int i = 0; i < tfList.Length; i++)
        {
            if (tfList[i].TryGetComponent(out TransformTracker tracker))
            {
                string name = tracker.name;
                names.Add(name.Contains(":") ? name.Split(':')[1] : name);
            }
        }

        if (names.Count > 0)
        {
            m_Settings.Joints = new JointSettings.Joint[names.Count];

            for (int i = 0; i < names.Count; i++)
            {
                m_Settings.Joints[i] = new JointSettings.Joint() { Name = names[i] };
            }
        }
    }

    private void ResetConstraints()
    {
        for (int i = 0; i < Trackers.Length; i++)
        {
            var settings = m_Settings.Joints[i];
            settings.MinSwingTwist = Vector3.positiveInfinity;
            settings.MaxSwingTwist = Vector3.negativeInfinity;
        }
    }

    private void UpdateConstraints()
    {
        float deltaTime = Time.fixedDeltaTime;

        for (int i = 0; i < Trackers.Length; i++)
        {
            var tracker = Trackers[i];
            var settings = m_Settings.Joints[i];

            tracker.ManagedUpdate(deltaTime);
            Vector3 eulers = JointUtil.RotationToSwingTwist(
                settings, tracker.LocalRotation);

            settings.MinSwingTwist = Vector3.Min(settings.MinSwingTwist, eulers);
            settings.MaxSwingTwist = Vector3.Max(settings.MaxSwingTwist, eulers);
        }
    }

    private void FinalizeConstraints()
    {
        for (int i = 0; i < Trackers.Length; i++)
        {
            var settings = m_Settings.Joints[i];
            const float pad = 1;

            Vector3 eulers = settings.MinSwingTwist;
            eulers.x = Mathf.Floor(eulers.x) - pad;
            eulers.y = Mathf.Floor(eulers.y) - pad;
            eulers.z = Mathf.Floor(eulers.z) - pad;
            settings.MinSwingTwist = eulers;

            eulers = settings.MaxSwingTwist;
            eulers.x = Mathf.Ceil(eulers.x) + pad;
            eulers.y = Mathf.Ceil(eulers.y) + pad;
            eulers.z = Mathf.Ceil(eulers.z) + pad;
            settings.MaxSwingTwist = eulers;

            settings.CommitConstraints();
        }
    }
}
