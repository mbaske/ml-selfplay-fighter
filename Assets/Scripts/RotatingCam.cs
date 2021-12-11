using UnityEngine;

public class RotatingCam : MonoBehaviour
{
    [SerializeField]
    private float m_Height = 2;
    [SerializeField]
    private float m_Offset = -0.25f;
    [SerializeField]
    private float m_Radius = 3;
    [SerializeField]
    private float m_Speed = 0.25f;
    [SerializeField]
    private float m_Smooth = 0.5f;

    [SerializeField]
    private Transform[] m_Agents;
    private Vector3 m_LookTarget;
    private Vector3 m_LookVelo;
    private Vector3 m_MoveTarget;
    private Vector3 m_MoveVelo;
    private float angle;

    private void LateUpdate()
    {
        angle += Time.deltaTime * m_Speed;
        Vector3 target = new Vector3(Mathf.Cos(angle) * m_Radius,
            m_Height, Mathf.Sin(angle) * m_Radius);
        m_MoveTarget = Vector3.SmoothDamp(
            m_MoveTarget, target, ref m_MoveVelo, 0.5f);
        transform.position = m_MoveTarget;

        target = Vector3.zero;
        for (int i = 0; i < m_Agents.Length; i++)
        {
            target += m_Agents[i].position;
        }
        target /= m_Agents.Length;
        target += Vector3.up * m_Offset;

        m_LookTarget = Vector3.SmoothDamp(
            m_LookTarget, target, ref m_LookVelo, m_Smooth);
        transform.LookAt(m_LookTarget);
    }
}