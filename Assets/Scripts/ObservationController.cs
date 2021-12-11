using UnityEngine;
using System.Collections.Generic;

public struct OpponentRelativeMetrics
{
    // Distance between agents' hips.
    public float Distance;
    // Angle between agent forward and a line connecting agents' hips.
    public float FacingAngle;
    // Distances between agent's local hands/feet positions and opponent's
    // local hands/feet positions. Used for mimicking animated pose.
    public float LeftFootOffset;
    public float RightFootOffset;
    public float LeftHandOffset;
    public float RightHandffset;
}

[RequireComponent(typeof(AgentReferenceFrame))]
[RequireComponent(typeof(TrackingController))]
public class ObservationController : MonoBehaviour
{
    [HideInInspector]
    public AgentReferenceFrame Frame;
    [HideInInspector]
    public TrackingController Tracking;

    [SerializeField]
    private ObservationController m_Opponent;
    private JointSettings m_Settings;
    private Hand[] m_Hands;
    private Foot[] m_Feet;

    [SerializeField]
    private bool m_DrawGizmos;
    private OpponentRelativeMetrics m_ORM;


    public void Initialize()
    {
        m_Settings = FindObjectOfType<SettingsProvider>().JointSettings;
        m_Hands = GetComponentsInChildren<Hand>();
        m_Feet = GetComponentsInChildren<Foot>();

        Frame = GetComponent<AgentReferenceFrame>();
        Frame.Initialize();

        Tracking = GetComponent<TrackingController>();
        Tracking.Initialize();
    }

    public void ManagedReset()
    {
        Frame.ManagedReset();
        Tracking.ManagedReset();
    }

    public void ManagedUpdate(float deltaTime)
    {
        Frame.ManagedUpdate(deltaTime);
        Tracking.ManagedUpdate(deltaTime);
    }


    public float GetOpponentDistance()
    {
        return (m_Opponent.Frame.Position - Frame.Position).magnitude;
    }

    public OpponentRelativeMetrics GetOpponentRelativeMetrics()
    {
        Vector3 delta = Frame.LocalizeVector(
            m_Opponent.Frame.Position - Frame.Position);
        m_ORM.Distance = delta.magnitude;
        m_ORM.FacingAngle = Vector3.SignedAngle(
            Vector3.forward, delta, Vector3.up);

        var pos = m_Opponent.GetLocalFootPositions();
        m_ORM.LeftFootOffset = (pos.Item1
            - Frame.LocalizePoint(m_Feet[0].Position)).magnitude;
        m_ORM.RightFootOffset = (pos.Item2
            - Frame.LocalizePoint(m_Feet[1].Position)).magnitude;

        pos = m_Opponent.GetLocalHandPositions();
        m_ORM.LeftHandOffset = (pos.Item1
            - Frame.LocalizePoint(m_Hands[0].Position)).magnitude;
        m_ORM.RightHandffset = (pos.Item2
            - Frame.LocalizePoint(m_Hands[1].Position)).magnitude;

        return m_ORM;
    }

    public (Vector3, Vector3) GetLocalFootPositions()
    {
        return (Frame.LocalizePoint(m_Feet[0].Position),
                Frame.LocalizePoint(m_Feet[1].Position));
    }

    public (Vector3, Vector3) GetLocalHandPositions()
    {
        return (Frame.LocalizePoint(m_Hands[0].Position),
                Frame.LocalizePoint(m_Hands[1].Position));
    }



