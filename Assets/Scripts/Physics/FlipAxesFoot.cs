using UnityEngine;

namespace MBaske.Fighter
{
    public class FlipAxesFoot : FlipAxes
    {
        public override Quaternion Flip(Quaternion q)
        {
            return new Quaternion(
                -q.x,
                q.z,
                q.y,
                q.w
            );
        }
    }
}