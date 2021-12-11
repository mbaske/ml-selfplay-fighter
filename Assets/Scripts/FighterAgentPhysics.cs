using Unity.MLAgents.Actuators;
using Unity.MLAgents;
using UnityEngine;

public class FighterAgentPhysics : FighterAgentBase
{
    protected StatsRecorder m_Stats;
    protected JointController m_JointController;
    private ArticulationBodyResetter m_ABResetter;

    public override void Initialize()
    {
        base.Initialize();

        m_Stats = Academy.Instance.StatsRecorder;

        m_JointController = GetComponent<JointController>();
        m_JointController.Initialize();
        m_ABResetter = GetComponent<ArticulationBodyResetter>();
        m_ABResetter.Initialize();
    }

    public override void ManagedReset()
    {
        base.ManagedReset();

        m_JointController.ManagedReset();
        m_ABResetter.ManagedReset();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        base.OnActionReceived(actionBuffers);

        m_JointController.ApplyActions(m_Actions);
    }
}
