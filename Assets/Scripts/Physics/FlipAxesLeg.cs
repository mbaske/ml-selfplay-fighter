using UnityEngine;

namespace MBaske.Fighter
{
    public class FlipAxesLeg : FlipAxes
    {
        public override Quaternion Flip(Quaternion q)
        {
            return new Quaternion(
                -q.x,
                -q.y,
                q.z,
                q.w
            );
        }
    }
}