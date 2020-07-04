using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System;

namespace MBaske.Fighter
{
    public class FighterAgent : Agent
    {
        [SerializeField]
        private FighterAgent opponent;
        
        // DEMO
        public event Action<OpponentCollision> OnValidPunch;
        public int WinCount { get; private set; }
        public int DownCount { get; private set; }

        // CURRICULUM
        public event Action<EpisodeStop> OnEpisodeStop;
        // Accumulated strike force.
        public float MaxAccumForce { get; set; } // see AutoCurriculum
        public float AccumForce { get; private set; }
        public float CrntForce { get; private set; }
        // TODO Automate value adjustments.
        [SerializeField, Tooltip("Minimum punch force required (velocity magnitude)")]
        public float MinForce = 1f;

        // Agent down.
        public int MaxDownSteps { get; set; } // see AutoCurriculum
        public int DownStepCount { get; private set; }
        public bool IsDown { get; private set; }
        // TODO Automate value adjustments.
        [SerializeField, Tooltip("Agent is considered down if its head drops below this height")]
        public float DownThresh = 1.3f;

        // PHYSICS
        [Header("Observable BodyParts")]
        public Observables Hips;
        public Observables Head;
        public Observables LeftHand;
        public Observables RightHand;
        public Observables LeftFoot;
        public Observables RightFoot;
        // Part list used for downward raycasts.
        private Observables[] parts;
        private BodyRoot root;
        private BallJoint[] joints;
        private Resetter resetter;

        // RAY DETECTION
        private static Ray[] rays = CreateRays();
        private const float rayRadius = 0.1f;
        private const float rayLength = 2f;
        private const int groundLayerMask = 1 << 8;
        private int opponentLayerMask;

        // OBSERVATIONS
        private List<float> jointObs;
        private List<float> rayObs;

        // ACTIONS
        private int interval;
        private int numActions;
        private int actionStep;
        private float[] lerpActions;

        // DEBUG
        [Header("Debug")]
        [SerializeField]
        private bool ignoreActions;
        [SerializeField]
        private bool fixateBodyRoot;
        [SerializeField]
        private bool drawRays;

        public override void Initialize()
        {
            resetter = new Resetter(transform);
            root = GetComponentInChildren<BodyRoot>();
            joints = GetComponentsInChildren<BallJoint>();
            jointObs = new List<float>();

            // https://github.com/Unity-Technologies/ml-agents/issues/4180
            var a = GetComponent<DecisionRequester>();
            var b = GetComponent<PausableDecisionRequester>(); // demo
            interval = b != null ? b.DecisionPeriod : a.DecisionPeriod;
            numActions = GetComponent<BehaviorParameters>().BrainParameters.NumActions;
            lerpActions = new float[numActions];

            rayObs = new List<float>();
            parts = new Observables[] { Hips, Head, LeftHand, RightHand, LeftFoot, RightFoot };
            opponentLayerMask = 1 << opponent.gameObject.layer;

            if (fixateBodyRoot)
            {
                root.gameObject.AddComponent<FixedJoint>();
            }

            root.Initialize();
            foreach (BallJoint joint in joints)
            {
                joint.Initialize();
            }

            var detectors = GetComponentsInChildren<CollisionDetection>();
            foreach (var detector in detectors)
            {
                detector.OnCollision += OnCollision;
            }
        }

        private void OnDestroy()
        {
            var detectors = GetComponentsInChildren<CollisionDetection>();
            foreach (var detector in detectors)
            {
                detector.OnCollision -= OnCollision;
            }
        }

        public override void OnEpisodeBegin()
        {
            resetter.Reset();

            if (!fixateBodyRoot)
            {
                RandomizePosition();
            }

            foreach (BallJoint joint in joints)
            {
                joint.OnReset();
            }
            root.OnReset();

            // Start new episode with nulled action values -> T-pose.
            Array.Clear(lerpActions, 0, numActions);

            CrntForce = 0;
            AccumForce = 0;
            DownStepCount = 0;
        }

