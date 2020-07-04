using UnityEngine;

namespace MBaske.Fighter
{
    public enum BodyPartName
    {
        Hips = 0,
        Spine1 = 1,
        Spine2 = 2,
        Head = 3,
        Arm = 4,
        ForeArm = 5,
        Hand = 6,
        UpLeg = 7,
        Leg = 8,
        Foot = 9
    }

    public enum BodyPartSide
    {
        None = 0,
        Left = 1,
        Right = 2
    }

    public abstract class BodyPart : MonoBehaviour
    {
        public BodyPartName Name;
        public BodyPartSide Side;

        [Tooltip("The skeleton transform associated with this rigidbody")]
        public Transform animTarget;

        protected Quaternion defRotSelf;
        protected Quaternion defRotAnim;
        // Need to flip rotation axes for some of the body parts
        // in order to sync skeleton rotations with rigidbodies.
        private FlipAxesDel flipAxes;
        private delegate Quaternion FlipAxesDel(Quaternion q);

        public virtual void Initialize()
        {
            FlipAxes flip = GetComponent<FlipAxes>();
            flipAxes = flip != null ? (FlipAxesDel)flip.Flip : DefaultAxes;
        }

        public virtual void OnReset()
        {
            defRotAnim = animTarget.localRotation;
        }

        private Quaternion DefaultAxes(Quaternion q)
        {
            return q;
        }

        protected void AnimateSkeleton(Quaternion deltaRot)
        {
            animTarget.localRotation = defRotAnim * flipAxes(deltaRot);
        }

        protected static float WrapAngle(float deg)
        {
            return Mathf.DeltaAngle(0, deg);
        }

        protected static Vector3 WrapAngles(Vector3 deg)
        {
            deg.x = WrapAngle(deg.x);
            deg.y = WrapAngle(deg.y);
            deg.z = WrapAngle(deg.z);
            return deg;
        }
    }
}