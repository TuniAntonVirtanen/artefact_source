using UnityEngine;

public class PawnTestDriver : MonoBehaviour
{
    [SerializeField] private Pawn pawn;

    private void Awake()
    {
        pawn = GetComponent<Pawn>();
    }

    private void Update()
    {
        // Get input from WASD / Arrow keys
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        // Combine into a movement vector
        Vector2 inputDirection = new Vector2(moveX, moveY);

        // Normalize to prevent faster diagonal movement
        if (inputDirection.magnitude > 1f)
        {
            inputDirection.Normalize();
        }

        // Pass the direction into your existing Pawn physics handler
        pawn.SetMovement(inputDirection);
    }
}