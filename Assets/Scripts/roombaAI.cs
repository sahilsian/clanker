using UnityEngine;

// Handles the behavior for the "Rogue Roomba Bot" enemy
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class RoombaAI : MonoBehaviour
{
    [Header("Patrol")]
    public Transform pointA; // The first patrol point
    public Transform pointB; // The second patrol point
    public float patrolSpeed = 3f;

    [Header("Graphics")]
    public Transform spriteTransform; // The bot's sprite (if separate)

    private Rigidbody2D rb;
    private Transform currentTarget;
    private bool isFacingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        // Start by moving towards point B
        currentTarget = pointB;
        if (spriteTransform == null)
        {
            spriteTransform = transform; // Default to this object
        }
    }

    void Update()
    {
        if (currentTarget == null) return; // Don't do anything if no points are set

        // Move towards the current target
        float direction = (currentTarget.position.x > transform.position.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(patrolSpeed * direction, rb.linearVelocity.y);

        // Flip the sprite to face the direction of movement
        if (direction > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (direction < 0 && isFacingRight)
        {
            Flip();
        }

        // Check if we've reached the target
        if (Mathf.Abs(transform.position.x - currentTarget.position.x) < 0.5f)
        {
            // Switch targets
            currentTarget = (currentTarget == pointB) ? pointA : pointB;
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        spriteTransform.localScale = new Vector3(-spriteTransform.localScale.x,
                                                 spriteTransform.localScale.y,
                                                 spriteTransform.localScale.z);
    }

    // This function is called by the PlayerController when the enemy is hit
    public void Defeat()
    {
        Debug.Log("Roomba defeated!");
        // TODO: Add explosion/death animation
        Destroy(gameObject);
    }

    // This handles damaging the player
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Roomba hit the player!");
            // TODO: Add logic to damage the player
        }
    }
}