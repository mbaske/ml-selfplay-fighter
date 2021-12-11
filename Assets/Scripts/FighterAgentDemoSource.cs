using UnityEngine;
using System.Collections.Generic;

public class FighterAgentDemoSource : FighterAgentAnimated
{
    [HideInInspector]
    public List<float> JointRotations;
    public Pose HipsPose => m_HipsPoses.Peek();

    private Queue<Pose> m_HipsPoses;


    public override void Initialize()
    {
        base.Initialize();

        m_HipsPoses = new Queue<Pose>(m_DecisionInterval);
        JointRotations = new List<float>(c_NumActions);
    }

    public override void ManagedReset()
    {
        base.ManagedReset();

        m_HipsPoses.Clear();
    }

    public override void OnEpisodeStep(int episodeStep)
    {
        base.OnEpisodeStep(episodeStep);

        if (m_HipsPoses.Count == m_DecisionInterval)
        {
            m_HipsPoses.Dequeue();
        }

        m_HipsPoses.Enqueue(new Pose(
            m_Root.localPosition, m_Root.localRotation));
    }

    public override void PrepareRequestDecision(float deltaTime)
    {
        base.PrepareRequestDecision(deltaTime);

        ObsCtrl.Tracking.GetRotations(JointRotations);
    }
}
