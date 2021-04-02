using UnityEngine;
using Unity.MLAgents;

namespace MBaske.SelfPlayFighter
{
    public class SelfPlayRewards
    {
        private readonly FighterAgent[] m_Agents;
        private readonly StatsRecorder m_Stats;
        private float m_RequiredCumulativeStrength;

        public SelfPlayRewards(FighterAgent[] agents, float initialStrength)
        {
            m_Agents = agents;
            m_RequiredCumulativeStrength = initialStrength;
            m_Stats = Academy.Instance.StatsRecorder;
            UpdateRequiredCumulativeStrength();
        }

        public void OnAgentEndsEpisode(FighterAgent agent)
        {
            //Debug.Log(agent.State.EndEpisodeReason);

            switch (agent.State.EndEpisodeReason)
            {
                case EndEpisodeReason.RequiredCumulativeStrengthExceeded:
                    // Agent wins.
                    agent.SetReward(1);
                    agent.Opponent.SetReward(-1);
                    // More cumulative strength needed next round.
                    UpdateRequiredCumulativeStrength(1);
                    break;

                case EndEpisodeReason.InvalidPoseTimeout:
                    // Agent loses.
                    // https://forum.unity.com/threads/elo-decreasing-with-positive-mean-reward.1079672/#post-6990929
                    agent.SetReward(-1);
                    agent.Opponent.SetReward(1);
                    break;

                case EndEpisodeReason.AgentIdleTimeout:
                    // Draw.
                    // Less cumulative strength needed next round.
                    UpdateRequiredCumulativeStrength(-0.1f);
                    break;

                case EndEpisodeReason.AgentDown:
                    // Draw.
                    // We're not rewarding agents here in order to prevent early 
                    // knock-out punches. 
                    // Although that would be a good winning strategy, it doesn't 
                    // make for very interesting episodes. 
                    // Agents would try to end rounds quickly by hitting as hard as 
                    // they can, or by smashing into opponents to bring them down.
                    break;
            }
        }

        public void OnEpisodeMaxLengthReached()
        {
            RewardHarderHittingAgent();
            // Less cumulative strength needed next round.
            UpdateRequiredCumulativeStrength(-1);
        }

        public void RewardHarderHittingAgent()
        {
            if (m_Agents[0].State.NormalizedCumulativeStrength > 
                m_Agents[1].State.NormalizedCumulativeStrength)
            {
                m_Agents[0].AddReward(1);
                m_Agents[1].AddReward(-1);
            }
            else if (m_Agents[0].State.NormalizedCumulativeStrength < 
                m_Agents[1].State.NormalizedCumulativeStrength)
            {
                m_Agents[0].AddReward(-1);
                m_Agents[1].AddReward(1);
            }
            // else: Draw, no rewards.
        }

        private void UpdateRequiredCumulativeStrength(float delta = 0)
        {
            m_RequiredCumulativeStrength = Mathf.Max(1, m_RequiredCumulativeStrength + delta);
            m_Agents[0].State.RequiredCumulativeStrength = m_RequiredCumulativeStrength;
            m_Agents[1].State.RequiredCumulativeStrength = m_RequiredCumulativeStrength;

            m_Stats.Add("Fighter/Req. Strength", m_RequiredCumulativeStrength);
        }
    }
}