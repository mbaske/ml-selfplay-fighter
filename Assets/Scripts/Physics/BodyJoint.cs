using UnityEngine;

namespace MBaske.SelfPlayFighter
{
    public class BodyJoint : ArticulationBase
    {
        public float NormStiffness { get; private set; }

        private bool[] m_DriveEnabled;

        public override void Initialize()
        {
            base.Initialize();

            m_DriveEnabled = new bool[]
            {
                m_Body.twistLock == ArticulationDofLock.LimitedMotion,
                m_Body.swingYLock == ArticulationDofLock.LimitedMotion,
                m_Body.swingZLock == ArticulationDofLock.LimitedMotion
            };
        }

        public void ApplyActions(float[] actions, ref int index)
        {
            // Store for observation.
            NormStiffness = actions[index++]; 
            float stiffness = Stiffness(NormStiffness);

            if (m_DriveEnabled[0])
            {
                m_Body.xDrive = UpdateDrive(m_Body.xDrive, stiffness, actions[index++]);
            }
            if (m_DriveEnabled[1])
            {
                m_Body.yDrive = UpdateDrive(m_Body.yDrive, stiffness, actions[index++]);
            }
            if (m_DriveEnabled[2])
            {
                m_Body.zDrive = UpdateDrive(m_Body.zDrive, stiffness, actions[index++]);
            }
        }

        public void SetGlobalDriveValues(float normStiffness, float damping, float forceLimit)
        {
            float stiffness = Stiffness(normStiffness);
            m_Body ??= GetComponent<ArticulationBody>();
            m_Body.xDrive = UpdateDrive(m_Body.xDrive, stiffness, damping, forceLimit);
            m_Body.yDrive = UpdateDrive(m_Body.yDrive, stiffness, damping, forceLimit);
            m_Body.zDrive = UpdateDrive(m_Body.zDrive, stiffness, damping, forceLimit);
        }

        private static ArticulationDrive UpdateDrive(ArticulationDrive drive, float stiffness, float damping, float forceLimit)
        {
            drive.stiffness = stiffness;
            drive.damping = damping;
            drive.forceLimit = forceLimit;

            return drive;
        }

        private static ArticulationDrive UpdateDrive(ArticulationDrive drive, float stiffness, float target)
        {
            drive.stiffness = stiffness;
            drive.target = target * (target > 0 ? drive.upperLimit : -drive.lowerLimit);

            return drive;
        }

        private static float Stiffness(float norm)
        {
            return Mathf.Pow(10, norm + 3) * 2.5f; // 250 - 25000
        }


        //// Debug/Tweak.
        //[SerializeField, Range(-1f, 1f)]
        //private float s;
        //[SerializeField, Range(-1f, 1f)]
        //private float x;
        //[SerializeField, Range(-1f, 1f)]
        //private float y;
        //[SerializeField, Range(-1f, 1f)]
        //private float z;

        //private void FixedUpdate()
        //{
        //    float[] actions = new float[m_Body.dofCount + 1];

        //    int index = 0;
        //    actions[index++] = s;

        //    if (m_DriveEnabled[0])
        //    {
        //        actions[index++] = x;
        //    }
        //    if (m_DriveEnabled[1])
        //    {
        //        actions[index++] = y;
        //    }
        //    if (m_DriveEnabled[2])
        //    {
        //        actions[index++] = z;
        //    }

        //    index = 0;
        //    ApplyActions(actions, ref index);
        //}
    }
}