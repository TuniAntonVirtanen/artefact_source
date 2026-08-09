using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

using UnityEngine;
using System.Collections.Generic;

public abstract class AbstractAgent : Agent
{
    [SerializeField] private Pawn pawnPrefab;
    [SerializeField] private int numPawns;

    private BoardManager boardManager;
    private List<Transform> pawnTransforms = new List<Transform>();
    private List<Transform> enemyPawnTransforms = new List<Transform>();
    private List<float> observations = new List<float>();    
    private float Xoffset = 2.5f;     // How far from center point towards goal
    private float Yoffset = 1.2f;     // Has to be less than 0 to upper wall
    private Transform gameBoardTransform;
    private Transform puck;

    protected List<Pawn> pawns = new List<Pawn>();
    private protected TeamSide teamSide;
    
    // Mirror observations/actions so one policy works for both sides.
    protected virtual bool UseMirroring => true;
    public bool IsMirrored => UseMirroring && teamSide == TeamSide.Right;
    
    // --- SHOULDER-TO-SHOULDER CONFIGURATION ---
    // 0.2f is spacing between agents.
    private const float PawnDiameter = 0.2f; 
    public int NumPawns => numPawns;

    public void SetNumPawns(int num){
        Debug.Log($"Pawns set to {num}");
        numPawns = num;
    }

    public virtual void Initiate(TeamSide side, Transform gamePuck, BoardManager boardManager){
        this.boardManager = boardManager;
        this.gameBoardTransform = boardManager.transform;
        pawns.Clear();
        pawnTransforms.Clear();     
        puck = gamePuck;

        teamSide = side;
        float pawnX = (side == TeamSide.Right) ? Xoffset : Xoffset * -1 ;

        // Calculate start position to keep the packed group perfectly centered on Y = 0
        float startY = -((numPawns - 1) * PawnDiameter) / 2f;

        for (int i = 0; i < numPawns; i++){
            float pawnY = startY + (i * PawnDiameter);

            Vector2 originalPosition = new Vector2(pawnX, pawnY);

            Pawn pawn = Instantiate(pawnPrefab);
            pawn.transform.SetParent(gameBoardTransform, false);
            pawn.transform.position = originalPosition;
            pawn.SetOriginalPosition(originalPosition);
            pawn.SetColor(side);
            pawn.SetOwner(this);
            pawns.Add(pawn);        
            pawnTransforms.Add(pawn.transform);            
        }
    }
    
    public void SetSide(TeamSide side){
        teamSide = side;

        float pawnX = (side == TeamSide.Right) ? Xoffset : Xoffset * -1 ;
        
        // Calculate start position to keep the packed group perfectly centered on Y = 0
        float startY = -((pawns.Count - 1) * PawnDiameter) / 2f;

        for(int i = 0; i < pawns.Count; ++i){
            float pawnY = startY + (i * PawnDiameter);
            Vector2 originalPosition = new Vector2(pawnX, pawnY);
            pawns[i].SetOriginalPosition(originalPosition);
            pawns[i].SetColor(side);
        }
    }

    // Reset all controlled pawns.
    public void Reset(){
        foreach (Pawn pawn in pawns){
            pawn.Reset();
        }        
    }

    // Convert policy outputs into pawn movement.
    public virtual void DeployPolicy(ActionSegment<float> inputs){
        int j = 0;

        for (int i = 0; i < numPawns; i++)
        {
            float actionX = inputs[j];
            float actionY = inputs[j + 1];

            if (IsMirrored)
            {
                actionX *= -1f;
            }

            pawns[i].SetMovement(new Vector2(actionX, actionY));

            j += 2;
        }
    }

