using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] private TeamSide teamSide;
    [SerializeField] private BoardManager boardManager;

    // Detect when the puck enters the goal.
    void OnCollisionEnter2D(Collision2D collision){
        if (collision.gameObject.name == "Puck") {
            boardManager.GoalScored(teamSide);
        }        
    }
}
