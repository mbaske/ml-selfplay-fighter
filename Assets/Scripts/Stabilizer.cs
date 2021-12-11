using UnityEngine;

public class Stabilizer : MonoBehaviour
{
    [SerializeField]
    private float m_Strength = 0;

    private ArticulationBody m_Hips;


    private void Awake()
    {
        if (m_Strength > 0)
        {
            m_Hips = GetComponent<ArticulationBody>();
        }
        else
        {
            enabled = false;
        }
    }

    private void FixedUpdate()
    {
        Vector3 up = transform.up;
        float x = Vector3.SignedAngle(up, Vector3.up, Vector3.forward);
        float z = Vector3.SignedAngle(up, Vector3.up, Vector3.right);

        m_Hips.AddTorque(
            x * m_Strength * Vector3.forward +
            z * m_Strength * Vector3.right);
    }
}
