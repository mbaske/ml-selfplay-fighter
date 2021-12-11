using UnityEngine;

public class AgentReferenceFrame : MonoBehaviour
{
    // World pos at ground.
    public Vector3 Position;
    // Local to boxing ring (5 x 5 m), mirrored x/z coords for agent with
    // default z-pos < 0. y is the relative angle towards the ring center.
    public Vector3 NormPos;
    // World velocity.
    public Vector3 Velocity;
    // Local around Vector3.up axis.
    public float AngularVelocity;
    public float Inclination;

    public Matrix4x4 Matrix => m_Matrix;

    private Transform m_Hips;
    private Transform m_Head;
    private Transform m_BoxingRing;

    private Matrix4x4 m_Matrix;
    private Matrix4x4 m_InvMatrix;

    private Vector3 m_HipsPos;
    private Vector3 m_PrevHipsPos;
    private Vector3 m_FaceForward;
    private Vector3 m_PrevFaceFwd;

    // -1 mirrors local position.
    private int m_Sign = 1;
    // Boxing ring ground y.
    private const float c_Ground = 0;


    // Initialize, Reset and Update methods are invoked via
    // ObservationController, who utilizes the reference frame.

    public void Initialize()
    {
        m_Hips = transform.Find("mixamorig:Hips");
        m_Head = transform.DeepFind("mixamorig:Head");
        m_BoxingRing = transform.parent == null
            ? GameObject.FindWithTag(TagsAndLayers.BoxingRing).transform
            : transform.parent.Find("BoxingRing");

        m_Sign = -(int)Mathf.Sign(m_BoxingRing.InverseTransformPoint(m_Hips.position).z);
    }

    public void ManagedReset()
    {
        ManagedUpdate();

        m_PrevHipsPos = m_HipsPos;
        m_PrevFaceFwd = m_FaceForward;

        Velocity = Vector3.zero;
        AngularVelocity = 0;
    }

    public void ManagedUpdate(float deltaTime)
    {
        ManagedUpdate();

        Velocity = (m_HipsPos - m_PrevHipsPos) / deltaTime;
        m_PrevHipsPos = m_HipsPos;
       
        AngularVelocity = Vector3.SignedAngle(m_PrevFaceFwd, m_FaceForward, Vector3.up)
            / deltaTime * Mathf.Deg2Rad;
        m_PrevFaceFwd = m_FaceForward;
    }

    public Vector3 LocalizePoint(Vector3 p)
    {
        return m_InvMatrix.MultiplyPoint3x4(p);
    }

    public Vector3 LocalizeVector(Vector3 v)
    {
        return m_InvMatrix.MultiplyVector(v);
    }

    private void ManagedUpdate()
    {
        m_FaceForward = Vector3.ProjectOnPlane(m_Head.forward, Vector3.up).normalized;

        m_HipsPos = m_Hips.position;
        Position = m_HipsPos;
        Position.y = c_Ground;

        m_Matrix = Matrix4x4.TRS(Position, Quaternion.LookRotation(m_FaceForward), Vector3.one);
        m_InvMatrix = m_Matrix.inverse;

        NormPos = m_BoxingRing.InverseTransformPoint(Position) * m_Sign / 2.5f;
        NormPos.y = Vector3.SignedAngle(m_FaceForward, NormPos * -m_Sign, Vector3.up) / 180f;

        Inclination = Vector3.Angle(Vector3.up, m_Head.position - m_HipsPos);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(Position, m_FaceForward);
    }
}
