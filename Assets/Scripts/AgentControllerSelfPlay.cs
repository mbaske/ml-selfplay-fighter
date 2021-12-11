using UnityEngine;
using Unity.MLAgents;

// Asymmetric self-play.

public class AgentControllerSelfPlay : AgentController
{
    private FighterAgentTrainSelfPlay m_AgentA;
    private FighterAgentTrainSelfPlay m_AgentB;
    private StatsRecorder m_Stats;

    private float m_VarEpisodeLength;
    [SerializeField, Tooltip("Seconds")]
    private float m_MinEpisodeLength = 10;
    [SerializeField, Tooltip("Seconds")]
    private float m_MaxEpisodeLength = 60;
    [SerializeField, Tooltip("Seconds")]
    private float m_EpisodeLengthChange = 0.025f;


    protected override void Initialize()
    {
        base.Initialize();

        m_Stats = Academy.Instance.StatsRecorder;

        m_VarEpisodeLength = m_EpisodeLength;
        UpdateEpisodeLength(0);

        m_AgentA = m_Agents[0].GetComponent<FighterAgentTrainSelfPlay>();
        m_AgentB = m_Agents[1].GetComponent<FighterAgentTrainSelfPlay>();
    }

    protected override void EndEpisode()
    {
        float d = m_AgentA.CmlPunchVelocity - m_AgentB.CmlPunchVelocity;

        if (!m_CancelEpisode)
        {
            if (d > 0)
            {
                m_AgentA.SetReward(1);
                m_AgentB.SetReward(0.5f);
                //m_AgentA.SetReward(m_AgentA.CmlPunchVelocity * 0.2f);
                //m_AgentB.SetReward(m_AgentA.CmlPunchVelocity * 0.1f);
            }
            else if (d < 0)
            {
                m_AgentB.SetReward(1);
                m_AgentA.SetReward(0.5f);
                //m_AgentB.SetReward(m_AgentA.CmlPunchVelocity * 0.2f);
                //m_AgentA.SetReward(m_AgentA.CmlPunchVelocity * 0.1f);
            }
            // else: no punches.
        }

        m_Stats.Add("Agent/Punch Difference", Mathf.Abs(d));
        m_Stats.Add("Agent/Episode Cancelled", m_CancelEpisode ? 1 : 0);
        m_Stats.Add("Agent/Episode Nominal Length", m_VarEpisodeLength);
        m_Stats.Add("Agent/Episode Actual Length",
            m_EpisodeStepCount / (float)SettingsProvider.FPS);

        UpdateEpisodeLength(m_CancelEpisode
            ? -m_EpisodeLengthChange
            : m_EpisodeLengthChange);

        base.EndEpisode();
    }

    protected override void OnBoxingRingCollision(
        string detectorTag, string agentTag)
    {
        if (detectorTag == TagsAndLayers.Ground)
        {
            if (agentTag == TagsAndLayers.AgentA)
            {
                m_AgentA.SetReward(-0.5f);
            }
            else
            {
                m_AgentB.SetReward(-0.5f);
            }

            CancelEpisode();
        }
    }

    private void UpdateEpisodeLength(float change)
    {
        m_VarEpisodeLength = Mathf.Clamp(m_VarEpisodeLength + change,
            m_MinEpisodeLength, m_MaxEpisodeLength);
        // Overrides base class max step.
        m_EpisodeMaxStep = Mathf.RoundToInt(
            m_VarEpisodeLength * SettingsProvider.FPS);
    }
}