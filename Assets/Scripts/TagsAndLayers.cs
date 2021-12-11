using UnityEngine;

public class TagsAndLayers : MonoBehaviour
{
    public static string AgentA = "AgentA";
    public static string BlockA = "BlockA";
    public static string TargetA = "TargetA";

    public static string AgentB = "AgentB";
    public static string BlockB = "BlockB";
    public static string TargetB = "TargetB";
    
    public static string Foot = "Foot";
    public static string BoxingRing = "BoxingRing";
    public static string Ground = "Ground";
    public static string Walls = "Walls";

    public static int LayerA = 6;
    public static int LayerA_NoSelfCollision = 7;
    public static int LayerB = 8;
    public static int LayerB_NoSelfCollision = 9;

    public string BlockTag => CompareTag(AgentA)
        ? BlockB
        : BlockA;

    public string TargetTag => CompareTag(AgentA)
        ? TargetB
        : TargetA;

    public int LayerMask => CompareTag(AgentA)
        ? (1 << LayerB) | (1 << LayerB_NoSelfCollision)
        : (1 << LayerA) | (1 << LayerA_NoSelfCollision);


    private void Awake()
    {
        if (CompareTag(AgentB))
        {
            var list = GetComponentsInChildren<Transform>();

            foreach (var tf in list)
            {
                var go = tf.gameObject;

                if (go.CompareTag(BlockA))
                {
                    go.tag = BlockB;
                }
                else if (go.CompareTag(TargetA))
                {
                    go.tag = TargetB;
                }

                if (go.layer == LayerA)
                {
                    go.layer = LayerB;
                }
                else if (go.layer == LayerA_NoSelfCollision)
                {
                    go.layer = LayerB_NoSelfCollision;
                }
            }
        }
    }
}
