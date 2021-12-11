using UnityEngine;

public class FighterAgentAnimated : FighterAgentBase
{
    [SerializeField]
    private string[] m_Clips;
    private int m_ClipIndex;
    private Animator m_Animator;


    public override void Initialize()
    {
        base.Initialize();

        m_Animator = GetComponent<Animator>();
    }

    public override void OnEpisodeStep(int episodeStep)
    {
        base.OnEpisodeStep(episodeStep);

        if (episodeStep == 0)
        {
            PlayAnimation();
        }
    }

    public override void PrepareEndEpisode()
    {
        //m_Animator.Play("Base Layer.T-Pose", 0);
        m_Animator.StopPlayback();
    }

    private void PlayAnimation()
    {
        m_Animator.Play($"Base Layer.{m_Clips[m_ClipIndex]}", 0);
        m_ClipIndex++;
        m_ClipIndex %= m_Clips.Length;
    }
}
    