using UnityEngine.UI;
using UnityEngine;

namespace MBaske.Fighter
{
    public class Demo : MonoBehaviour
    {
        [Header("Punch pause")]
        [SerializeField]
        private float minForce = 10f;
        [SerializeField]
        private float duration = 1.5f;
        [SerializeField]
        private float dampTime = 0.2f;
        [SerializeField]
        private float zoomInFactor = 2f;
        [SerializeField]
        private float speedUpFactor = 2f;
        [SerializeField]
        private float heightOffset = 0f;

        [Space, SerializeField]
        private Vector3 lookTarget = Vector3.up;

        [Space, SerializeField]
        private AutoCurriculum curriculum;
        [SerializeField]
        private RotatingCam cam;

        [SerializeField]
        private FighterAgent agentA;
        [SerializeField]
        private FighterAgent agentB;

        [SerializeField]
        private Outline leftHandA;
        [SerializeField]
        private Outline leftHandB;
        [SerializeField]
        private Outline rightHandA;
        [SerializeField]
        private Outline rightHandB;

        [SerializeField]
        private ProgressBar pbForceA;
        [SerializeField]
        private ProgressBar pbForceB;
        [SerializeField]
        private ProgressBar pbDownA;
        [SerializeField]
        private ProgressBar pbDownB;

        [SerializeField]
        private Text textPunchA;
        [SerializeField]
        private Text textPunchB;
        [SerializeField]
        private Text textWinCountA;
        [SerializeField]
        private Text textWinCountB;
        [SerializeField]
        private Text textDownCountA;
        [SerializeField]
        private Text textDownCountB;

        [SerializeField]
        private Text textCrntMax;

        private bool isPaused;
        private float endPauseThresh;
        private float pauseStartTime;
        private float timeSincePauseStart => Time.time - pauseStartTime;

        private float distDamp;
        private float speedDamp;
        private float latDamp;
        private Vector3 lookDamp;
        private float dampThresh;
        private Vector3 tmpLookTarget;
        private Rigidbody[] rigidbodies;
        private PausableDecisionRequester[] decisionRequesters;

        private void Awake()
        {
            DisableShowPunch();

            agentA.OnValidPunch += OnValidPunchA;
            agentB.OnValidPunch += OnValidPunchB;

            dampThresh = duration - dampTime;
            rigidbodies = FindObjectsOfType<Rigidbody>();
            decisionRequesters = FindObjectsOfType<PausableDecisionRequester>();
        }

        private void OnDestroy()
        {
            agentA.OnValidPunch -= OnValidPunchA;
            agentB.OnValidPunch -= OnValidPunchB;
        }

        private void OnValidPunchA(OpponentCollision collision)
        {
            if (IsPausablePunch(collision, out float force))
            {
                textPunchA.text = $"FORCE {Round(force)}";
                textPunchA.enabled = true;
                if (collision.bodyPartA.Side == BodyPartSide.Left)
                {
                    leftHandA.enabled = true;
                }
                else
                {
                    rightHandA.enabled = true;
                }

                Pause(collision);
            }
        }

        private void OnValidPunchB(OpponentCollision collision)
        {
            if (IsPausablePunch(collision, out float force))
            {
                textPunchB.text = $"FORCE {Round(force)}";
                textPunchB.enabled = true;
                if (collision.bodyPartA.Side == BodyPartSide.Left)
                {
                    leftHandB.enabled = true;
                }
                else
                {
                    rightHandB.enabled = true;
                }

                Pause(collision);
            }
        }

        private bool IsPausablePunch(OpponentCollision collision, out float force)
        {
            force = collision.velocity.magnitude;
            // TODO latency / hand position issue.
            return timeSincePauseStart > duration * 2f &&
                force >= minForce &&
                collision.IsUpperBodyHit() &&
                (collision.bodyPartA.transform.position -
                collision.bodyPartB.transform.position).magnitude < 0.4f;
        }

        private void Pause(OpponentCollision collision)
        {
            tmpLookTarget = collision.bodyPartA.transform.position;

            foreach (var rb in rigidbodies)
            {
                rb.Sleep();
            }
            foreach (var dr in decisionRequesters)
            {
                dr.IsPaused = true;
            }

            isPaused = true;
            pauseStartTime = Time.time;
            Invoke("EndPause", duration);
        }

        private void EndPause()
        {
            isPaused = false;
            DisableShowPunch();

            foreach (var dr in decisionRequesters)
            {
                dr.IsPaused = false;
            }
        }

        private void DisableShowPunch()
        {
            textPunchA.enabled = false;
            textPunchB.enabled = false;
            leftHandA.enabled = false;
            leftHandB.enabled = false;
            rightHandA.enabled = false;
            rightHandB.enabled = false;
        }

        private void Update()
        {
            UpdateInfo();

            if (isPaused)
            {
                bool b = timeSincePauseStart < dampThresh;
                cam.LookTarget = Vector3.SmoothDamp(cam.LookTarget,
                        b ? tmpLookTarget : lookTarget, ref lookDamp, dampTime);
                cam.DistanceFactor = Mathf.SmoothDamp(cam.DistanceFactor,
                        b ? 1f / zoomInFactor : 1f, ref distDamp, dampTime);
                cam.SpeedFactor = Mathf.SmoothDamp(cam.SpeedFactor,
                        b ? speedUpFactor : 0f, ref speedDamp, dampTime);
                cam.HeightOffset = Mathf.SmoothDamp(cam.HeightOffset,
                        b ? heightOffset : 0f, ref latDamp, dampTime);
            }
            else
            {
                cam.LookTarget = lookTarget;
                cam.DistanceFactor = 1f;
                cam.SpeedFactor = 1f;
                cam.HeightOffset = 0f;
            }
        }

        private void UpdateInfo()
        {
            float thresh = Round(curriculum.MaxAccumForce);
            float time = Round(curriculum.MaxDownSteps * Time.fixedDeltaTime, 2);
            textCrntMax.text = $"Req. Acc. Force: {thresh}\nMax. Down Time: {time}";

            textWinCountA.text = Leading0(agentA.WinCount);
            textWinCountB.text = Leading0(agentB.WinCount);
            textDownCountA.text = Leading0(agentA.DownCount);
            textDownCountB.text = Leading0(agentB.DownCount);

            pbForceA.SetValue(Round(agentA.AccumForce, 0).ToString(), agentA.AccumForce / thresh);
            pbForceB.SetValue(Round(agentB.AccumForce, 0).ToString(), agentB.AccumForce / thresh);

            if (agentA.IsDown)
            {
                pbDownA.SetValue(Round(agentA.DownStepCount * Time.fixedDeltaTime, 2).ToString(),
                    agentA.DownStepCount / (float)curriculum.MaxDownSteps);
            }
            else
            {
                pbDownA.SetValue("", 0);
            }
            if (agentB.IsDown)
            {
                pbDownB.SetValue(Round(agentB.DownStepCount * Time.fixedDeltaTime, 2).ToString(),
                    agentB.DownStepCount / (float)curriculum.MaxDownSteps);
            }
            else
            {
                pbDownB.SetValue("", 0);
            }
        }

        private static float Round(float val, int dec = 1)
        {
            float d = Mathf.Pow(10, dec);
            return Mathf.Round(val * d) / d;
        }

        private static string Leading0(int val)
        {
            return val > 0 ? (val < 10 ? "0" + val.ToString() : val.ToString()) : "";
        }
    }
}