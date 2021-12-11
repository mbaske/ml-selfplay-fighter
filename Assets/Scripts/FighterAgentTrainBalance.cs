using UnityEngine;
using Unity.MLAgents.Sensors;

// Learn balancing with demo generated from idle/guard animation.
// Uses animated dummy opponent for generating observations.

public class FighterAgentTrainBalance : FighterAgentPhysics
{
    [SerializeField, Tooltip("Target distance between agents")]
    private float m_Distance = 1.15f;
    // Apply random force to body root for training robustness.
    [SerializeField]
    private float m_MaxKickStrength = 13000;
    [SerializeField, Tooltip("Seconds")]
    private float m_MinKickInterval = 3;
    [SerializeField, Tooltip("Seconds")]
    private float m_MaxKickInterval = 5;
    private int m_KickInterval; // steps
    [SerializeField, Tooltip("Steps, must be multiple of decision interval")]
    private const int m_StatsInterval = 60;

    private ArticulationBody m_Hips;


    public override void Initialize()
    {
        base.Initialize();

        m_Hips = m_Root.GetComponent<ArticulationBody>();
        RandomizeKickInterval();
    }

    public override void ManagedReset()
    {
        base.ManagedReset();

        RandomizeKickInterval();
    }

    public override void OnEpisodeStep(int episodeStep)
    {
        base.OnEpisodeStep(episodeStep);

        if (episodeStep % m_KickInterval == 0 && episodeStep > 0)
        {
            m_Hips.AddForce(Vector3.ProjectOnPlane(
                Random.onUnitSphere, Vector3.up) * m_MaxKickStrength);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);

        var m = ObsCtrl.GetOpponentRelativeMetrics();

        // Normalize rewards to 0/+1 range and multiply them so that agent
        // won't optimize for some while neglecting others.
        // This will result in agent roughly mimicking its opponent's pose.

        float distance = 1 - Mathf.Clamp01(Mathf.Abs(m_Distance - m.Distance));
        float angle = Mathf.Pow(1 - Mathf.Abs(m.FacingAngle) / 180, 4);
        float lFoot = Mathf.Pow(1 - Mathf.Clamp01(m.LeftFootOffset), 2);
        float rFoot = Mathf.Pow(1 - Mathf.Clamp01(m.RightFootOffset), 2);
        float lHand = Mathf.Pow(1 - Mathf.Clamp01(m.LeftHandOffset), 2);
        float rHand = Mathf.Pow(1 - Mathf.Clamp01(m.RightHandffset), 2);

        AddReward(distance * angle * lFoot * rFoot * lHand * rHand);

        if (m_EpisodeStep % m_StatsInterval == 0 && m_EpisodeStep > 0)
        {
            m_Stats.Add("Agent/Distance", m.Distance);
            m_Stats.Add("Agent/Angle", m.FacingAngle);
            m_Stats.Add("Agent/Foot Left", m.LeftFootOffset);
            m_Stats.Add("Agent/Foot Right", m.RightFootOffset);
            m_Stats.Add("Agent/Hand Left", m.LeftHandOffset);
            m_Stats.Add("Agent/Hand Right", m.RightHandffset);
        }
    }

    private void RandomizeKickInterval()
    {
        m_KickInterval = Mathf.RoundToInt(
            Random.Range(m_MinKickInterval, m_MaxKickInterval)
            * SettingsProvider.FPS);
    }
}
