using UnityEngine;

namespace MBaske.SelfPlayFighter
{
    public abstract class ArticulationBase : MonoBehaviour
    {
        public Quaternion LocalRotation => transform.localRotation;
        public Vector3 WorldPosition => m_Body.worldCenterOfMass;

        protected ArticulationBody m_Body;

        private const int m_GroundMask = 1 << 3;

        public virtual void Initialize()
        {
            m_Body = GetComponent<ArticulationBody>();
        }

        public virtual void SetSelfPlay(bool selfPlay, int teamID)
        {
            // Layers:
            // 6 - AgentA (rig default)
            // 7 - AgentA-IgnoreSelfCollide (rig default)
            // 8 - AgentB
            // 9 - AgentB-IgnoreSelfCollide

            gameObject.layer += teamID * 2;
        }

        public float DistanceToGround()
        {
            if (Physics.Raycast(WorldPosition, Vector3.down, out RaycastHit hit, 2, m_GroundMask))
            {
                return hit.distance;
            }

            return 2;
        }
    }
}