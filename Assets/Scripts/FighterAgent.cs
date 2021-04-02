using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Random = UnityEngine.Random;
using System;

namespace MBaske.SelfPlayFighter
{
    public class FighterAgent : Agent
    {
        public bool IsActive { get; set; }

        // Self-Play.
        public bool SelfPlay { get; set; }
        private int m_TeamID;
        [Header("Self-Play")]
        [SerializeField, Tooltip("Add a small reward to encourange " +
            "agent-to-agent proximity and alignment.")]
        private float m_ProximityRewardFactor = 0.01f;
        [SerializeField, Tooltip("Helps agent with balancing itself.")]
        private float m_StablizationStrength = 500;

        [Space]
        [Tooltip("Always required for providing observations, " +
            "even if we don't train with self-play initially.")]
        public FighterAgent Opponent;
        // Stack observed positions to infer motion.
        private Vector3[] m_OpponentJointPositionBuffer;

        // State.
        public AgentState State { get; private set; }
        public event Action<FighterAgent> EndEpisodeEvent;

        // Decisions & Actions.
        public int DecisionInterval { get; set; }
        public int EpisodeStep { get; set; }
        // Interpolate actions between decision steps.
        private float[] m_ActionBuffer;


        public BodyRoot BodyRoot { get; private set; }

        private Hand[] m_Hands;
        private Foot[] m_Feet;
        // Bodyparts that register hits.
        private Target[] m_Targets;

        private BodyJoint[] m_Joints;
        // Stack observed rotations to infer motion.
        private Quaternion[] m_JointRotationBuffer;

        [Header("Balancing")]
        [SerializeField, Tooltip("Kick agent occasionally to improve robustness.")]
        private bool m_AddRandomForce;
        private const float c_RandomForceRatio = 0.00002f;

        [Space, SerializeField]
        private GameObject m_RigPrefab;
        private bool m_HasRig;

        private const int c_NumJoints = 21;
        private const int c_NumActions = 74;


        public override void Initialize()
        {
            State = new AgentState();
            m_TeamID = GetComponent<BehaviorParameters>().TeamId;

            m_ActionBuffer = new float[c_NumActions];
            m_JointRotationBuffer = new Quaternion[c_NumJoints];
            m_OpponentJointPositionBuffer = new Vector3[c_NumJoints];
        }

        // TODO Couldn't get the ArticulationBody rig to reset properly. Workaround: respawn rig.
        // https://forum.unity.com/threads/featherstones-solver-for-articulations.792294/page-4#post-6220986
        // https://forum.unity.com/threads/featherstones-solver-for-articulations.792294/page-4#post-6225153
        public void InstantiateRig()
        {
            var rig = Instantiate(m_RigPrefab, transform);
            BodyRoot = rig.GetComponentInChildren<BodyRoot>();
            BodyRoot.Initialize();
            BodyRoot.SetSelfPlay(SelfPlay, m_TeamID);
            BodyRoot.SetCenterPosition(transform.parent.position);
            BodyRoot.SetStablizationStrength(m_StablizationStrength);

            m_Feet = rig.GetComponentsInChildren<Foot>();
            m_Hands = rig.GetComponentsInChildren<Hand>();
            m_Joints = rig.GetComponentsInChildren<BodyJoint>();
            m_Targets = rig.GetComponentsInChildren<Target>();

            for (int i = 0; i < c_NumJoints; i++)
            {
                m_Joints[i].Initialize();
                m_Joints[i].SetSelfPlay(SelfPlay, m_TeamID);
            }

            for (int i = 0; i < m_Targets.Length; i++)
            {
                m_Targets[i].Initialize();
                m_Targets[i].SetSelfPlay(SelfPlay, m_TeamID);
            }

            for (int i = 0; i < 2; i++)
            {
                m_Feet[i].Initialize();
                m_Hands[i].Initialize();
                m_Hands[i].SetSelfPlay(SelfPlay, m_TeamID);
                m_Hands[i].TargetContactEvent += State.AddContactStrength;
                m_Hands[i].HandContactEvent += State.ResetIdleCount;
            }

            m_HasRig = true;
        }

        public void DestroyRig()
        {
            if (transform.childCount > 0)
            {
                m_Hands[0].TargetContactEvent -= State.AddContactStrength;
                m_Hands[1].TargetContactEvent -= State.AddContactStrength;
                m_Hands[0].HandContactEvent -= State.ResetIdleCount;
                m_Hands[1].HandContactEvent -= State.ResetIdleCount;
                Destroy(transform.GetChild(0).gameObject);

                m_HasRig = false;
            }
        }

        public bool HasRig(out BodyJoint[] joints)
        {
            joints = m_Joints;
            return m_HasRig;
        }

