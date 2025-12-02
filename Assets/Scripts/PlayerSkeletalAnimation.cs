using UnityEngine;
using System.Collections;

// This script replaces PlayerAnimation.cs for the skeletal character.
// It uses Animator.Play() to directly control animations, bypassing the
// complex transition graph while keeping the exact same "game feel" logic.
public class PlayerSkeletalAnimation : MonoBehaviour
{
    [Header("Animation Clip Names")]
    // These must match the exact names in your Animation window/Animator Controller
    public string idleAnim = "idle";
    public string runAnim = "Walk";
    public string jumpAnim = "Jump";
    public string wallRunAnim = "Wall Run";
    public string punchAnim = "Punch";
    public string kickAnim = "Kick";
    public string hurtAnim = "Hurt";
    public string swingAnim = "Swing";
    
    [Header("Animation Timing")]
    public float punchDuration = 0.25f;
    public float kickDuration = 0.5f;
    public float hurtDuration = 0.5f;

    [Header("References")]
    public Animator animator;
    public Rigidbody2D rb;

    // These variables MUST MATCH the ones on your PlayerController
    [Header("Duplicate State Checks")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;
    public Transform wallCheck;
    public LayerMask wallLayer;
    public float wallCheckRadius = 0.2f;

    // Private state variables
    private bool isGrounded;
    private bool isWallSliding;
    private bool isAttacking = false;
    private bool isHurt = false;
    private bool isSwinging = false;
    
    // Track current animation to avoid spamming Play()
    private string currentAnimState = "";

    void Start()
    {
        if (rb == null) rb = GetComponentInParent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("PlayerSkeletalAnimation: No Animator found! Please assign it in the inspector.");
        }
    }

    void FixedUpdate()
    {
        // --- Duplicate Physics Logic (from PlayerAnimation.cs) ---
        // We do this to keep the animation logic self-contained and responsive
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        bool isWallDetected = false;
        if (wallCheck != null)
        {
            isWallDetected = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);
        }

        // Check horizontal movement
        float velocityX = 0f;
        if (rb != null)
        {
            // Unity 6 uses linearVelocity, older versions use velocity.
            // Using linearVelocity based on your previous file context.
            velocityX = rb.linearVelocity.x; 
        }

        bool isMovingHorizontally = Mathf.Abs(velocityX) > 0.1f;

        if (isWallDetected && !isGrounded && isMovingHorizontally)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
    }

    void Update()
    {
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        string newAnimState = idleAnim;

        // Priority 1: Hurt
        if (isHurt)
        {
            newAnimState = hurtAnim;
        }
        // Priority 2: Attacking
        else if (isAttacking)
        {
            // We don't change the state here because PlayAttack() sets the specific attack state (Punch/Kick)
            // and we want to hold that state until isAttacking becomes false.
            return; 
        }
        // Priority 3: Swinging
        else if (isSwinging)
        {
            newAnimState = swingAnim;
        }
        // Priority 4: Wall Sliding
        else if (isWallSliding)
        {
            newAnimState = wallRunAnim;
        }
        // Priority 4: Jumping / Falling
        else if (!isGrounded)
        {
            newAnimState = jumpAnim;
        }
        // Priority 5: Running
        else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            newAnimState = runAnim;
        }
        // Priority 6: Idle (Default)
        else
        {
            newAnimState = idleAnim;
        }

        // Only call Play if the state has changed
        ChangeAnimationState(newAnimState);
    }

    private void ChangeAnimationState(string newState)
    {
        if (currentAnimState == newState) return;

        if (animator != null)
        {
            animator.Play(newState);
        }
        currentAnimState = newState;
    }

    public void TriggerJump()
    {
        if (animator != null)
        {
            animator.Play(jumpAnim, -1, 0f);
            currentAnimState = jumpAnim;
        }
    }

    // Public API: Matches PlayerAnimation.cs
    public void PlayAttack(string attackType)
    {
        string animToPlay = "";
        float duration = 0f;

        if (attackType == "Punch")
        {
            animToPlay = punchAnim;
            duration = punchDuration;
        }
        else if (attackType == "Kick")
        {
            animToPlay = kickAnim;
            duration = kickDuration;
        }
        else
        {
            return;
        }

        // Interrupt existing attack if any
        StopCoroutine("AttackCoroutine");
        StartCoroutine(AttackCoroutine(duration, animToPlay));
    }

    private IEnumerator AttackCoroutine(float duration, string animName)
    {
        isAttacking = true;
        
        // Force play the attack animation immediately
        ChangeAnimationState(animName);

        yield return new WaitForSeconds(duration);

        isAttacking = false;
        // The Update() loop will take over and transition back to Idle/Run/etc.
        currentAnimState = ""; // Force update in next frame
    }

    public void PlayHurt()
    {
        StopCoroutine("HurtCoroutine");
        StartCoroutine(HurtCoroutine());
    }

    private IEnumerator HurtCoroutine()
    {
        isHurt = true;
        yield return new WaitForSeconds(hurtDuration);
        isHurt = false;
        currentAnimState = ""; // Force update
    }

    public void SetSwinging(bool swinging)
    {
        isSwinging = swinging;
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
