using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

using UnityEngine;

public class AIAgent : AbstractAgent
{
    // Collect the current game state for the policy.
    public override void CollectObservations(VectorSensor sensor){
        GatherObservations(sensor);
    }

    // Apply the policy output to the controlled pawns.
    public override void OnActionReceived(ActionBuffers actions){
        DeployPolicy(actions.ContinuousActions);
    }
}
