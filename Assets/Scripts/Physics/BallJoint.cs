using UnityEngine;
using System.Collections.Generic;

namespace MBaske.Fighter
{
    public class BallJoint : BodyPart
    {
        public bool[] DOF { get; private set; }

        [SerializeField, Tooltip("Rotation range for actions < 0 (positive)")]
        private Vector3 min;
        [SerializeField, Tooltip("Rotation range for actions > 0 (positive)")]
        private Vector3 max;
        private Vector3 normRot;
        
        [Header("Defaults, 0/0/0 for T-pose")]
        [SerializeField, Range(-1f, 1f)]
        private float x;
        [SerializeField, Range(-1f, 1f)]
        private float y;
        [SerializeField, Range(-1f, 1f)]
        private float z;

        private Quaternion crntRot => Quaternion.Inverse(transform.localRotation) * connected.localRotation;
        private Quaternion deltaRot => Quaternion.Inverse(crntRot) * defRotSelf;

        private ConfigurableJoint joint;
        private Transform connected;

        public override void Initialize()
        {
            base.Initialize();

            joint = GetComponent<ConfigurableJoint>();
            joint.anchor = transform.InverseTransformPoint(animTarget.position);
            connected = joint.connectedBody.transform;

            DOF = new bool[3] {
                joint.angularXMotion > 0,
                joint.angularYMotion > 0,
                joint.angularZMotion > 0
            };
        }

        public override void OnReset()
        {
            base.OnReset();

            defRotSelf = crntRot;
            normRot = new Vector3(x, y, z);
        }

        // Transform rotation.
        public void AddNormRotationTo(List<float> obs)
        {
            Vector3 norm = WrapAngles(deltaRot.eulerAngles) / 180f;
            obs.Add(norm.x);
            obs.Add(norm.y);
            obs.Add(norm.z);
        }

        // Joint rotation.
        public void SetNormRotation(float[] norm, ref int index)
        {
            if (DOF[0])
            {
                normRot.x = norm[index++];
            }
            if (DOF[1])
            {
                normRot.y = norm[index++];
            }
            if (DOF[2])
            {
                normRot.z = norm[index++];
            }
        }

        public void ApplyRotation()
        {
            joint.targetRotation = Quaternion.Euler(new Vector3(
                normRot.x * (normRot.x > 0 ? max.x : min.x),
                normRot.y * (normRot.y > 0 ? max.y : min.y),
                normRot.z * (normRot.z > 0 ? max.z : min.z)
            ));
        }

        private void Update()
        {
            AnimateSkeleton(deltaRot);
        }

        private void OnValidate()
        {
            if (DOF != null && DOF.Length == 3)
            {
                min.x = DOF[0] ? Mathf.Max(0, min.x) : 0;
                min.y = DOF[1] ? Mathf.Max(0, min.y) : 0;
                min.z = DOF[2] ? Mathf.Max(0, min.z) : 0;
                max.x = DOF[0] ? Mathf.Max(0, max.x) : 0;
                max.y = DOF[1] ? Mathf.Max(0, max.y) : 0;
                max.z = DOF[2] ? Mathf.Max(0, max.z) : 0;
                x = Mathf.Clamp(x, min.x > 0 ? -1 : 0, max.x > 0 ? 1 : 0);
                y = Mathf.Clamp(y, min.y > 0 ? -1 : 0, max.y > 0 ? 1 : 0);
                z = Mathf.Clamp(z, min.z > 0 ? -1 : 0, max.z > 0 ? 1 : 0);
                normRot = new Vector3(x, y, z);
            }
        }
    }
}