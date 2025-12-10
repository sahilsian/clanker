using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

// Handles player movement, jumping, combat, and wall sliding.
// Requires PlayerInput component set to "Send Messages".
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 15f;
    public float stompBounceForce = 8f; // How high the player bounces after stomping

    [Header("Wall Run Settings")]
    public float wallSlideSpeed = 2f; 
    public Vector2 wallJumpForce = new Vector2(7f, 14f);

    [Header("Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    
    public Transform wallCheck;
    public float wallCheckRadius = 0.3f;
    public LayerMask wallLayer;
    
    public LayerMask enemyLayer; 

    [Header("Graphics")]
    public Transform spriteTransform;

    [Header("Combat")]
    public Transform kickHitbox; // Empty object at player's foot
    public float kickRadius = 0.5f;
    public LayerMask enemyLayer; // Set this to the "Enemy" layer

    // --- NEW: Wall Run ---
    [Header("Wall Run")]
    public Transform wallCheck; // Assign the empty object on the player's side
    public LayerMask wallLayer;   // Set this to the "Wall" layer
    public float wallCheckRadius = 0.3f;
    public float wallSlideSpeed = 2f; // How fast we slide down
    public Vector2 wallJumpForce = new Vector2(7f, 14f); // x = away, y = up

    // Internal State
    private Rigidbody2D rb;
    private float horizontalMove = 0f;
    private bool isGrounded;
    private bool isFacingRight = true;

    // --- NEW: Wall State ---
    private bool isTouchingWall;
    private bool isWallSliding;

    private void Start()
    {
        // Cache rigidbody and lock rotation so physics doesn't tip the player over
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void FixedUpdate()
    {
        // Physics checks
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);

        HandleWallSliding();

        // Apply horizontal movement *only* if not wall sliding
        if (!isWallSliding)
        {
            rb.linearVelocity = new Vector2(horizontalMove * moveSpeed, rb.linearVelocity.y);
        }

        FlipSprite();
        CheckForStomp();
    }

    // --- NEW: Wall Slide Logic ---
    private void HandleWallSliding()
    {
        // Wall slide if: touching wall, not grounded, and moving into the wall
        if (isTouchingWall && !isGrounded && horizontalMove != 0)
        {
            isWallSliding = true;
            // Slide down at a controlled speed
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void CheckForStomp()
    {
        // Stomp if: falling, and our feet hit an enemy
        if (rb.linearVelocity.y < -0.1f)
        {
            Collider2D enemyStomped = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, enemyLayer);
            if (enemyStomped != null)
            {
                // Check for RoombaAI script and defeat it
                RoombaAI enemy = enemyStomped.GetComponent<RoombaAI>();
                if (enemy != null)
                {
                    enemy.Defeat();
                    // Bounce off
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, stompBounceForce);
                }
            }
        }
    }

    // --- Input System Callbacks ---

    public void OnMove(InputValue value)
    {
        // Capture horizontal input unless locked by knockback/hurt
        if (inputLocked) return;
        Vector2 moveInput = value.Get<Vector2>();
        horizontalMove = moveInput.x;
    }

    // --- MODIFIED: OnJump ---
    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            if (isGrounded)
            {
                // Normal ground jump
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
            else if (isWallSliding) // --- NEW: Wall Jump ---
            {
                isWallSliding = false;
                // Jump away from the wall
                float jumpDirection = isFacingRight ? -1 : 1; 
                rb.linearVelocity = new Vector2(jumpDirection * wallJumpForce.x, wallJumpForce.y);
            }
        }
    }

    // --- MODIFIED: FlipSprite ---
    private void FlipSprite()
    {
        // Don't flip the sprite while sliding
        if (isWallSliding) return; 

        if (horizontalMove < 0 && isFacingRight)
        {
            Flip();
        }
        // If moving RIGHT and currently facing LEFT
        else if (horizontalMove > 0 && !isFacingRight)
        {
            Flip();
        }
    }

    public void OnKick(InputValue value)
    {
        if (value.isPressed)
        {
            Debug.Log("PERFORM KICK!");
            
            Collider2D[] hits = Physics2D.OverlapCircleAll(kickHitbox.position, kickRadius, enemyLayer);
            foreach (Collider2D hit in hits)
            {
                RoombaAI enemy = hit.GetComponent<RoombaAI>();
                if (enemy != null)
                {
                    enemy.Defeat();
                }
            }
        }
    }

    private IEnumerator TemporaryInputLock(float duration)
    {
        // Temporarily block movement input (used during knockback or hurt)
        // prevent further input and clear any residual horizontal input
        inputLocked = true;
        horizontalMove = 0f;
        Debug.Log($"[PlayerMovement] Input locked for {duration} seconds");
        yield return new WaitForSeconds(duration);
        inputLocked = false;
        Debug.Log("[PlayerMovement] Input unlocked");
    }

    // Public helper to lock input for a given duration (used by other scripts like PlayerCombat)
    public void LockInput(float duration)
    {
        // Start the lock coroutine and immediately clear stored horizontal input
        // Start the same temporary lock coroutine
        // ensure horizontal input is cleared immediately
        horizontalMove = 0f;
        Debug.Log($"[PlayerMovement] LockInput called for {duration} seconds");
        StartCoroutine(TemporaryInputLock(duration));
    }

    // --- MODIFIED: OnDrawGizmos ---
    private void OnDrawGizmos()
    {
        // Draw the ground check
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // Draw the kick hitbox
        if (kickHitbox != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(kickHitbox.position, kickRadius);
        }

        // --- NEW: Draw Wall Check Gizmo ---
        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
        }
    }
}
