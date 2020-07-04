using UnityEngine;

namespace MBaske.Fighter
{
    public enum EpisodeStop
    {
        Win = 0,
        Lose = 1,
        Draw = 2
    }

    public class AutoCurriculum : MonoBehaviour
    {
        [Header("Initial values (updated at runtime)")]
        public float MaxDownSteps = 1;
        public float MaxAccumForce = 1;

        [Header("Counters (readonly)")]
        [SerializeField]
        private int episodeCount;
        [SerializeField]
        private int winCount;
        [SerializeField]
        private int loseCount;
        [SerializeField]
        private int drawCount;

        private float lerpUpdate = 1f;
        private FighterAgent[] agents;

        private void Awake()
        {
            agents = FindObjectsOfType<FighterAgent>();
            foreach (FighterAgent agent in agents)
            {
                agent.OnEpisodeStop += OnEpisodeStop;
            }
            UpdateAgents();
        }

        private void OnDestroy()
        {
            foreach (FighterAgent agent in agents)
            {
                agent.OnEpisodeStop -= OnEpisodeStop;
            }
        }

        private void UpdateAgents()
        {
            foreach (FighterAgent agent in agents)
            {
                agent.MaxDownSteps = (int)MaxDownSteps;
                agent.MaxAccumForce = MaxAccumForce;
            }
        }

        private void OnEpisodeStop(EpisodeStop reason)
        {
            episodeCount++;
            switch (reason)
            {
                case EpisodeStop.Win:
                    winCount++;
                    break;
                case EpisodeStop.Lose:
                    loseCount++;
                    break;
                case EpisodeStop.Draw:
                    drawCount++;
                    break;
            }

            MaxDownSteps = Mathf.Lerp(
                MaxDownSteps,
                drawCount < loseCount
                    ? MaxDownSteps + 1
                    : Mathf.Max(1, MaxDownSteps - 1),
                lerpUpdate
            );

            MaxAccumForce = Mathf.Lerp(
                MaxAccumForce,
                winCount > episodeCount * 0.33f
                    ? MaxAccumForce + 1
                    : Mathf.Max(1, MaxAccumForce - 1),
                lerpUpdate
            );

            UpdateAgents();
        }
    }
}