        public void ResetAgent()
        {
            State.Reset();
            IsActive = true;

            Array.Clear(m_ActionBuffer, 0, c_NumActions);
            Array.Clear(m_JointRotationBuffer, 0, c_NumJoints);
            Array.Clear(m_OpponentJointPositionBuffer, 0, c_NumJoints);
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            BodyRoot.ManagedUpdate();
            
            // Position and orientation in boxing ring.
            sensor.AddObservation(BodyRoot.NormFwdAngle);
            sensor.AddObservation(BodyRoot.NormPosition);

            sensor.AddObservation(BodyRoot.AxisInclination);
            sensor.AddObservation(Sigmoid(BodyRoot.LocalVelocity));
            sensor.AddObservation(Sigmoid(BodyRoot.LocalAngularVelocity));

            // 0 - 2m
            sensor.AddObservation(m_Hands[0].DistanceToGround() - 1);
            sensor.AddObservation(m_Hands[1].DistanceToGround() - 1);
            // 0 - 0.5m
            sensor.AddObservation(Mathf.Min(m_Feet[0].DistanceToGround() * 4 - 1, 1));
            sensor.AddObservation(Mathf.Min(m_Feet[1].DistanceToGround() * 4 - 1, 1));


            bool opponentHasRig = Opponent.HasRig(out BodyJoint[] opponentJoints);

            for (int i = 0; i < c_NumJoints; i++)
            {
                // Value set by prior action, applies to all drives of a joint.
                sensor.AddObservation(m_Joints[i].NormStiffness);

                // Infer self motion from buffered rotations.
                sensor.AddObservation(m_JointRotationBuffer[i]);

                Quaternion rot = m_Joints[i].LocalRotation;
                sensor.AddObservation(rot);
                m_JointRotationBuffer[i] = rot;

                // Infer opponent motion from buffered positions.
                sensor.AddObservation(m_OpponentJointPositionBuffer[i]);

                if (opponentHasRig)
                {
                    // Opponent joint position in local reference frame.
                    // Distances are normalized from 0 - 2m with higher resolution at close range.
                    Vector3 pos = Sigmoid(BodyRoot.LocalPoint(opponentJoints[i].WorldPosition), 1.5f);
                    sensor.AddObservation(pos);
                    m_OpponentJointPositionBuffer[i] = pos;
                }
                else
                {
                    // There are rare cases when a decision happens between the opponent destroying
                    // and respawning its rig. Joint references are no longer valid at this point,
                    // so we just repeat adding the latest buffered values instead. 
                    sensor.AddObservation(m_OpponentJointPositionBuffer[i]);
                }
            }

            sensor.AddObservation(State.NormalizedCumulativeStrength);
        }

        public override void Heuristic(in ActionBuffers actionsOut) { }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            var actions = actionBuffers.ContinuousActions.Array;
            int step = (EpisodeStep + 1) % DecisionInterval;
            bool isPreDecisionStep = step == 0;

            if (isPreDecisionStep)
            {
                // Apply and store current actions prior to next decision.
                Array.Copy(actions, 0, m_ActionBuffer, 0, c_NumActions);
            }
            else
            {
                // Interpolate from previous to current actions.
                float t = step / (float)DecisionInterval;
                for (int i = 0; i < c_NumActions; i++)
                {
                    actions[i] = Mathf.Lerp(m_ActionBuffer[i], actions[i], t);
                }
            }

            for (int i = 0, j = 0; i < c_NumJoints; i++)
            {
                m_Joints[i].ApplyActions(actions, ref j);
            }

            if (SelfPlay)
            {
                // Need to keep track of velocities in self-play,
                // since ArticulationBody collisions don't seem 
                // to produce any relative velocities.

                m_Hands[0].BufferVelocity();
                m_Hands[1].BufferVelocity();

                for (int i = 0; i < m_Targets.Length; i++)
                {
                    m_Targets[i].BufferVelocity();
                }
            }

            if (isPreDecisionStep)
            {
                UpdateState();

                if (m_AddRandomForce && Random.value < EpisodeStep * c_RandomForceRatio)
                {
                    BodyRoot.AddRandomForce();
                }
            }
        }

        private void UpdateState()
        {
            // Dropping below min height deactivates agent immediately.
            IsActive = State.ValididateBodyHeight(BodyRoot.WorldPosition.y);

            if (IsActive)
            {
                float footToFootDistance = (m_Feet[0].WorldPosition - m_Feet[1].WorldPosition).magnitude;
                float handToHandDistance = (m_Hands[0].WorldPosition - m_Hands[1].WorldPosition).magnitude;

                if (SelfPlay)
                {
                    Vector3 opponentDelta = Opponent.BodyRoot.WorldPosition - BodyRoot.WorldPosition;
                    float angle = Vector3.Angle(BodyRoot.ForwardXZ, Vector3.ProjectOnPlane(opponentDelta, Vector3.up));
                    
                    if (angle <= 45)
                    {
                        // Add small reward to encourange agent-to-agent proximity and alignment.
                        float angleFactor = 1 - angle / 45f;
                        float distanceFactor = 1 - Mathf.Min(1, opponentDelta.magnitude);
                        AddReward(angleFactor * distanceFactor * m_ProximityRewardFactor);
                    }

                    State.ValidateFootToFootDistance(footToFootDistance);
                    State.ValidateHandToHandDistance(handToHandDistance);
                    State.ValidateIdleCount();
                }
                else
                {
                    // Train self-balancing and posture first, all factors are between 0 and 1.
                    // T-Pose has foot-to-foot distance within limits, but hand-to-hand distance out of bounds.
                    // Constraining the allowed hand-to-hand distance like this results in the agent balancing
                    // itself by moving the hands around in front of its body - which is a good starting point
                    // for later self-play training.
                    float poseFactor = Mathf.Max(0, 1 - BodyRoot.NormUpAngle);
                    float handFactor = Mathf.Max(0, 1 - State.GetHandDistanceExcess(handToHandDistance));
                    float footFactor = Mathf.Max(0, 1 - State.GetFootDistanceExcess(footToFootDistance));
                    //float heightFactor = Mathf.Min(1, BodyRoot.WorldPosition.y);
                    AddReward(poseFactor * handFactor * footFactor);
                }
            }

            if (State.EndEpisode)
            {
                EndEpisodeEvent.Invoke(this);
            }
        }

        private static float Sigmoid(float val, float scale = 1)
        {
            return Mathf.Clamp(val / (1f + Mathf.Abs(val)) * scale, -1f, 1f);
        }

        private static Vector3 Sigmoid(Vector3 v3, float scale = 1)
        {
            v3.x = Sigmoid(v3.x, scale);
            v3.y = Sigmoid(v3.y, scale);
            v3.z = Sigmoid(v3.z, scale);
            return v3;
        }
    }
}