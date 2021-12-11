using System;
using UnityEngine;

public enum TwistAxis
{
    Right, Up, Forward
}

public static class JointUtil
{
    public static Quaternion SwingTwistToRotation(
        JointSettings.Joint settings, Vector3 eulers)
    {
        if (settings.TwistAxis == TwistAxis.Right)
        {
            return Quaternion.Euler(0, eulers.y, eulers.z)
                 * Quaternion.Euler(eulers.x, 0, 0);
        }

        if (settings.TwistAxis == TwistAxis.Up)
        {
            return Quaternion.Euler(eulers.x, 0, eulers.z)
                 * Quaternion.Euler(0, eulers.y, 0);
        }

        if (settings.TwistAxis == TwistAxis.Forward)
        {
            return Quaternion.Euler(eulers.x, eulers.y, 0)
                 * Quaternion.Euler(0, 0, eulers.z);
        }

        throw new NotImplementedException("Invalid settings");
    }

    public static Vector3 RotationToSwingTwist(
        JointSettings.Joint settings, Quaternion rotation)
    {
        Vector3 eulers = Vector3.zero;

        if (settings.TwistAxis == TwistAxis.Right)
        {
            Vector3 proj = Vector3.ProjectOnPlane(
                rotation * Vector3.right,
                Vector3.up);

            eulers.y = Vector3.SignedAngle(
                Vector3.right,
                proj,
                Vector3.up);

            eulers.z = Vector3.SignedAngle(
                proj,
                rotation * Vector3.right,
                Vector3.forward);

            if (Mathf.Abs(eulers.y) >= 90)
            {
                eulers.z *= -1;
            }

            eulers.x = Vector3.SignedAngle(
                Quaternion.Euler(eulers) * Vector3.up,
                rotation * Vector3.up,
                rotation * Vector3.right);

            return eulers;
        }

        if (settings.TwistAxis == TwistAxis.Up)
        {
            Vector3 proj = Vector3.ProjectOnPlane(
                rotation * Vector3.up,
                Vector3.right);

            eulers.x = Vector3.SignedAngle(
                Vector3.up,
                proj,
                Vector3.right);

            eulers.z = Vector3.SignedAngle(
                proj,
                rotation * Vector3.up,
                Vector3.forward);

            if (Mathf.Abs(eulers.x) >= 90)
            {
                eulers.z *= -1;
            }

            eulers.y = Vector3.SignedAngle(
                Quaternion.Euler(eulers) * Vector3.forward,
                rotation * Vector3.forward,
                rotation * Vector3.up);

            return eulers;
        }

        if (settings.TwistAxis == TwistAxis.Forward)
        {
            // TODO axis flip check.

            Vector3 proj = Vector3.ProjectOnPlane(
            rotation * Vector3.forward,
            Vector3.up);

            eulers.y = Vector3.SignedAngle(
                Vector3.forward,
                proj,
                Vector3.up);

            eulers.x = Vector3.SignedAngle(
                proj,
                rotation * Vector3.forward,
                Vector3.right);

            eulers.z = Vector3.SignedAngle(
                Quaternion.Euler(eulers) * Vector3.up,
                rotation * Vector3.up,
                rotation * Vector3.forward);

            return eulers;
        }

        throw new NotImplementedException("Invalid settings");
    }



    public static Vector3 NormalizeSwingTwist(
        JointSettings.Joint settings, Vector3 eulers)
    {
        eulers.x = Mathf.Clamp((eulers.x - settings.Center.x)
            / settings.Extent.x, -1, 1);
        eulers.y = Mathf.Clamp((eulers.y - settings.Center.y)
            / settings.Extent.y, -1, 1);
        eulers.z = Mathf.Clamp((eulers.z - settings.Center.z)
            / settings.Extent.z, -1, 1);

        return eulers;
    }

    public static Vector3 DeNormalizeSwingTwist(
        JointSettings.Joint settings, Vector3 eulers)
    {
        return settings.Center + Vector3.Scale(settings.Extent, eulers);
    }


    public static Vector3 SwingTwistToDriveTargets(Vector3 eulers)
    {
        return EulersToDriveTargets(eulers);
    }

    public static Vector3 NormalizedSwingTwistToDriveTargets(
        JointSettings.Joint settings, float x, float y, float z)
    {
        return EulersToDriveTargets(
            DeNormalizeSwingTwist(settings, new Vector3(x, y, z)));
    }

    public static Vector3 NormalizedSwingTwistToDriveTargets(
        JointSettings.Joint settings, Vector3 eulers)
    {
        return EulersToDriveTargets(
            DeNormalizeSwingTwist(settings, eulers));
    }

    // Articulation body drive axes are flipped 
    // with respect to their transforms' local eulers.

    public static Vector3 EulersToDriveTargets(Vector3 eulers)
    {
        return new Vector3(eulers.y, -eulers.x, eulers.z);
    }

    public static Vector3 WrapEulers(Vector3 eulers)
    {
        eulers.x = Mathf.DeltaAngle(0, eulers.x);
        eulers.y = Mathf.DeltaAngle(0, eulers.y);
        eulers.z = Mathf.DeltaAngle(0, eulers.z);

        return eulers;
    }
}
