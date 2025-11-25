using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

<<<<<<< HEAD
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerControl : MonoBehaviour
=======
// RESPONSIBILITY: Physics, Movement, Jumping, Wall Sliding, Stomping
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
>>>>>>> fighter/main
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 15f;
    public float stompBounceForce = 8f; 

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

<<<<<<< HEAD
    [Header("Graphics")]
    public Transform spriteTransform;
=======
    // REMOVED: public Transform spriteTransform; -> We now flip the whole object!
>>>>>>> fighter/main

    // Internal State
    private Rigidbody2D rb;
    private float horizontalMove = 0f;
    private bool isGrounded;
    private bool isFacingRight = true;
<<<<<<< HEAD
=======
    private bool isTouchingWall;
    private bool isWallSliding;
    public bool IsStomping { get; private set; }
    [Header("Knockback")]
    public float knockbackForce = 8f;
    [Header("Knockback/Control")]
    public float knockbackLockDuration = 0.15f;
    private bool inputLocked = false;
    [Header("Hurt Settings")]
    [Tooltip("How long player input is locked when the player is hurt (in seconds)")]
    public float hurtInputLockDuration = 0.25f;
>>>>>>> fighter/main

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void FixedUpdate()
    {
<<<<<<< HEAD
=======
        // 1. Checks
>>>>>>> fighter/main
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);

        // 2. Logic
        HandleWallSliding();
        CheckForStomp();

        // 3. Movement Application
        if (!isWallSliding)
        {
            rb.linearVelocity = new Vector2(horizontalMove * moveSpeed, rb.linearVelocity.y);
        }

        FlipObject();
    }

<<<<<<< HEAD
    // ---- Player Input ----

    public void OnMove(InputValue value)
    {
        if (GameManager.Instance != null && GameManager.Instance.isDialogueActive)
            return;

=======
    private void HandleWallSliding()
    {
        if (isTouchingWall && !isGrounded && horizontalMove != 0)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue));
        }
        else
        {
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
                    Debug.Log("Player Stomped an Enemy!");
                    if (!IsStomping) StartCoroutine(StompWindow());
                    enemy.TakeDamage(5, "Stomp"); 
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

    private void Bounce()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, stompBounceForce);
    }

    // --- INPUT SYSTEM MESSAGES ---

    public void OnMove(InputValue value)
    {
        if (inputLocked) return;
>>>>>>> fighter/main
        Vector2 moveInput = value.Get<Vector2>();
        horizontalMove = moveInput.x;
    }

    public void OnJump(InputValue value)
    {
<<<<<<< HEAD
        if (GameManager.Instance != null && GameManager.Instance.isDialogueActive)
            return;

        if (value.isPressed && isGrounded)
=======
        if (value.isPressed)
>>>>>>> fighter/main
        {
            if (inputLocked) return;
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
            else if (isWallSliding)
            {
                isWallSliding = false;
                float jumpDirection = isFacingRight ? -1 : 1; 
                rb.linearVelocity = new Vector2(jumpDirection * wallJumpForce.x, wallJumpForce.y);
            }
        }
    }

<<<<<<< HEAD
    public void OnKick(InputValue value)
    {
        if (GameManager.Instance != null && GameManager.Instance.isDialogueActive)
            return;

        if (value.isPressed)
        {
            Debug.Log("PERFORM KICK!");
        }
=======
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
>>>>>>> fighter/main
    }

    /// <summary>
    /// Apply a knockback to the player away from an attacker.
    /// </summary>
    /// <param name="direction">Normalized direction away from attacker (playerPos - attackerPos)</param>
    /// <param name="force">Scalar force to apply</param>
    public void ApplyKnockback(Vector2 direction, float force)
    {
<<<<<<< HEAD
        if (GameManager.Instance != null && GameManager.Instance.isDialogueActive)
            return;

        if (value.isPressed)
        {
            Debug.Log("PERFORM HEAVY UPPERCUT!");
        }
=======
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
>>>>>>> fighter/main
    }

    private IEnumerator TemporaryInputLock(float duration)
    {
<<<<<<< HEAD
        if (GameManager.Instance != null && GameManager.Instance.isDialogueActive)
            return;

        if (value.isPressed)
        {
            Debug.Log("PERFORM DODGE-ROLL!");
        }
    }

    private void FlipSprite()
    {
        if (horizontalMove < 0 && isFacingRight)
        {
            isFacingRight = false;
            spriteTransform.localScale = new Vector3(
                -Mathf.Abs(spriteTransform.localScale.x),
                spriteTransform.localScale.y,
                spriteTransform.localScale.z
            );
        }
        else if (horizontalMove > 0 && !isFacingRight)
        {
            isFacingRight = true;
            spriteTransform.localScale = new Vector3(
                Mathf.Abs(spriteTransform.localScale.x),
                spriteTransform.localScale.y,
                spriteTransform.localScale.z
            );
        }
    }

=======
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

>>>>>>> fighter/main
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
