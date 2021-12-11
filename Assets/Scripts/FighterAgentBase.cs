using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using System;

public class FighterAgentBase : Agent
{
    [HideInInspector]
    public ObservationController ObsCtrl;

    protected List<float> m_Observations;
    protected Transform m_Root;
    
    // 21 joints, 3 DOF each.
    protected const int c_NumActions = 63;
    protected float[] m_PrevActions;
    // Interpolated.
    protected float[] m_Actions;

    protected int m_LoopStep;
    protected int m_EpisodeStep;
    protected int m_DecisionInterval;
    protected bool m_IsDecisionStep;
    protected bool m_IsPreDecisionStep;


    public override void Initialize()
    {
        m_Root = transform.Find("mixamorig:Hips");

        ObsCtrl = GetComponent<ObservationController>();
        ObsCtrl.Initialize();
        m_Observations = new List<float>();

        m_PrevActions = new float[c_NumActions];
        m_Actions = new float[c_NumActions];
    }

    public void SetDecisionInterval(int interval)
    {
        m_DecisionInterval = interval;
    }

    public virtual void ManagedReset()
    {
        ObsCtrl.ManagedReset();
        Array.Clear(m_PrevActions, 0, c_NumActions);
        Array.Clear(m_Actions, 0, c_NumActions);
    }

    // Invoked at fixed update before any other method calls.
    // EpisodeStep is zero-based and increments after all agent
    // loop methods have been invoked by AgentController.
    // We don't use Agent's StepCount, which increments on
    // Academy.AgentIncrementStep.
    public virtual void OnEpisodeStep(int episodeStep)
    {
        m_EpisodeStep = episodeStep;
        m_IsDecisionStep = episodeStep % m_DecisionInterval == 0;
        // m_LoopStep starts with 1, used for interpolating
        // actions, t = m_LoopStepCount / DecisionInterval.
        m_LoopStep = ++episodeStep % m_DecisionInterval;
        m_IsPreDecisionStep = m_LoopStep == 0;
    }

    // Invoked before RequestDecision call.
    // We need to update all trackers on both agents 
    // prior to vector observations being collected.
    public virtual void PrepareRequestDecision(float deltaTime)
    {
        ObsCtrl.ManagedUpdate(deltaTime);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        m_Observations.Clear();
        ObsCtrl.CollectObservations(m_Observations);
        sensor.AddObservation(m_Observations);
        // Root inclination.
        sensor.AddObservation(m_Root.right.y); 
        sensor.AddObservation(m_Root.up.y);
        sensor.AddObservation(m_Root.forward.y);
    }

    public override void Heuristic(in ActionBuffers actionsOut) { }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        m_Actions = actionBuffers.ContinuousActions.Array;

        if (m_IsPreDecisionStep)
        {
            Array.Copy(m_Actions, m_PrevActions, c_NumActions);
        }
        else
        {
            // Interpolate from previous to current actions.
            float t = m_LoopStep / (float)m_DecisionInterval;

            for (int i = 0; i < c_NumActions; i++)
            {
                m_Actions[i] = Mathf.Lerp(m_PrevActions[i], m_Actions[i], t);
            }
        }
    }

    // Invoked before EndEpisode call.
    public virtual void PrepareEndEpisode() { }
}