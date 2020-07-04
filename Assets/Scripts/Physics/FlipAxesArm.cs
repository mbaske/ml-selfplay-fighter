using UnityEngine;

namespace MBaske.Fighter
{
    public class FlipAxesArm : FlipAxes
    {
        public override Quaternion Flip(Quaternion q)
        {
            return new Quaternion(
                q.z * -xSign,
                q.x * xSign, 
                -q.y,
                q.w
            );
        }
    }
}