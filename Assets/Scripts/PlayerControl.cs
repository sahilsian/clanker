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

    // REMOVED: public Transform spriteTransform; -> We now flip the whole object!

    // Internal State
    private Rigidbody2D rb;
    private float horizontalMove = 0f;
    private bool isGrounded;
    private bool isFacingRight = true;
    private bool isTouchingWall;
    private bool isWallSliding;
    public bool IsStomping { get; private set; }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void FixedUpdate()
    {
        // 1. Checks
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
        Vector2 moveInput = value.Get<Vector2>();
        horizontalMove = moveInput.x;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
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