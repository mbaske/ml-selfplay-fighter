using UnityEngine;

public class ObservationTest : MonoBehaviour
{
    [SerializeField]
    private ObservationController m_ObservationControllerA;
    [SerializeField]
    private ObservationController m_ObservationControllerB;

    private void Awake()
    {
        m_ObservationControllerA.Initialize();
        m_ObservationControllerB.Initialize();
    }

    private void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;
        m_ObservationControllerA.ManagedUpdate(deltaTime);
        m_ObservationControllerB.ManagedUpdate(deltaTime);
    }
}
