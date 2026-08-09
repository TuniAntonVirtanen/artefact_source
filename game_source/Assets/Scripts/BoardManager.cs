using UnityEngine;
using Unity.MLAgents;
using System.Collections.Generic;   // Contains dict
using Random = System.Random;


public class BoardManager : MonoBehaviour
{
    [SerializeField] private GameObject puck;
    [SerializeField] private GameManager gameManager;
    
    private AbstractAgent leftAgent;
    private AbstractAgent rightAgent;
    
    private Dictionary<TeamSide, AbstractAgent> agentSides;
    private Rigidbody2D puckRb;

    void Start(){
        puckRb = puck.GetComponent<Rigidbody2D>();
    }

    // Set up both agents for a new match.
    public void Startmatch(AbstractAgent left, AbstractAgent right){
        leftAgent = left;
        rightAgent = right;

        // Assign field sides for the agents (Left or Right)
        leftAgent.Initiate(TeamSide.Left, puck.transform, this);
        rightAgent.Initiate(TeamSide.Right, puck.transform, this);

        // To hold reference on enemy transforms for gathering observations
        if (leftAgent != null && rightAgent != null){
            leftAgent.SetEnemyPawnTransforms(rightAgent.GetPawnTransforms());
            rightAgent.SetEnemyPawnTransforms(leftAgent.GetPawnTransforms());
        }
    }

    // End the current match and clean up both agents.
    public void EndMatch(){
        ResetBoard();
        leftAgent.Kill();
        rightAgent.Kill();
        leftAgent = null;
        rightAgent = null;
    }

    public void GoalScored(TeamSide teamSide){        
        gameManager.TeamScored(teamSide);    
        ResetBoard();
    }

    // Reset the puck and both agents to their starting positions.
    public void ResetBoard(){   
        Rigidbody2D rb = puck.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.inertia = 0f;

        // Randomize puck starting position, center around -1, 0 (left player starts)
        float randomX = UnityEngine.Random.Range(-2.2f, -2.0f);
        float randomY = UnityEngine.Random.Range(-0.1f, 0.1f);
        puck.transform.position = new Vector2(randomX, randomY);

        leftAgent.Reset();
        rightAgent.Reset();
    }

    // End the current training episode.
    public void ResetGame(){
        leftAgent.EndEpisode();
        ResetBoard();
    }
}