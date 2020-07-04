using UnityEngine;

namespace MBaske
{
    public class RotatingCam : MonoBehaviour
    {
        public float SpeedFactor { get; set; } = 1;
        public float DistanceFactor { get; set; } = 1;
        public float HeightOffset { get; set; }
        public Vector3 LookTarget;
        [SerializeField]
        private Vector2 offset;
        [SerializeField]
        private Vector3 center;

        [System.Serializable]
        private struct Modulator
        {
            public float @default;
            public float range;
            public float speed;
            [HideInInspector]
            public float val;

            public float Evaluate()
            {
                return @default + Mathf.Sin(val) * range;
            }
        }

        [SerializeField]
        private float longitudeSpeed;
        private float longitude;
        [SerializeField]
        private Modulator latitude;
        [SerializeField]
        private Modulator distance;

        private void LateUpdate()
        {
            float dt = Time.deltaTime;
            longitude += dt * longitudeSpeed * SpeedFactor;
            latitude.val += dt * latitude.speed;
            distance.val += dt * distance.speed;
            Vector3 fwd = Vector3.forward * distance.Evaluate() * DistanceFactor;
            transform.position = center + Quaternion.Euler(-latitude.Evaluate(), longitude, 0)
                * fwd + Vector3.up * HeightOffset;
            transform.LookAt(LookTarget + transform.right * offset.x + transform.up * offset.y);
        }
    }
}