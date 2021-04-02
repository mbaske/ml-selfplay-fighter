using UnityEngine;
using System;

namespace MBaske.SelfPlayFighter
{
    public class Hand : VelocityTracker
    {
        public event Action<float> TargetContactEvent;
        // We're registering hand-to-hand contacts, but not their strengths.
        public event Action HandContactEvent;

        [SerializeField, Tooltip("Ignore hits with strength below value.")]
        private float m_MinStrength = 1;
        [SerializeField, Tooltip("Cap hit strength to value.")]
        private float m_MaxStrength = 10;

        [SerializeField, Tooltip("Used for calculating contact direction.")]
        private Transform m_ReferencePoint;

        private string m_OpponentTargetTag;
        private string m_OpponentHandTag;
        private bool m_SelfPlay;

        public override void SetSelfPlay(bool selfPlay, int teamID)
        {
            m_SelfPlay = selfPlay;
            gameObject.tag = "Hand" + (teamID == 0 ? "A" : "B");

            m_OpponentHandTag = "Hand" + (teamID == 0 ? "B" : "A");
            m_OpponentTargetTag = "Target" + (teamID == 0 ? "B" : "A");
        }

        private void OnCollisionEnter(Collision collision)
        {
            // BUG: Debug.Log(collision.relativeVelocity);
            // https://issuetracker.unity3d.com/issues/collision-dot-relativevelocity-always-return-zero-when-using-articulationbody-on-collision

            // Reading the ArticulationBody's velocity here doesn't give reliable results either.
            // Values sometimes seem to indicate that the hand is already bouncing off the target,
            // producing negative dot products. Workaround: store velocities upfront, see VelocityTracker.

            if (m_SelfPlay)
            {
                var obj = collision.gameObject;

                if (obj.CompareTag(m_OpponentTargetTag))
                {
                    Vector3 pos = m_ReferencePoint.position;
                    Vector3 dir = (collision.collider.ClosestPoint(pos) - pos).normalized;
                    float relativeStrength = Vector3.Dot(GetVelocity() - obj.GetComponent<Target>().GetVelocity(), dir);
            
                    if (relativeStrength >= m_MinStrength)
                    {
                        TargetContactEvent?.Invoke(Mathf.Min(relativeStrength, m_MaxStrength));
                    }
                }
                else if (obj.CompareTag(m_OpponentHandTag))
                {
                    HandContactEvent?.Invoke();
                }
            }
        }
    }
}