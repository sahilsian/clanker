using UnityEngine;

// RESPONSIBILITY: Movement and Dealing Damage to Player
[RequireComponent(typeof(Rigidbody2D))]
public class RoombaAI : MonoBehaviour
{
    [Header("Patrol")]
    public Transform pointA;
    public Transform pointB;
    public float patrolSpeed = 3f;
    public int contactDamage = 1;

    [Header("Graphics")]
    public Transform spriteTransform; 

    private Rigidbody2D rb;
    private Transform currentTarget;
    private bool isFacingRight = true;

    void Start()
    {
        // Cache components and pick an initial patrol target
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        // Default to current transform if not assigned
        if (spriteTransform == null) spriteTransform = transform;
        
        // DETACH WAYPOINTS so they don't move with the enemy
        if (pointA != null) pointA.parent = null;
        if (pointB != null) pointB.parent = null;

        // Start patrol
        currentTarget = pointB;
    }

    void FixedUpdate()
    {
        // Patrol between two points, flipping visuals as needed
        if (currentTarget == null) return; 

        // 1. Move
        float direction = (currentTarget.position.x > transform.position.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(patrolSpeed * direction, rb.linearVelocity.y);

        // 2. Face Direction
        if (direction > 0 && !isFacingRight) Flip();
        else if (direction < 0 && isFacingRight) Flip();

        // 3. Switch Target when close
        if (Mathf.Abs(transform.position.x - currentTarget.position.x) < 0.5f)
        {
            currentTarget = (currentTarget == pointB) ? pointA : pointB;
        }
    }

    private void Flip()
    {
        // Mirror the sprite horizontally when turning around
        isFacingRight = !isFacingRight;
        Vector3 scale = spriteTransform.localScale;
        scale.x *= -1;
        spriteTransform.localScale = scale;
    }

    // Handles damaging the player if they touch the Roomba
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Apply contact damage and knockback when colliding with the player
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Roomba hit the player! Ouch!");
            // Apply contact damage using the PlayerCombat API
            PlayerCombat pc = collision.gameObject.GetComponent<PlayerCombat>();
            if (pc != null)
            {
                bool damaged = pc.TakeDamage(contactDamage, "Roomba");

                // Apply knockback if player actually took damage
                if (damaged)
                {
                    PlayerMovement pm = collision.gameObject.GetComponent<PlayerMovement>();
                    if (pm != null)
                    {
                        Vector2 dir = (pm.transform.position - transform.position).normalized;
                        pm.ApplyKnockback(dir, pm.knockbackForce);
                    }
                }
            }
        }
    }
}
