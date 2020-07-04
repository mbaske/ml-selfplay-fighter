using UnityEngine;

namespace MBaske.Fighter
{
    public class BodyRoot : BodyPart
    {
        private Vector3 defPosAnim;
        private Vector3 defPosSelf;

        public override void OnReset()
        {
            base.OnReset();

            defPosAnim = animTarget.position;
            defPosSelf = transform.position;
            defRotSelf = transform.rotation;
        }

        public Vector3 Localize(Vector3 v)
        {
            return transform.InverseTransformVector(v);
        }

        private void Update()
        {
            AnimateSkeleton(Quaternion.Inverse(transform.rotation) * defRotSelf);
            animTarget.position = defPosAnim + (transform.position - defPosSelf);
        }
    }
}