    // Gather observations for the ML policy.
    public virtual void GatherObservations(VectorSensor sensor){
        float mirror = IsMirrored ? -1f : 1f;
        float arenaWidth = 3.0f;
        float arenaHeight = 1.5f;
        float corner2corner = 6.71f;
        float maxPawnSpeed = 1200f;
        float maxPuckSpeed = 2500f;
        
        // Own pawn observations
        foreach (Transform pawnTransform in pawnTransforms)
        {
            Rigidbody2D pawnRb = pawnTransform.GetComponent<Rigidbody2D>();

            // Position
            sensor.AddObservation((pawnTransform.position.x * mirror) / arenaWidth);
            sensor.AddObservation(pawnTransform.position.y / arenaHeight);

            // Velocity
            sensor.AddObservation((pawnRb.linearVelocity.x * mirror) / maxPawnSpeed);
            sensor.AddObservation(pawnRb.linearVelocity.y / maxPawnSpeed);

            // Relative puck position
            Vector2 relativePuck = (Vector2)(puck.position - pawnTransform.position);

            sensor.AddObservation((relativePuck.x * mirror) / (arenaWidth * 2));
            sensor.AddObservation(relativePuck.y / arenaHeight);

            // Distance to puck
            float puckDistance = relativePuck.magnitude;
            sensor.AddObservation(puckDistance / corner2corner);
        }

        // Enemy pawn observations
        foreach (Transform enemyTransform in enemyPawnTransforms)
        {
            Rigidbody2D enemyRb = enemyTransform.GetComponent<Rigidbody2D>();

            // Position
            sensor.AddObservation((enemyTransform.position.x * mirror) / arenaWidth);
            sensor.AddObservation(enemyTransform.position.y / arenaHeight);

            // Velocity
            sensor.AddObservation((enemyRb.linearVelocity.x * mirror) / maxPawnSpeed);
            sensor.AddObservation(enemyRb.linearVelocity.y / maxPawnSpeed);
        }

        // Puck observations
        Rigidbody2D puckRb = puck.GetComponent<Rigidbody2D>();

        // Position
        sensor.AddObservation((puck.position.x * mirror) / arenaWidth);
        sensor.AddObservation(puck.position.y / arenaHeight);

        // Velocity
        sensor.AddObservation((puckRb.linearVelocity.x * mirror) / maxPuckSpeed);
        sensor.AddObservation(puckRb.linearVelocity.y / maxPuckSpeed);

        // --- GENERALIZED STATE ANCHORS ---
        float puckXLocal = (puck.position.x * mirror);

        // Anchor 1: Is the puck in defense zone? (1 = Yes, 0 = No)
        float puckOnMySideFlag = (puckXLocal < 0f) ? 1f : 0f;
        sensor.AddObservation(puckOnMySideFlag);

        // Anchor 2: Puck is behind ALL pawns? (1 = Yes, 0 = No)
        float yieldingFlag = 0f;
        if (puckOnMySideFlag == 1f && pawnTransforms.Count > 0)
        {
            bool puckBehindAll = true;
            foreach (Transform pawnTransform in pawnTransforms)
            {
                float pawnXLocal = pawnTransform.position.x * mirror;
                if (puckXLocal >= pawnXLocal)
                {
                    puckBehindAll = false;
                    break;
                }
            }
            yieldingFlag = puckBehindAll ? 1f : 0f;
        }
        sensor.AddObservation(yieldingFlag);
    }

    public virtual List<Transform> GetPawnTransforms(){
        return pawnTransforms;
    }

    public virtual void SetEnemyPawnTransforms(List<Transform> transforms){
        enemyPawnTransforms = transforms;
    }

    // Destroy all spawned pawns.
    public void Kill(){
        foreach (Pawn pawn in pawns){
            Destroy(pawn.gameObject);
        }       
    }
    
    //----- MLAgents toolkit methods
    public override void OnEpisodeBegin(){      
        boardManager.ResetBoard();
    }

    // Called when one of this agent's pawns hits the puck.
    public virtual void OnPuckHit(Vector2 puckVelocity){}

    // Return all pawn positions from this agent's perspective.
    public List<Vector2> GetPerspectivePawnPositions()
    {
        List<Vector2> localPositions = new List<Vector2>();
        float mirror = IsMirrored ? -1f : 1f;

        foreach (Transform pawnTransform in pawnTransforms)
        {
            if (pawnTransform != null)
            {
                Vector3 rawPos = pawnTransform.position;
                localPositions.Add(new Vector2(rawPos.x * mirror, rawPos.y));
            }
        }

        return localPositions;
    }

    public Vector2 GetPerspectivePosition()
    {
        if (pawnTransforms.Count == 0) return Vector2.zero;
        
        Vector2 rawPos = pawnTransforms[0].position;
        float mirror = IsMirrored ? -1f : 1f;
        return new Vector2(rawPos.x * mirror, rawPos.y);
    }

    public Vector2 GetPerspectivePuckPosition(Transform puckTransform)
    {
        float mirror = IsMirrored ? -1f : 1f;
        return new Vector2(puckTransform.position.x * mirror, puckTransform.position.y);
    }
}