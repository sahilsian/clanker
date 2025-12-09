using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

// RESPONSIBILITY: Physics, Movement, Jumping, Wall Sliding, Stomping
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 15f;
    public float stompBounceForce = 8f; 

    [Header("Visuals")]
    public float doubleJumpSpinDuration = 0.5f;

    [Header("Jump Settings")]
    public float doubleJumpForceMultiplier = 0.5f;

    [Header("Wall Run Settings")]
    public float wallSlideSpeed = 2f;
    public float wallRunUpwardForce = 3f; // Upward momentum while wall running
    public Vector2 wallJumpForce = new Vector2(7f, 14f);
    public float wallRunDuration = 2f; // How long the player can wall run before needing to touch ground

    [Header("Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    
    public Transform wallCheck;
    public float wallCheckRadius = 0.3f;
    public LayerMask wallLayer;
    
    public LayerMask enemyLayer; 

    // Internal State
    private Rigidbody2D rb;
    private float horizontalMove = 0f;
    private bool isGrounded;
    private bool isFacingRight = true;
    private bool isTouchingWall;
    private bool isWallSliding;
    public bool IsStomping { get; private set; }
    
    [Header("Knockback")]
    public float knockbackForce = 8f;
    
    [Header("Knockback/Control")]
    public float knockbackLockDuration = 0.15f;
    private bool inputLocked = false;
    
    [Header("Hurt Settings")]
    public float hurtInputLockDuration = 0.5f;
    public int maxJumps = 2;
    private int jumpCount;
    private float wallRunTimeRemaining; // Timer for wall run duration
    private PlayerSkeletalAnimation playerAnim;
    
    // Property to allow other scripts (like PlayerDash) to take control
    public bool IsExternalMovementActive { get; set; } = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        playerAnim = GetComponentInChildren<PlayerSkeletalAnimation>();
        wallRunTimeRemaining = wallRunDuration; // Start with full wall run time
        Debug.Log($"[PlayerMovement] Wall Layer Mask: {wallLayer.value}");
    }

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);

        if (isGrounded)
        {
            wallRunTimeRemaining = wallRunDuration; // Reset wall run timer when grounded
        }

        HandleWallSliding();
        CheckForStomp();

        // 3. Movement Application
        // Allow external scripts (like Dash) to override movement
        if (!IsExternalMovementActive && !isWallSliding)
        {
            rb.linearVelocity = new Vector2(horizontalMove * moveSpeed, rb.linearVelocity.y);
        }

        FlipObject();
    }

    private void HandleWallSliding()
    {
        if (isTouchingWall) Debug.Log($"[WallRun] Touching: {isTouchingWall}, Grounded: {isGrounded}, Input: {horizontalMove}, TimeLeft: {wallRunTimeRemaining:F2}, Sliding: {isWallSliding}");

        // Can wall slide if: touching wall, airborne, moving toward wall, and have time remaining
        bool canWallSlide = isTouchingWall && !isGrounded && horizontalMove != 0 && wallRunTimeRemaining > 0;

        if (canWallSlide)
        {
            if (!isWallSliding)
            {
                Debug.Log("[WallRun] Started Wall Sliding");
                jumpCount = 0; // Reset jumps when grabbing wall
            }
            isWallSliding = true;
            
            // Consume wall run time
            wallRunTimeRemaining -= Time.fixedDeltaTime;
            
            // Prevent any downward movement during wall run - only maintain or go up
            float targetYVelocity = Mathf.Max(rb.linearVelocity.y, 0f) + wallRunUpwardForce * Time.fixedDeltaTime;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, targetYVelocity);
        }
        else
        {
            if (isWallSliding) Debug.Log("[WallRun] Stopped Wall Sliding");
            isWallSliding = false;
        }
    }

    private void CheckForStomp()
    {
        if (rb.linearVelocity.y < -0.1f)
        {
            Collider2D enemyStomped = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, enemyLayer);
            if (enemyStomped != null)
            {
                BossCar boss = enemyStomped.GetComponent<BossCar>();
                if (boss != null)
                {
                    Debug.Log("Player Stomped the Boss!");
                    if (!IsStomping) StartCoroutine(StompWindow());
                    boss.TakeDamage("Stomp");
                    Bounce();
                    return; 
                }

                EnemyBase enemy = enemyStomped.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    // Check if this enemy allows stomping
                    if (!enemy.canBeStomped) return;

                    Debug.Log("Player Stomped an Enemy!");
                    if (!IsStomping) StartCoroutine(StompWindow());
                    enemy.ApplyDamage(5, "Stomp"); 
                    Bounce();
                }
            }
        }
    }

    private IEnumerator StompWindow(float duration = 0.2f)
    {
        IsStomping = true;
        yield return new WaitForSeconds(duration);
        IsStomping = false;
    }

    private IEnumerator DoDoubleJumpSpin()
    {
        float elapsed = 0f;
        // Front flip direction: CW (-360) if facing right, CCW (360) if facing left
        float rotationAmount = isFacingRight ? -360f : 360f;
        
        // Use a separate tracker for rotation to avoid Euler angle wrapping issues
        float currentRotation = 0f;
        float previousRotation = 0f;

        while (elapsed < doubleJumpSpinDuration)
        {
            elapsed += Time.deltaTime;
            float percent = Mathf.Clamp01(elapsed / doubleJumpSpinDuration);
            
            // Linear rotation for now
            currentRotation = Mathf.Lerp(0f, rotationAmount, percent);
            float delta = currentRotation - previousRotation;
            
            transform.Rotate(0, 0, delta);
            previousRotation = currentRotation;
            
            yield return null;
        }

        // Ensure we end up exactly back at 0 rotation
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    private void Bounce()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, stompBounceForce);
        jumpCount = 0; // Reset to 0 so we can do a full set of jumps (Jump + Double Jump)
    }

    // --- INPUT SYSTEM MESSAGES ---

    public void OnMove(InputValue value)
    {
        if (inputLocked) return;
        Vector2 input = value.Get<Vector2>();
        horizontalMove = input.x;
    }

    public void OnJump(InputValue value)
    {
        if (inputLocked || !value.isPressed) return;

        // Wall jump removed - use normal jump logic instead
        // Normal & Double Jump
        if (isGrounded || jumpCount < maxJumps)
        {
            // If grounded, force reset (safety)
            if (isGrounded) jumpCount = 0;

            jumpCount++;

            if (jumpCount == 1)
            {
                // First Jump
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
            else
            {
                // Double Jump (or triple, etc.)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * doubleJumpForceMultiplier);
                if (playerAnim != null) playerAnim.TriggerJump();
                StartCoroutine(DoDoubleJumpSpin());
            }
        }
    }

    // --- UPDATED: Flip Logic ---
    private void FlipObject()
    {
        if (isWallSliding) return; 

        // If moving LEFT and currently facing RIGHT
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

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        
        // This flips the ENTIRE Player object (Sprite, Hitboxes, WallCheck, etc.)
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    /// <summary>
    /// Apply a knockback to the player away from an attacker.
    /// </summary>
    /// <param name="direction">Normalized direction away from attacker (playerPos - attackerPos)</param>
    /// <param name="force">Scalar force to apply</param>
    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (rb == null) return;

        // Ensure horizontal component is significant and add a small upward lift
        float horizontal = direction.x * force;
        float vertical = Mathf.Max(force * 0.45f, 0.5f);

        // Cancel existing velocity and apply immediate knockback
        // stop player movement input so FixedUpdate won't overwrite knockback
        horizontalMove = 0f;
        rb.linearVelocity = new Vector2(horizontal, vertical);

        // If wall sliding, cancel it so player isn't stuck to walls after knockback
        isWallSliding = false;
        // Temporarily lock input to prevent immediate counter-movement
        Debug.Log($"[PlayerMovement] ApplyKnockback: dir={direction} force={force} -> locking input for {knockbackLockDuration}s");
        StartCoroutine(TemporaryInputLock(knockbackLockDuration));
    }

    private IEnumerator TemporaryInputLock(float duration)
    {
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
        // Start the same temporary lock coroutine
        // ensure horizontal input is cleared immediately
        horizontalMove = 0f;
        Debug.Log($"[PlayerMovement] LockInput called for {duration} seconds");
        StartCoroutine(TemporaryInputLock(duration));
    }

    // Public helper to reset double jump (used by GrapplingHook)
    public void ResetDoubleJump()
    {
        jumpCount = 0;
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
        }
    }
}