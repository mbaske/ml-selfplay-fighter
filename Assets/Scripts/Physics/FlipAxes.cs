using UnityEngine;

namespace MBaske.Fighter
{
    public abstract class FlipAxes : MonoBehaviour
    {
        protected float xSign;

        private void Awake()
        {
            xSign = Mathf.Sign(transform.localPosition.x);
        }

        public virtual Quaternion Flip(Quaternion q)
        {
            return q;
        }
    }
}