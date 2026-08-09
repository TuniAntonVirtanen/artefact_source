using UnityEngine;

public class Puck : MonoBehaviour
{
    [SerializeField] private float hitForce = 100;
    [SerializeField] private float maxSpeed = 70f;
    [SerializeField] private float wallBounceDamping = 0.98f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        ClampSpeed();
    }

    // Prevent the puck from exceeding the maximum speed.
    private void ClampSpeed()
    {
        float speed = rb.linearVelocity.magnitude;

        if (speed > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ContactPoint2D contact = collision.contacts[0];
        Vector2 normal = contact.normal;

        // Handle wall bounces manually for consistent reflections.
        if (collision.gameObject.CompareTag("Wall"))
        {
            Vector2 v = rb.linearVelocity;

            // Only reflect if moving into the wall
            if (Vector2.Dot(v, normal) < 0f)
            {
                v = Vector2.Reflect(v, normal) * wallBounceDamping;

                // small separation to prevent re-collision jitter
                v += normal * 0.02f;

                rb.linearVelocity = v;
            }

            return;
        }

        // Apply an impulse based on the paddle's movement.
        if (collision.gameObject.CompareTag("Pawn"))
        {
            Rigidbody2D paddleRb = collision.rigidbody;
            if (paddleRb == null) return;

            Vector2 paddleVel = paddleRb.linearVelocity;
            Vector2 puckVel = rb.linearVelocity;

            // Relative velocity (key for billiard-style response)
            Vector2 relativeVelocity = puckVel - paddleVel;

            float impactSpeed = Vector2.Dot(relativeVelocity, normal);

            // Ignore if already separating
            if (impactSpeed > 0f) return;

            // Tunable impulse strength
            float impulse = -(1f + hitForce) * impactSpeed;

            rb.linearVelocity += normal * impulse;
        }
    }
}