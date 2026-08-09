using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class HumanAgent : AbstractAgent
{
    protected override bool UseMirroring => false;
    
    // Collect the current game state for the policy.
    public override void CollectObservations(VectorSensor sensor)
    {
        GatherObservations(sensor);
    }

    // Apply the chosen actions to the controlled pawns.
    public override void OnActionReceived(ActionBuffers actions)
    {
        DeployPolicy(actions.ContinuousActions);
    }

    // Generate actions from player keyboard/controller input.
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;

        // 1. Clear actions
        for (int i = 0; i < actions.Length; i++)
            actions[i] = 0f;

        // 2. Keep the existing keyboard layout logic intact
        if (teamSide == TeamSide.Left)
        {
            // Pawn 1 (WASD)
            actions[0] = GetAxis(KeyCode.A, KeyCode.D);
            actions[1] = GetAxis(KeyCode.S, KeyCode.W);

            if (NumPawns > 1)
            {
                // Pawn 2 (GYJH)
                actions[2] = GetAxis(KeyCode.G, KeyCode.J);
                actions[3] = GetAxis(KeyCode.H, KeyCode.Y);
            }
        }
        else
        {
            // Pawn 1 (Arrow keys)
            actions[0] = GetAxis(KeyCode.LeftArrow, KeyCode.RightArrow);
            actions[1] = GetAxis(KeyCode.DownArrow, KeyCode.UpArrow);

            if (NumPawns > 1)
            {
                // Pawn 2 (Numpad)
                actions[2] = GetAxis(KeyCode.Keypad1, KeyCode.Keypad3);
                actions[3] = GetAxis(KeyCode.Keypad2, KeyCode.Keypad5);
            }
        }

        // 3. Global Controller Support Layer
        // Read Left Stick (Pawn 1)
        float ctrlX1 = Input.GetAxis("Horizontal"); 
        float ctrlY1 = -Input.GetAxis("Vertical");   

        // Use += so it blends seamlessly with the keyboard inputs
        if (Mathf.Abs(ctrlX1) > 0.1f) actions[0] += ctrlX1;
        if (Mathf.Abs(ctrlY1) > 0.1f) actions[1] += ctrlY1;

        if (NumPawns > 1)
        {
            // Read Right Stick (Pawn 2) - Native PS4 Windows mapping (3rd and 6th axis)
            float ctrlX2 = Input.GetAxis("RightStickX"); 
            float ctrlY2 = -Input.GetAxis("RightStickY");   

            if (Mathf.Abs(ctrlX2) > 0.1f) actions[2] += ctrlX2;
            if (Mathf.Abs(ctrlY2) > 0.1f) actions[3] += ctrlY2;
        }
    }

    private float GetAxis(KeyCode negative, KeyCode positive)
    {
        float value = 0f;

        if (Input.GetKey(negative))
            value -= 1f;

        if (Input.GetKey(positive))
            value += 1f;

        return value;
    }
}