using Unity.MLAgents.Actuators;
using UnityEngine;

public class FighterAgentDemoRecorder: FighterAgentBase
{
    [SerializeField]
    private FighterAgentDemoSource m_Source;

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;
        // Joint target rotations from source
        // agent's previous decision step.
        var targets = m_Source.JointRotations;

        for (int i = 0; i < c_NumActions; i++)
        {
            actions[i] = targets[i];
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        base.OnActionReceived(actionBuffers);

        // Interpolated towards target rotations.
        ObsCtrl.Tracking.SetRotations(m_Actions);

        // Copy pose from {episode step - decision interval}.
        Pose pose = m_Source.HipsPose;
        m_Root.localPosition = pose.position;
        m_Root.localRotation = pose.rotation;
    }
}
 