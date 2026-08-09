using UnityEngine;
using Unity.MLAgents.Actuators;

public class ManualControl : MonoBehaviour
{
    private AbstractAgent agent;
    private float[] inputPolicy;

    void Awake(){
        agent = GetComponent<AbstractAgent>();
        inputPolicy = new float[agent.NumPawns * 2];
    }

    void Update(){
        // Feed player input into the agent as movement actions.
        Vector2 input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        inputPolicy[0] = input.x;
        inputPolicy[1] = input.y;

        agent.DeployPolicy(new ActionSegment<float>(inputPolicy));
    }    
}