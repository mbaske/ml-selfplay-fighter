using UnityEngine;

namespace MBaske.Fighter
{
    public class Observables : MonoBehaviour
    {
        public Vector3 Velocity => rb.velocity;
        public Vector3 AngularVelocity => rb.angularVelocity;
        public Vector3 Position => transform.TransformPoint(com);
        public Vector3 Forward => transform.forward;

        private Rigidbody rb;
        private Vector3 com;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            com = rb.centerOfMass;
        }
    }
}