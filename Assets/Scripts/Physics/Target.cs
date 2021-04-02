using UnityEngine;

namespace MBaske.SelfPlayFighter
{
    public class Target : VelocityTracker
    {
        public override void SetSelfPlay(bool selfPlay, int teamID)
        {
            gameObject.tag = "Target" + (teamID == 0 ? "A" : "B");
        }
    }
}