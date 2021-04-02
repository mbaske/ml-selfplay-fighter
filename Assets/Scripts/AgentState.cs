using UnityEngine;

namespace MBaske.SelfPlayFighter
{
    public enum EndEpisodeReason
    {
        RequiredCumulativeStrengthExceeded,
        InvalidPoseTimeout,
        AgentIdleTimeout,
        AgentDown
    }

    public class AgentState
    {
        public bool EndEpisode { get; private set; }
        public EndEpisodeReason EndEpisodeReason { get; private set; }


        // Cumulative contact strength required for winning a round.
        public float RequiredCumulativeStrength { get; set; }
        public float NormalizedCumulativeStrength { get; private set; }
        private float m_CumulativeStrength;

        // Dropping below min height deactivates agent immediately.
        // Assuming ground y = 0.
        private const float c_MinBodyRootHeight = 0.7f;

        // Invalid pose counters:
        // The maximum number of consecutive decision steps the agent is 
        // allowed to stick its hands or feet out beyond the given max distances.
        private const float c_MaxHandToHandDistance = 0.75f;
        private const float c_MaxFootToFootDistance = 0.6f;
        private const int c_InvalidHandDistanceMaxSteps = 15;
        private const int c_InvalidFootDistanceMaxSteps = 15;
        private int m_InvalidHandDistanceStepCount;
        private int m_InvalidFootDistanceStepCount;

        // The maximum number of consecutive decision steps 
        // the agent is allowed to NOT touch its opponent.
        private const int c_IdleMaxSteps = 30;
        private int m_IdleStepCount;

        public void Reset()
        {
            EndEpisode = false;
            m_CumulativeStrength = 0;
            NormalizedCumulativeStrength = -1;
            m_InvalidHandDistanceStepCount = 0;
            m_InvalidFootDistanceStepCount = 0;
            ResetIdleCount();
        }

        public void ResetIdleCount()
        {
            m_IdleStepCount = c_IdleMaxSteps;
        }

        public void AddContactStrength(float strength)
        {
            ResetIdleCount();

            m_CumulativeStrength += strength;
            NormalizedCumulativeStrength = Mathf.Min(1, 
                m_CumulativeStrength / RequiredCumulativeStrength * 2 - 1);
      
            if (m_CumulativeStrength > RequiredCumulativeStrength)
            {
                EndEpisodeReason = EndEpisodeReason.RequiredCumulativeStrengthExceeded;
                EndEpisode = true;
            }
        }

        public bool ValidateIdleCount()
        {
            if (--m_IdleStepCount == 0)
            {
                EndEpisodeReason = EndEpisodeReason.AgentIdleTimeout;
                EndEpisode = true;
                return false;
            }

            return true;
        }

        public bool ValididateBodyHeight(float height)
        {
            bool isValid = height >= c_MinBodyRootHeight;
            if (!isValid)
            {
                EndEpisodeReason = EndEpisodeReason.AgentDown;
                EndEpisode = true;
            }

            return isValid;
        }

        public bool ValidateHandToHandDistance(float distance)
        {
            if (distance > c_MaxHandToHandDistance)
            {
                if (++m_InvalidHandDistanceStepCount > c_InvalidHandDistanceMaxSteps)
                {
                    EndEpisodeReason = EndEpisodeReason.InvalidPoseTimeout;
                    EndEpisode = true;
                    return false;
                }
            }

            m_InvalidHandDistanceStepCount = 0;
            return true;
        }

        public float GetHandDistanceExcess(float distance) 
            => Mathf.Max(0, distance - c_MaxHandToHandDistance);


        public bool ValidateFootToFootDistance(float distance)
        {
            if (distance > c_MaxFootToFootDistance)
            {
                if (++m_InvalidFootDistanceStepCount > c_InvalidFootDistanceMaxSteps)
                {
                    EndEpisodeReason = EndEpisodeReason.InvalidPoseTimeout;
                    EndEpisode = true;
                    return false;
                }
            }

            m_InvalidFootDistanceStepCount = 0;
            return true;
        }

        public float GetFootDistanceExcess(float distance)
            => Mathf.Max(0, distance - c_MaxFootToFootDistance);

    }
}