using UnityEngine;

namespace MBaske.SelfPlayFighter
{
    public class VelocityTracker : ArticulationBase
    {
        private Vector3[] m_VelocityBuffer;
        private const int c_BufferSize = 3;
        private int m_BufferIndex;

        public override void Initialize()
        {
            base.Initialize(); 

            m_VelocityBuffer = new Vector3[c_BufferSize];
        }

        public void BufferVelocity()
        {
            m_VelocityBuffer[m_BufferIndex++] = m_Body.velocity;
            m_BufferIndex %= c_BufferSize;
        }

        public Vector3 GetVelocity()
        {
            Vector3 sum = Vector3.zero;
            for (int i = 0; i < c_BufferSize; i++)
            {
                sum += m_VelocityBuffer[i];
            }
            return sum / c_BufferSize;
        }
    }
}