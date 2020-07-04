using UnityEngine;

namespace MBaske.Fighter
{
    // Used for demo, agents always face each other.
    public class LookAtTarget : MonoBehaviour
    {
        [SerializeField]
        private Transform lookTarget;

        private void FixedUpdate()
        {
            transform.rotation = Quaternion.LookRotation(lookTarget.position - transform.position);
        }
    }
}