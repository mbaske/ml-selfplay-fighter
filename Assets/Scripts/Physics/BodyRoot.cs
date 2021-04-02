using UnityEngine;

namespace MBaske.SelfPlayFighter
{
    public class BodyRoot : ArticulationBase
    {
        public float NormUpAngle => Vector3.Angle(Vector3.up, transform.up) / 90f;
        public float NormFwdAngle => Vector3.SignedAngle(m_RefForward, transform.forward, Vector3.up) / 180f;
        public Vector3 ForwardXZ => Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        public Vector3 NormPosition => m_NormPosition;
        public Vector3 LocalVelocity => LocalVector(m_Body.velocity);
        public Vector3 LocalAngularVelocity => LocalVector(m_Body.angularVelocity);
        public Vector3 AxisInclination => new Vector3(transform.right.y, transform.up.y, transform.forward.y);


        [Header("Global Joint Settings")]
        [SerializeField, Range(-1f, 1f)]
        private float m_Stiffness = 0;
        [SerializeField]
        private float m_Damping = 100;
        [SerializeField]
        private float m_ForceLimit = 1000;

        private void OnValidate()
        {
            var joints = GetComponentsInChildren<BodyJoint>();
            foreach (var joint in joints)
            {
                joint.SetGlobalDriveValues(m_Stiffness, m_Damping, m_ForceLimit);
            }
        }


        // Reference values for calculating position  
        // and forward angle relative to boxing ring.
        private Vector3 m_RefCenter;
        private Vector3 m_RefForward;
        private Quaternion m_RefRotation;

        private Vector3 m_NormPosition;
        
        // Matrix for transforming opponent positions to local frame.
        // Aligned with transform XZ position and Y rotation.
        // Y position and XZ rotation are fixed.
        private Matrix4x4 m_ToLocalMatrix;

        // Assuming ground level y = 0.
        private const float c_DefHeight = 1;

        private float m_StablizationStrength;

        public void SetStablizationStrength(float strength)
        {
            m_StablizationStrength = strength;
        }

        public void SetCenterPosition(Vector3 center)
        {
            m_RefCenter = center;
            m_RefForward = transform.position.z < center.z ? Vector3.forward : Vector3.back;
            m_RefRotation = Quaternion.LookRotation(m_RefForward);
        }

        public void ManagedUpdate()
        {
            Vector3 pos = transform.position;
            Vector3 offset = m_RefRotation * (pos - m_RefCenter);

            // Boxing ring is 2x2m.
            m_NormPosition.x = Mathf.Clamp(offset.x * 0.5f, -1f, 1f);
            m_NormPosition.z = Mathf.Clamp(offset.z * 0.5f, -1f, 1f);

            m_NormPosition.y = Mathf.Clamp(pos.y - c_DefHeight, -1f, 1f);
          
            pos.y = c_DefHeight;
            Quaternion rot = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            m_ToLocalMatrix = Matrix4x4.TRS(pos, rot, Vector3.one).inverse;
        }

        public Vector3 LocalPoint(Vector3 p)
        {
            return m_ToLocalMatrix.MultiplyPoint3x4(p);
        }

        public Vector3 LocalVector(Vector3 v)
        {
            return transform.InverseTransformDirection(v);
        }

        public void AddRandomForce(float min = 1000, float max = 5000)
        {
            m_Body.AddRelativeForce(new Vector3(Random.Range(min, max), 0, Random.Range(min, max)));
        }

        private void FixedUpdate()
        {
            if (m_StablizationStrength > 0)
            {
                Vector3 counterTorque = (
                    Vector3.right * transform.forward.y
                     + Vector3.back * transform.right.y
                ) * m_StablizationStrength;
                m_Body.AddRelativeTorque(counterTorque);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            var list = GetComponentsInChildren<ArticulationBody>();
            foreach (var body in list)
            {
                Gizmos.DrawSphere(body.worldCenterOfMass, 0.025f);
            }
        }
    }
}