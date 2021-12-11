using UnityEngine;

public class TransformTracker : MonoBehaviour
{
    // World
    public Vector3 Position;
    public Vector3 Velocity;
    public Vector3 AngularVelocity;

    public Quaternion LocalRotation;

    private Vector3 m_PrevPos;
    private Quaternion m_PrevRot;

    public void ManagedReset()
    {
        Position = transform.position;
        Velocity = Vector3.zero;
        AngularVelocity = Vector3.zero;
        LocalRotation = Quaternion.identity;

        m_PrevPos = Position;
        m_PrevRot = transform.rotation;
    }

    public void ManagedUpdate(float deltaTime)
    {
        Position = transform.position;
        Velocity = (Position - m_PrevPos) / deltaTime;
        m_PrevPos = Position;

        Quaternion rot = transform.rotation;
        Quaternion delta = rot * Quaternion.Inverse(m_PrevRot);
        delta.ToAngleAxis(out float angle, out Vector3 axis);
        m_PrevRot = rot;
        AngularVelocity = 1 / deltaTime * angle * Mathf.Deg2Rad * axis;

        LocalRotation = transform.localRotation;
    }
}
