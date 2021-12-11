using UnityEngine;
using Unity.MLAgents.Policies;
using Unity.Barracuda;

public class FighterAgentInference : FighterAgentPhysics
{
    [SerializeField]
    private NNModel[] m_Models;

    public override void Initialize()
    {
        base.Initialize();
        RandomizeModel();
    }

    public override void ManagedReset()
    {
        base.ManagedReset();
        RandomizeModel();
    }

    private void RandomizeModel()
    {
        GetComponent<BehaviorParameters>().Model
            = m_Models[Random.Range(0, m_Models.Length)];
    }
}
