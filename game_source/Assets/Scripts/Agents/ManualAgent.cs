using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using UnityEngine;

public class ManualAgent : AbstractAgent
{
    // Apply manually supplied movement inputs to the agent.
    public void ManualStep(List<float> inputs)
    {
        int expectedSize = NumPawns * 2;

        if (inputs == null || inputs.Count < expectedSize)
            return;

        // Convert List<float> → ActionSegment<float>-compatible array usage
        float[] arr = new float[expectedSize];
        for (int i = 0; i < expectedSize; i++)
        {
            arr[i] = inputs[i];
        }

        DeployPolicy(new ActionSegment<float>(arr));
    }
}