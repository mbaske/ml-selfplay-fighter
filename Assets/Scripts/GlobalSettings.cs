using UnityEngine;

namespace MBaske.Fighter
{
    public class GlobalSettings : MonoBehaviour
    {
        [SerializeField]
        private float timeScale = -1;
        [SerializeField]
        private float drag = 3;
        [SerializeField]
        private float angularDrag = 0.5f;
        [SerializeField]
        private int solverIterations = 64;
        [SerializeField]
        private int solverVelocityIterations = 8;

        private Rigidbody[] rigidbodies;

        private void Awake()
        {
            ApplySettings();
        }

        private void OnValidate()
        {
            ApplySettings();
        }

        private void ApplySettings()
        {
            if (timeScale != -1)
            {
                Time.timeScale = timeScale;
            }

            if (rigidbodies == null)
            {
                rigidbodies = FindObjectsOfType<Rigidbody>();
            }

            foreach (Rigidbody rb in rigidbodies)
            {
                rb.drag = drag;
                rb.angularDrag = angularDrag;
                rb.solverIterations = solverIterations;
                rb.solverVelocityIterations = solverVelocityIterations;
            }
        }
    }
}