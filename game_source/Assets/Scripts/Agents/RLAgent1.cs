using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

using UnityEngine;

// Applied only as template for GameObject
public class RLAgent1 : AbstractAgent
{
    public override void CollectObservations(VectorSensor sensor){
        GatherObservations(sensor);
    }

    public override void OnActionReceived(ActionBuffers actions){
        pawns[0].SetMovement(new Vector2(actions.ContinuousActions[0], actions.ContinuousActions[1]));    
    }
}
