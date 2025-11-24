using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerControl : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 15f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;

    [Header("Graphics")]
    public Transform spriteTransform;

    private Rigidbody2D rb;
    private float horizontalMove = 0f;
    private bool isGrounded;
    private bool isFacingRight = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        rb.linearVelocity = new Vector2(horizontalMove * moveSpeed, rb.linearVelocity.y);

        FlipSprite();
    }

    // ---- Player Input ----

    public void OnMove(InputValue value)
    {
        if (GameManager.Instance != null && GameManager.Instance.isDialogueActive)
            return;

        Vector2 moveInput = value.Get<Vector2>();
        horizontalMove = moveInput.x;
    }

    public void OnJump(InputValue value)
    {
        if (GameManager.Instance != null && GameManager.Instance.isDialogueActive)
            return;

        if (value.isPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    public void OnKick(InputValue value)
    {
        if (GameManager.Instance != null && GameManager.Instance.isDialogueActive)
            return;

        if (value.isPressed)
        {
            Debug.Log("PERFORM KICK!");
        }
    }

    public void OnUppercut(InputValue value)
    {
        if (GameManager.Instance != null && GameManager.Instance.isDialogueActive)
            return;

        if (value.isPressed)
        {
            Debug.Log("PERFORM HEAVY UPPERCUT!");
        }
    }

    public void OnDodgeRoll(InputValue value)
    {
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

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
