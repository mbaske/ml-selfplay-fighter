using UnityEngine;

public class FighterAgentTrainSelfPlay : FighterAgentPhysics
{
    [HideInInspector]
    public float CmlPunchVelocity;
    [HideInInspector]
    public int PunchCount;
    [HideInInspector]
    public int BlockCount;


    public override void Initialize()
    {
        base.Initialize();

        var hands = GetComponentsInChildren<Hand>();
        hands[0].PunchEvent += HandlePunch;
        hands[1].PunchEvent += HandlePunch;
        hands[0].BlockEvent += HandleBlock;
        hands[1].BlockEvent += HandleBlock;
    }

    public override void ManagedReset()
    {
        base.ManagedReset();

        CmlPunchVelocity = 0;
        PunchCount = 0;
        BlockCount = 0;
    }

    public override void PrepareEndEpisode()
    {
        float sec = m_EpisodeStep / (float)SettingsProvider.FPS;
        m_Stats.Add("Agent/Punch Frequency", PunchCount / sec);
        m_Stats.Add("Agent/Block Frequency", BlockCount / sec);
    }

    private void HandlePunch(Collision collision, Hand hand)
    {
        PunchCount++;
        float strength = collision.relativeVelocity.magnitude;
        CmlPunchVelocity += strength;
        m_Stats.Add("Agent/Punch Velocity", strength);
    }

    // Hitting arms or hands is considered opponent blocking.
    private void HandleBlock(Collision collision, Hand hand)
    {
        BlockCount++;
        float strength = collision.relativeVelocity.magnitude;
        m_Stats.Add("Agent/Block Velocity", strength);
    }
}
