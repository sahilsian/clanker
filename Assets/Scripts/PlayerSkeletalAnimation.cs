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

    [Header("Effect Settings")]
    [Header("Punch Effect")]
    public SpriteRenderer punchEffectRenderer;
    public Sprite[] punchEffectFrames;
    public float punchEffectSpeed = 0.1f;
    public Vector2 punchMoveOffset = new Vector2(1f, 0f);
    public float punchMoveSpeed = 5f;

    [Header("Kick Effect")]
    public SpriteRenderer kickEffectRenderer;
    public Sprite[] kickEffectFrames;
    public float kickEffectSpeed = 0.1f;
    public Vector2 kickMoveOffset = new Vector2(0.5f, 0.5f);
    public float kickMoveSpeed = 5f;

    // Internal state for effects
    private Vector3 punchInitialPos;
    private Vector3 kickInitialPos;
    
    // Frenzy Reference
    private PlayerFrenzy playerFrenzy;

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
    public bool IsAttacking { get; private set; } = false;
    private bool isHurt = false;
    private bool isSwinging = false;
    
    // Track current animation to avoid spamming Play()
    private string currentAnimState = "";

    // Coroutine tracking
    private Coroutine currentAttackCoroutine;
    private Coroutine currentEffectCoroutine;

    void Start()
    {
        if (rb == null) rb = GetComponentInParent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("PlayerSkeletalAnimation: No Animator found! Please assign it in the inspector.");
        }

        // Store initial positions of effect renderers
        if (punchEffectRenderer != null) punchInitialPos = punchEffectRenderer.transform.localPosition;
        if (kickEffectRenderer != null) kickInitialPos = kickEffectRenderer.transform.localPosition;

        playerFrenzy = GetComponent<PlayerFrenzy>();
        if (playerFrenzy == null) playerFrenzy = GetComponentInParent<PlayerFrenzy>();
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
        else if (IsAttacking)
        {
            // We don't change the state here because PlayAttack() sets the specific attack state (Punch/Kick)
            // and we want to hold that state until IsAttacking becomes false.
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
        if (currentAttackCoroutine != null) StopCoroutine(currentAttackCoroutine);
        if (currentEffectCoroutine != null) StopCoroutine(currentEffectCoroutine);

        // Reset effects immediately
        if (punchEffectRenderer != null) { punchEffectRenderer.enabled = false; punchEffectRenderer.sprite = null; }
        if (kickEffectRenderer != null) { kickEffectRenderer.enabled = false; kickEffectRenderer.sprite = null; }

        currentAttackCoroutine = StartCoroutine(AttackCoroutine(duration, animToPlay, attackType));
    }

    private IEnumerator AttackCoroutine(float duration, string animName, string attackType)
    {
        IsAttacking = true;
        
        // Force play the attack animation immediately
        ChangeAnimationState(animName);

        // --- Play Effect Animation ---
        // Only play normal effects if Frenzy is NOT active
        if (playerFrenzy == null || !playerFrenzy.IsFrenzyActive)
        {
            SpriteRenderer targetRenderer = null;
            Sprite[] effects = null;
            float speed = 0.1f;
            Vector2 moveOffset = Vector2.zero;
            float moveSpeed = 0f;
            Vector3 initialPos = Vector3.zero;

            if (attackType == "Punch") 
            {
                targetRenderer = punchEffectRenderer;
                effects = punchEffectFrames;
                speed = punchEffectSpeed;
                moveOffset = punchMoveOffset;
                moveSpeed = punchMoveSpeed;
                initialPos = punchInitialPos;
            }
            else if (attackType == "Kick") 
            {
                targetRenderer = kickEffectRenderer;
                effects = kickEffectFrames;
                speed = kickEffectSpeed;
                moveOffset = kickMoveOffset;
                moveSpeed = kickMoveSpeed;
                initialPos = kickInitialPos;
            }

            if (targetRenderer != null && effects != null && effects.Length > 0)
            {
                currentEffectCoroutine = StartCoroutine(PlayEffectAnimation(targetRenderer, effects, speed, initialPos, moveOffset, moveSpeed));
            }
        }

        yield return new WaitForSeconds(duration);

        IsAttacking = false;
        // The Update() loop will take over and transition back to Idle/Run/etc.
        currentAnimState = ""; // Force update in next frame
        currentAttackCoroutine = null;
    }

    private IEnumerator PlayEffectAnimation(SpriteRenderer renderer, Sprite[] frames, float animSpeed, Vector3 startPos, Vector2 offset, float moveSpeed)
    {
        renderer.enabled = true;
        renderer.transform.localPosition = startPos; // Reset to start

        // Calculate target position based on facing direction
        // Assuming the parent flips scale.x, localPosition logic might need adjustment if it doesn't.
        // If the parent flips, local offset X is automatically flipped relative to world, which is good.
        Vector3 targetPos = startPos + (Vector3)offset;

        // We run the animation and movement in parallel within this coroutine
        int frameCount = frames.Length;
        float totalDuration = frameCount * animSpeed;
        float elapsed = 0f;
        int currentFrame = 0;

        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;

            // 1. Update Sprite
            // Calculate frame based on time to be precise
            currentFrame = Mathf.Clamp(Mathf.FloorToInt(elapsed / animSpeed), 0, frameCount - 1);
            renderer.sprite = frames[currentFrame];

            // 2. Update Position (Move towards target)
            // We move continuously towards the target
            renderer.transform.localPosition = Vector3.MoveTowards(renderer.transform.localPosition, targetPos, moveSpeed * Time.deltaTime);

            yield return null;
        }

        renderer.sprite = null;
        renderer.enabled = false;
        renderer.transform.localPosition = startPos; // Reset for next time
        currentEffectCoroutine = null;
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