        public override void CollectObservations(VectorSensor sensor) // 194
        {
            if (opponent == null)
            {
                return;
            }

            actionStep = 0;
            Transform rt = root.transform;
            Vector3 inclination = new Vector3(rt.right.y, rt.up.y, rt.forward.y);
            sensor.AddObservation(inclination);
            sensor.AddObservation(Normalization.Sigmoid(CrntForce, 0.25f) * 2f - 1f);
            CrntForce = 0;
            // Observe positions relative to this agent's root (hips).
            // All vectors are localized (root.InverseTransformVector(v))
            // and normalized using a sigmoid function. The idea here is
            // that small value changes matter less the farther away an
            // observed object is, or the faster it is moving.
            // The sigmoid function provides a higher value resolution
            // for small vectors and asymptotes towards -1/+1.
            Vector3 rootPos = rt.position;
            // This agent. 
            // Hips position = root position. Since all positions are relative to 
            // this agent's root, we don't observe this, as it will always be 0/0/0.
            AddVectorObs(sensor, Hips.Velocity);
            AddVectorObs(sensor, Hips.AngularVelocity);
            AddVectorObs(sensor, Head.Position - rootPos);
            AddVectorObs(sensor, Head.Velocity);
            AddVectorObs(sensor, LeftHand.Position - rootPos);
            AddVectorObs(sensor, LeftHand.Velocity);
            AddVectorObs(sensor, RightHand.Position - rootPos);
            AddVectorObs(sensor, RightHand.Velocity);
            AddVectorObs(sensor, LeftFoot.Position - rootPos);
            AddVectorObs(sensor, LeftFoot.Velocity);
            AddVectorObs(sensor, RightFoot.Position - rootPos);
            AddVectorObs(sensor, RightFoot.Velocity);
            // Opponent agent.
            AddVectorObs(sensor, opponent.Hips.Position - rootPos);
            AddVectorObs(sensor, opponent.Hips.Velocity);
            AddVectorObs(sensor, opponent.Hips.AngularVelocity);
            AddVectorObs(sensor, opponent.Head.Position - rootPos);
            AddVectorObs(sensor, opponent.Head.Velocity);
            AddVectorObs(sensor, opponent.LeftHand.Position - rootPos);
            AddVectorObs(sensor, opponent.LeftHand.Velocity);
            AddVectorObs(sensor, opponent.RightHand.Position - rootPos);
            AddVectorObs(sensor, opponent.RightHand.Velocity);
            AddVectorObs(sensor, opponent.LeftFoot.Position - rootPos);
            AddVectorObs(sensor, opponent.LeftFoot.Velocity);
            AddVectorObs(sensor, opponent.RightFoot.Position - rootPos);
            AddVectorObs(sensor, opponent.RightFoot.Velocity);
            // Normalized rotations (wrapped eulers / 180).
            sensor.AddObservation(GetJointObs());
            // Normalized distances.
            sensor.AddObservation(GetRayObs());

            // TODO AddObservation DownStepCount / MaxDownSteps
        }

        private void AddVectorObs(VectorSensor sensor, Vector3 v)
        {
            sensor.AddObservation(Normalization.Sigmoid(root.Localize(v)));
        }

        private List<float> GetJointObs() // 48
        {
            jointObs.Clear();
            foreach (BallJoint joint in joints)
            {
                joint.AddNormRotationTo(jointObs);
            }

            return jointObs;
        }

        private List<float> GetRayObs() // 69
        {
            // Normalize results to -1/+1 range.
            // With rayLength = 2, we don't need to scale hit.distance.
            rayObs.Clear();
            RaycastHit hit;

            // Detect opponent.
            Transform rt = root.transform;
            Vector3 pos = rt.position;
            Quaternion yRot = Quaternion.Euler(0, rt.eulerAngles.y, 0);
            foreach (Ray ray in rays) // 63
            {
                Vector3 p = pos + yRot * ray.origin;
                Vector3 d = yRot * ray.direction;
                rayObs.Add(Physics.SphereCast(
                    p, rayRadius, d, out hit, rayLength, opponentLayerMask)
                    ? hit.distance - 1f : 1f);

                if (drawRays)
                {
                    Debug.DrawRay(p, d * rayLength, Color.cyan);
                }
            }

            // Detect ground.
            foreach (Observables part in parts) // 6
            {
                rayObs.Add(Physics.Raycast(
                    part.Position, Vector3.down, out hit, rayLength, groundLayerMask)
                    ? hit.distance - 1f : 1f);

                if (drawRays)
                {
                    Debug.DrawRay(part.Position, Vector3.down * rayLength, Color.magenta);
                }
            }

            return rayObs;
        }


