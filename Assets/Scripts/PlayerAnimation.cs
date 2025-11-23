using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

// This script is self-contained and handles all sprite swapping.
// It duplicates state checks (ground, wall, etc.) to avoid
// modifying the PlayerController.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAnimation : MonoBehaviour
{
    [Header("Sprite Poses")]
    public Sprite idleSprite;
    public Sprite[] runFrames; // --- MODIFIED: Array for run animation frames ---
    public float runAnimationSpeed = 0.1f; // --- NEW: Frequency for switching run frames ---
    public Sprite jumpSprite;
    public Sprite wallRunSprite;
    public Sprite attackSprite;
    public float attackAnimationTime = 0.5f;

    [Header("References")]
    public Transform spriteTransform;

    // These variables MUST MATCH the ones on your PlayerController
    [Header("Duplicate State Checks")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;
    public Transform wallCheck;
    public LayerMask wallLayer;
    public float wallCheckRadius = 0.2f;

    // Component references
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    // Private state variables
    private bool isGrounded;
    private bool isWallSliding;
    private bool isAttacking = false;

    // --- NEW: Animation state variables ---
    private int currentRunFrameIndex = 0;
    private float runFrameTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (spriteTransform != null)
        {
            spriteRenderer = spriteTransform.GetComponent<SpriteRenderer>();
        }
        else
        {
            Debug.LogError("Sprite Transform is not assigned in PlayerAnimation!");
        }

        // Initialize with idle sprite
        spriteRenderer.sprite = idleSprite;
    }

    void FixedUpdate()
    {
        // --- Duplicate Physics Logic ---
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        bool isWallDetected = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);
        bool isMovingHorizontally = Mathf.Abs(rb.linearVelocity.x) > 0.1f; // Using rb.velocity.x here

        if (isWallDetected && !isGrounded && isMovingHorizontally)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
        // --- End Duplicate Logic ---
    }


    void Update()
    {
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        if (isAttacking)
        {
            spriteRenderer.sprite = attackSprite;
            return;
        }

        if (isWallSliding)
        {
            spriteRenderer.sprite = wallRunSprite;
        }
        else if (!isGrounded)
        {
            spriteRenderer.sprite = jumpSprite;
        }
        else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f) // Player is running
        {
            // --- NEW: Run animation logic ---
            runFrameTimer += Time.deltaTime;
            if (runFrameTimer >= runAnimationSpeed)
            {
                currentRunFrameIndex = (currentRunFrameIndex + 1) % runFrames.Length;
                spriteRenderer.sprite = runFrames[currentRunFrameIndex];
                runFrameTimer = 0f;
            }
        }
        else // Idle state
        {
            spriteRenderer.sprite = idleSprite;
            // Reset run animation when idle
            currentRunFrameIndex = 0;
            runFrameTimer = 0f;
        }
    }

    // --- Input System Callback ---
    public void OnAttack(InputValue value)
    {
        if (value.isPressed && !isAttacking)
        {
            StartCoroutine(AttackCoroutine());
        }
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        yield return new WaitForSeconds(attackAnimationTime);
        isAttacking = false;
    }

    // Draws gizmos for ground and wall checks
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