    public void CollectObservations(List<float> list)
    {
        var selfTrackers = Tracking.Trackers;
        var opntTrackers = m_Opponent.Tracking.Trackers;

        for (int i = 0; i < selfTrackers.Length; i++)
        {
            var settings = m_Settings.Joints[i];

            // We're observing swing/twist eulers rather than quaternions,
            // so we can normalize rotations within joint min/max constraints.
            // The normalized eulers are also equivalent to agent actions.

            // Swing/twist rotation.
            Vector3 eulers = JointUtil.RotationToSwingTwist(
                settings, selfTrackers[i].LocalRotation);
            AddObservation(list, JointUtil.NormalizeSwingTwist(
                settings, eulers));
            // Angular velocity.
            AddObservation(list, Normalization.Sigmoid(
                Frame.LocalizeVector(selfTrackers[i].AngularVelocity)));

            if (settings.ObserveMotion)
            {
                // Self.
                var tracker = selfTrackers[i];
                Vector3 pos = Frame.LocalizePoint(tracker.Position);
                // Normalize local pos within bounds defined in settings.
                AddObservation(list, GetNormalizedPosition(
                    pos, settings.SelfBounds));
                // Velocity.
                AddObservation(list, Normalization.Sigmoid(
                    Frame.LocalizeVector(tracker.Velocity)));

                // Opponent.
                tracker = opntTrackers[i];
                pos = Frame.LocalizePoint(tracker.Position);
                // Normalize local pos within bounds defined in settings.
                AddObservation(list, GetNormalizedPosition(
                    pos, settings.OpponentBounds));
                // Velocity.
                AddObservation(list, Normalization.Sigmoid(
                    Frame.LocalizeVector(tracker.Velocity)));
            }
        }

        // Reference frame observations, self.
        AddObservation(list, Frame.NormPos);
        AddObservation(list, Normalization.Sigmoid(
            Frame.LocalizeVector(Frame.Velocity)));
        AddObservation(list, Normalization.Sigmoid(
            Frame.AngularVelocity)); // already local

        // Reference frame observations, opponent.
        var frame = m_Opponent.Frame;
        AddObservation(list, frame.NormPos);
        AddObservation(list, Normalization.Sigmoid(
            Frame.LocalizeVector(frame.Velocity)));
        AddObservation(list, Normalization.Sigmoid(
            frame.AngularVelocity)); // already local
    }

    private static Vector3 GetNormalizedPosition(Vector3 pos, Bounds bounds)
    {
        Vector3 ext = bounds.extents;
        pos -= bounds.center;
        pos.x = Mathf.Clamp(pos.x / ext.x, -1, 1);
        pos.y = Mathf.Clamp(pos.y / ext.y, -1, 1);
        pos.z = Mathf.Clamp(pos.z / ext.z, -1, 1);

        return pos;
    }

    private static void AddObservation(List<float> list, float value)
    {
        // TODO always prevent div by 0.
        list.Add(float.IsNaN(value) ? 0 : value);
    }

    private static void AddObservation(List<float> list, Vector3 value)
    {
        AddObservation(list, value.x);
        AddObservation(list, value.y);
        AddObservation(list, value.z);
    }


    private void OnDrawGizmos()
    {
        if (Application.isPlaying && m_DrawGizmos)
        {
            var selfTrackers = Tracking.Trackers;
            var opntTrackers = m_Opponent.Tracking.Trackers;

            for (int i = 0; i < selfTrackers.Length; i++)
            {
                var settings = m_Settings.Joints[i];

                if (settings.DrawGizmos)
                {
                    Bounds selfBounds = settings.SelfBounds;
                    Bounds opntBounds = settings.OpponentBounds;

                    Vector3 selfPos = selfTrackers[i].Position;
                    Vector3 opntPos = opntTrackers[i].Position;

                    Gizmos.matrix = Matrix4x4.identity;
                    Gizmos.color = Color.white * (selfBounds.Contains(
                        Frame.LocalizePoint(selfPos)) ? 1 : 0.7f);
                    Gizmos.DrawWireSphere(selfPos, 0.15f);
                    Gizmos.color = Color.yellow * (opntBounds.Contains(
                        Frame.LocalizePoint(opntPos)) ? 1 : 0.7f);
                    Gizmos.DrawWireSphere(opntPos, 0.15f);

                    Gizmos.matrix = Frame.Matrix;
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireCube(selfBounds.center, selfBounds.size);
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireCube(opntBounds.center, opntBounds.size);
                }
            }
        }
    }
}
