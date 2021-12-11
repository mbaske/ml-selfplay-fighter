using UnityEngine;
using Unity.MLAgents;
using System.Collections.Generic;

// Controls episodes for a variable number of agents.
// Cancels an episode on boxing ring collision (ground and ropes)
// with any body parts other than feet.

public class AgentController : MonoBehaviour
{
    protected List<FighterAgentBase> m_Agents;
    protected Transform m_BoxingRing;

    protected enum RotationMode
    {
        None, Random, Step
    }
    [SerializeField]
    protected RotationMode m_RotationMode;
    // Random rotations for training.
    protected const float c_RandomRotationExtent = 75;
    // Stepped rotations for demo recorder.
    protected const float c_SteppedRotationIncrement = 40;
    protected float m_SteppedRotationAngle = -100;

    [SerializeField, Tooltip("Seconds")]
    protected int m_EpisodeLength = 30;
    [SerializeField, Tooltip("Steps")]
    protected int m_DecisionInterval = 6;
    
    protected int m_EpisodeMaxStep;
    protected int m_EpisodeStepCount;
    protected bool m_CancelEpisode;
    protected bool m_IsActive;
    protected float m_PrevTime;
    // Wait 1 frame after ManagedReset.
    protected InvokeAfterFrames m_DelayedStart;

    protected void Start()
    {
        Initialize();
    }

    protected virtual void Initialize()
    {
        m_EpisodeMaxStep = m_EpisodeLength * SettingsProvider.FPS;
        m_DelayedStart = new InvokeAfterFrames(this, StartEpisode, 1);

        m_Agents = new List<FighterAgentBase>(GetComponentsInChildren<FighterAgentBase>());
        m_Agents.ForEach(agent => agent.SetDecisionInterval(m_DecisionInterval));
        m_BoxingRing = transform.Find("BoxingRing");

        var detectors = GetComponentsInChildren<BoxingRingCollisionDetector>();
        foreach (var detector in detectors)
        {
            detector.CollisionEvent += OnBoxingRingCollision;
        }

        UpdateRotation();
        m_DelayedStart.Invoke();
    }

    protected void SetActive(bool active)
    {
        m_IsActive = active;

        if (active)
        {
            Academy.Instance.AgentPreStep += OnAgentPreStep;
        }
        else
        {
            Academy.Instance.AgentPreStep -= OnAgentPreStep;
        }
    }

    protected virtual void StartEpisode()
    {
        SetActive(true);

        m_CancelEpisode = false;
        m_EpisodeStepCount = 0;
        m_PrevTime = Time.time - Time.fixedDeltaTime * m_DecisionInterval;
    }

    protected virtual void EndEpisode()
    {
        SetActive(false);

        if (UpdateRotation())
        {
            m_Agents.ForEach(agent => agent.PrepareEndEpisode());
            m_Agents.ForEach(agent => agent.EndEpisode());
            m_Agents.ForEach(agent => agent.ManagedReset());

            m_DelayedStart.Invoke();
        }
#if UNITY_EDITOR
        else
        {
            // Stop recording demo after last rotation step.
            UnityEditor.EditorApplication.isPlaying = false;
        }
#endif
    }

    protected virtual void OnAgentPreStep(int academyStepCount)
    {
        m_Agents.ForEach(agent => agent.OnEpisodeStep(m_EpisodeStepCount));

        if (m_EpisodeStepCount % m_DecisionInterval == 0)
        {
            float deltaTime = Time.time - m_PrevTime;
            m_PrevTime = Time.time;

            m_Agents.ForEach(agent => agent.PrepareRequestDecision(deltaTime));
            m_Agents.ForEach(agent => agent.RequestDecision());
        }
        else
        {
            m_Agents.ForEach(agent => agent.RequestAction());
        }

        if (m_CancelEpisode || ++m_EpisodeStepCount == m_EpisodeMaxStep)
        {
            EndEpisode();
        }
    }

    protected virtual void OnBoxingRingCollision(
        string detectorTag, string agentTag)
    {
        CancelEpisode();
    }

    protected void CancelEpisode()
    {
        m_CancelEpisode = true;
    }

    protected void OnApplicationQuit()
    {
        SetActive(false);
    }

    // Returns false only if mode = stepped and no  
    // more rotation steps are available (demo recorder).
    protected bool UpdateRotation()
    {
        if (m_RotationMode == RotationMode.None)
        {
            return true;
        }

        bool step = m_RotationMode == RotationMode.Step;
        float angle = step
            ? m_SteppedRotationAngle += c_SteppedRotationIncrement
            : Random.Range(-c_RandomRotationExtent, c_RandomRotationExtent);

        var rot = Quaternion.AngleAxis(angle, Vector3.up);
        m_BoxingRing.localRotation = Quaternion.Inverse(rot);
        transform.localRotation = rot;

        return !(step && angle > 90);
    }
}