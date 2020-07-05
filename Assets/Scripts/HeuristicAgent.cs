using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using System;

namespace MBaske.Fighter
{
    // Agent for testing joint rotations.
    public class HeuristicAgent : Agent
    {
        [SerializeField]
        private bool fixateBodyRoot;

        private BodyRoot root;
        private BallJoint[] joints;
        private Resetter resetter;

        private int interval;
        private int numActions;
        private int actionStep;
        private float[] lerpActions;
        private float[] sliderValues;

        public override void Initialize()
        {
            root = GetComponentInChildren<BodyRoot>();
            joints = GetComponentsInChildren<BallJoint>();
            interval = GetComponent<DecisionRequester>().DecisionPeriod;
            numActions = GetComponent<BehaviorParameters>().BrainParameters.NumActions;
            lerpActions = new float[numActions];
            sliderValues = new float[numActions];
            resetter = new Resetter(transform);

            if (fixateBodyRoot)
            {
                root.gameObject.AddComponent<FixedJoint>();
            }

            root.Initialize();
            foreach (BallJoint joint in joints)
            {
                joint.Initialize();
            }

            Transform canvas = GetComponentInChildren<Canvas>().transform;
            for (int index = 0, i = 0; i < canvas.childCount; i++)
            {
                Transform tf = canvas.GetChild(i);
                Text text = tf.GetComponentInChildren<Text>();
                text.text = joints[i].name;

                IndexedSlider[] s = tf.GetComponentsInChildren<IndexedSlider>();
                for (int j = 0; j < 3; j++)
                {
                    if (joints[i].DOF[j])
                    {
                        s[j].SetIndex(index++);
                        s[j].OnValueChange = OnSliderValueChange;
                    }
                    else
                    {
                        s[j].SetInteractable(false);
                    }
                }
            }
        }

        public override void OnEpisodeBegin()
        {
            resetter.Reset();
            foreach (BallJoint joint in joints)
            {
                joint.OnReset();
            }
            root.OnReset();
            Array.Clear(lerpActions, 0, numActions);
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            actionStep = 0;
        }

        public override void OnActionReceived(float[] actions)
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

            foreach (BallJoint joint in joints)
            {
                joint.ApplyRotation();
            }
        }

        public override void Heuristic(float[] actionsOut)
        {
            for (int i = 0; i < sliderValues.Length; i++)
            {
                actionsOut[i] = sliderValues[i];
            }
        }

        public void OnSliderValueChange(int index, float val)
        {
            sliderValues[index] = val;
        }
    }
}


