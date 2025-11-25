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
    // legacy generic attack slot removed; use Punch/Kick slots instead
    public Sprite punchSprite;
    public float punchAnimationTime = 0.25f;
    public Sprite kickSprite;
    public float kickAnimationTime = 0.5f;
    public Sprite hurtSprite;
    public float hurtAnimationTime = 0.5f;

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
    private bool isHurt = false;
    private Sprite currentAttackSprite = null;

    // --- NEW: Animation state variables ---
    private int currentRunFrameIndex = 0;
    private float runFrameTimer = 0f;

    void Start()
    {
        // Cache components and set the initial idle sprite
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
        // Mirror the physics checks from PlayerMovement for animation decisions
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
        // Drive sprite changes every frame
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        // Choose the correct sprite based on movement, attack, and hurt states
        if (spriteRenderer == null) return;

        if (isHurt)
        {
            if (hurtSprite != null) spriteRenderer.sprite = hurtSprite;
            return;
        }

        if (isAttacking)
        {
            if (currentAttackSprite != null) spriteRenderer.sprite = currentAttackSprite;
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
        else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f && runFrames != null && runFrames.Length > 0) // Player is running
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
            // Fall back to the first run frame if idle sprite is missing so the player stays visible
            if (idleSprite != null) spriteRenderer.sprite = idleSprite;
            else if (runFrames != null && runFrames.Length > 0) spriteRenderer.sprite = runFrames[0];
            // Reset run animation when idle
            currentRunFrameIndex = 0;
            runFrameTimer = 0f;
        }
    }

    // (OnAttack removed — attacks use Punch/Kick inputs now)

    // Public API: play an attack animation by type: "Punch", "Kick", or "Default"
    public void PlayAttack(string attackType)
    {
        // Select attack sprite and duration, then trigger the coroutine
        Sprite spriteToUse = null;
        float duration = 0f;

        if (attackType == "Punch")
        {
            spriteToUse = punchSprite;
            duration = punchAnimationTime;
        }
        else if (attackType == "Kick")
        {
            spriteToUse = kickSprite;
            duration = kickAnimationTime;
        }
        else
        {
            // Unknown attack type — do nothing
            return;
        }

        // restart attack if already attacking
        try { StopCoroutine("AttackCoroutine"); } catch { }
        StartCoroutine(AttackCoroutine(duration, spriteToUse));
    }

    private IEnumerator AttackCoroutine(float duration, Sprite spriteToUse)
    {
        // Display the attack sprite for a set duration
        isAttacking = true;
        currentAttackSprite = spriteToUse;
        if (spriteRenderer != null && currentAttackSprite != null)
            spriteRenderer.sprite = currentAttackSprite;

        yield return new WaitForSeconds(duration);

        isAttacking = false;
        currentAttackSprite = null;
    }

    // Public API: play the hurt animation for a short duration
    public void PlayHurt()
    {
        // Restart hurt coroutine to ensure the effect persists for full duration
        // If already hurting, restart the coroutine to extend the effect
        try { StopCoroutine("HurtCoroutine"); } catch { }
        StartCoroutine(HurtCoroutine());
    }

    private IEnumerator HurtCoroutine()
    {
        // Temporarily mark the player as hurt for animation purposes
        isHurt = true;
        yield return new WaitForSeconds(hurtAnimationTime);
        isHurt = false;
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