        public override void OnActionReceived(float[] actions) // 37
        {
            if (!ignoreActions)
            {
                actionStep++;
                float t = actionStep / (float)interval;
                // Interpolate from previous to current 
                // actions for smooth joint rotations.
                for (int i = 0; i < numActions; i++)
                {
                    lerpActions[i] = Mathf.Lerp(lerpActions[i], actions[i], t);
                }

                int index = 0;
                // Pass index by reference: joints will apply as 
                // many action values as they have degrees of freedom.
                foreach (BallJoint joint in joints)
                {
                    joint.SetNormRotation(lerpActions, ref index);
                }
            }

            foreach (BallJoint joint in joints)
            {
                joint.ApplyRotation();
            }

            // SetOptionalRewards();
            CheckState();
        }

        private void CheckState()
        {
            float y = Head.Position.y; // ca. 1.7 in T-pose
            IsDown = y < DownThresh;
            DownStepCount = IsDown ? DownStepCount + 1 : 0;

            if (AccumForce >= MaxAccumForce)
            {
                OnEpisodeStop(EpisodeStop.Win);
                Done(1f, -0.5f); // alt. Done(1f, -1f); or Done(1f, 0f);
                WinCount++;
            }
            else if (IsDown)
            {
                if (opponent.IsDown)
                {
                    OnEpisodeStop(EpisodeStop.Draw);
                    Done(0f, 0f);
                    DownCount++;
                }
                else if (DownStepCount == MaxDownSteps)
                {
                    OnEpisodeStop(EpisodeStop.Lose);
                    Done(-1f, 0.5f); // alt. Done(-1f, 1f); or Done(-1f, 0f);
                    DownCount++;
                }
            }
        }

        private void Done(float thisRwd, float oppRwd)
        {
            SetReward(thisRwd);
            EndEpisode();
            opponent.SetReward(oppRwd);
            opponent.EndEpisode();
        }

        private void OnCollision(OpponentCollision collision)
        {
            if (collision.IsPunch() && !collision.OpponentBlocksPunch())
            {
                float force = collision.velocity.magnitude;
                CrntForce = force;
                if (force >= MinForce)
                {
                    AccumForce += force;
                    OnValidPunch?.Invoke(collision); // demo
                    // Debug.Log($"Punch {name}: {collision.bodyPartA.name} -> {collision.bodyPartB.name} | {force}");
                }
            }
        }

        private void SetOptionalRewards()
        {
            // Some optional rewards for fine-tuning behaviour.
            // NOTE Setting positive rewards in open-ended episodes can
            // lead to a conservative fighting style, because agents will 
            // be careful not to end episodes, but keep on collecting rewards.
            // Conversely, penalties might incentivise them to finish episodes early. 

            // Penalize facing away from opponent.
            Vector3 hipsDelta = opponent.Hips.Position - Hips.Position;
            float normAngle = Vector3.Angle(
                Vector3.ProjectOnPlane(Hips.Forward, Vector3.up),
                Vector3.ProjectOnPlane(hipsDelta, Vector3.up)
            ) / 180f;
            AddReward(normAngle * -0.03f);

            // Reward proximity to opponent.
            float sqrInvDistance = Mathf.Pow(1f / Mathf.Max(1f, hipsDelta.magnitude + 0.5f), 2f);
            AddReward(sqrInvDistance * 0.01f);
        }

        private void RandomizePosition()
        {
            Vector3 p = transform.localPosition;
            p.z += Random.value * Mathf.Sign(p.z) * 0.5f;
            p.x += Random.value - 0.5f;
            transform.localPosition = p;
        }

        // Ray matrix for opponent detection.
        private static Ray[] CreateRays()
        {
            List<Ray> rays = new List<Ray>();
            for (int x = -3; x <= 3; x++)
            {
                for (int y = -4; y <= 4; y++)
                {
                    rays.Add(new Ray(
                        new Vector3(x * 0.2f, y * 0.2f, 0),
                        Quaternion.Euler(-y, x * 3, 0) * Vector3.forward
                    ));
                }
            }
            return rays.ToArray();
        }
    }
}


