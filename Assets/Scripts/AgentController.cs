using UnityEngine;
using Unity.MLAgents;
using System.Collections;

namespace MBaske.SelfPlayFighter
{
    public class AgentController : MonoBehaviour
    {
        [SerializeField]
        private int m_DecisionInterval = 8;
        [SerializeField, Tooltip("Step count including actions.")]
        private int m_EpisodeMaxLength = 4000;
        private int m_StepCount;
        private bool m_IsActive;
        private bool m_SelfPlay;

        [Header("Self-Play")]
        [SerializeField, Tooltip("Initial value, changes during training.")]
        private float m_RequiredCumulativeStrength = 10;

        private SelfPlayRewards m_SelfPlayRewards;
        private FighterAgent[] m_Agents;
        private int m_NumAgents;

        // NOTE from the docs:
        // Switching from the Game view to the Scene view causes WaitForEndOfFrame to freeze. 
        // It only continues when the application switches back to the Game view. 
        // This can only happen when the application is working in the Unity editor.
        private readonly YieldInstruction m_Wait = new WaitForEndOfFrame();

        private void Start()
        {
            m_Agents = GetComponentsInChildren<FighterAgent>();
            m_NumAgents = m_Agents.Length;
            m_SelfPlay = m_NumAgents == 2;

            for (int i = 0; i < m_NumAgents; i++)
            {
                m_Agents[i].SelfPlay = m_SelfPlay;
                m_Agents[i].DecisionInterval = m_DecisionInterval;
                m_Agents[i].EndEpisodeEvent += OnAgentEndsEpisode;
            }

            if (m_SelfPlay)
            {
                m_SelfPlayRewards = new SelfPlayRewards(
                    m_Agents, m_RequiredCumulativeStrength);
            }

            StartCoroutine(BeginEpisode());
        }

        private void OnAgentEndsEpisode(FighterAgent agent)
        {
            if (m_IsActive)
            {
                m_SelfPlayRewards?.OnAgentEndsEpisode(agent);
                StartCoroutine(EndEpisode());
            }
        }

        private void OnEpisodeMaxLengthReached()
        {
            m_SelfPlayRewards?.OnEpisodeMaxLengthReached();
            StartCoroutine(EndEpisode());
        }


        private IEnumerator EndEpisode()
        {
            ToggleActive(false);

            // NOTE EndEpisode will invoke an extra CollectObservations call,
            // see Agent.NotifyAgentDone 'Make sure the latest observations are being passed to training.'

            for (int i = 0; i < m_NumAgents; i++)
            {
                m_Agents[i].EndEpisode();
            }

            yield return m_Wait;

            for (int i = 0; i < m_NumAgents; i++)
            {
                m_Agents[i].DestroyRig();
            }

            yield return m_Wait;

            StartCoroutine(BeginEpisode());
        }

        private IEnumerator BeginEpisode()
        {
            for (int i = 0; i < m_NumAgents; i++)
            {
                m_Agents[i].InstantiateRig();
            }

            yield return m_Wait;

            for (int i = 0; i < m_NumAgents; i++)
            {
                m_Agents[i].ResetAgent();
            }

            yield return m_Wait;

            ToggleActive(true);
        }

        private void ToggleActive(bool active)
        {
            if (active != m_IsActive)
            {
                m_IsActive = active;
                m_StepCount = 0;

                if (m_IsActive)
                {
                    Academy.Instance.AgentPreStep += OnAgentPreStep;
                }
                else
                {
                    Academy.Instance.AgentPreStep -= OnAgentPreStep;
                }
            }
            else
            {
                Debug.LogError("Controller already " + (m_IsActive ? "active" : "incative"));
            }
        }

        private void OnAgentPreStep(int academyStepCount)
        {
            if (m_StepCount % m_DecisionInterval == 0)
            {
                for (int i = 0; i < m_NumAgents; i++)
                {
                    if (m_Agents[i].IsActive)
                    {
                        m_Agents[i].EpisodeStep = m_StepCount;
                        m_Agents[i].RequestDecision();
                    }
                }
            }
            else
            {
                for (int i = 0; i < m_NumAgents; i++)
                {
                    if (m_Agents[i].IsActive)
                    {
                        m_Agents[i].EpisodeStep = m_StepCount;
                        m_Agents[i].RequestAction();
                    }
                }
            }

            if (m_StepCount == m_EpisodeMaxLength)
            {
                OnEpisodeMaxLengthReached();
            }

            m_StepCount++;
        }

        private void OnDestroy()
        {
            if (Academy.IsInitialized)
            {
                Academy.Instance.AgentPreStep -= OnAgentPreStep;
            }

            for (int i = 0; i < m_NumAgents; i++)
            {
                m_Agents[i].EndEpisodeEvent -= OnAgentEndsEpisode;
            }
        }
    }
}