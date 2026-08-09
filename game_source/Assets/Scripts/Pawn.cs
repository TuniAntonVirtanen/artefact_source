using UnityEngine;
using System.Collections.Generic;

public class Pawn : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2000f;

    private Vector2 originalPosition;    
    private Rigidbody2D rb;
    private Vector2 desiredVelocity;

    private AbstractAgent owner;


    void Awake(){
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetOriginalPosition(Vector2 position){
        originalPosition = position;
    }

    // Return the pawn to its starting state.
    public void Reset(){
        transform.position = originalPosition;
        
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        desiredVelocity = Vector2.zero;        
    }

    public void SetColor(TeamSide teamSide){
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (teamSide == TeamSide.Left){
            spriteRenderer.color = Color.green;
        }
        else{
            spriteRenderer.color = Color.blue;
        }
    }

// ----- Movement
    public void SetMovement(Vector2 direction){
        direction = Vector2.ClampMagnitude(direction, 1f);
        desiredVelocity = direction * moveSpeed;
    }

    void FixedUpdate(){
        // Smoothly move toward the target velocity.
        rb.linearVelocity = Vector2.Lerp(
            rb.linearVelocity,
            desiredVelocity,
            0.25f
        );

    }

    public void SetOwner(AbstractAgent agent){
        owner = agent;
    }

    private void OnCollisionExit2D(Collision2D collision) 
    {
        if (owner == null)
            return;

        // Notify the owning agent after hitting the puck.
        if (collision.gameObject.CompareTag("Puck"))
        {
            Rigidbody2D puckRb = collision.gameObject.GetComponent<Rigidbody2D>();

            if (puckRb != null)
            {
                owner.OnPuckHit(puckRb.linearVelocity);
            }
        }
    }
}
