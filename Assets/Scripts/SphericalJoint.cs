using UnityEngine;

public class SphericalJoint
{
    private readonly ArticulationBody m_Body;
    private Vector3 m_TPose;

    public SphericalJoint(ArticulationBody body, Vector3 tPose)
    {
        m_Body = body;
        m_TPose = tPose;
    }

    public void ManagedReset()
    {
        SetDriveTargets(m_TPose);
    }

    public void ApplySpringSettings(JointSettings settings)
    {
        var drive = m_Body.xDrive;
        drive.damping = settings.SpringDamping;
        drive.stiffness = settings.SpringStiffness;
        drive.forceLimit = settings.SpringForceLimit;
        m_Body.xDrive = drive;

        drive = m_Body.yDrive;
        drive.damping = settings.SpringDamping;
        drive.stiffness = settings.SpringStiffness;
        drive.forceLimit = settings.SpringForceLimit;
        m_Body.yDrive = drive;

        drive = m_Body.zDrive;
        drive.damping = settings.SpringDamping;
        drive.stiffness = settings.SpringStiffness;
        drive.forceLimit = settings.SpringForceLimit;
        m_Body.zDrive = drive;
    }

    public void SetDriveTargets(Vector3 eulers)
    {
        var drive = m_Body.xDrive;
        drive.target = eulers.x;
        m_Body.xDrive = drive;

        drive = m_Body.yDrive;
        drive.target = eulers.y;
        m_Body.yDrive = drive;

        drive = m_Body.zDrive;
        drive.target = eulers.z;
        m_Body.zDrive = drive;
    }
}
