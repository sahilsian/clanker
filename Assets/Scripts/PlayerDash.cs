using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerDash : MonoBehaviour
{
    [Header("Settings")]
    public bool isEnabled = true; // For Level 3
    [Tooltip("Distance to dash")]
    public float dashDistance = 5f;
    [Tooltip("Time it takes to complete dashed")]
    public float dashDuration = 0.2f;
    public float dashCooldown = 1.0f;

    [Header("Input")]
    // public KeyCode dashKey = KeyCode.LeftShift; // Legacy Unity Input

    private PlayerMovement playerMovement; // Note: Script file is PlayerControl.cs but class is PlayerMovement
    private PlayerCombat playerCombat;
    private PlayerSkeletalAnimation playerAnim;
    private Rigidbody2D rb;
    private float dashTimer;
    private bool isDashing;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerCombat = GetComponent<PlayerCombat>();
        playerAnim = GetComponent<PlayerSkeletalAnimation>();
        if (playerAnim == null) playerAnim = GetComponentInChildren<PlayerSkeletalAnimation>();
        rb = GetComponent<Rigidbody2D>();

        if (playerMovement == null) Debug.LogError("PlayerDash: missing PlayerMovement!");
        if (playerCombat == null) Debug.LogError("PlayerDash: missing PlayerCombat!");
    }

    private void Update()
    {
        // Only run if enabled (Level 3 toggle)
        if (!isEnabled) return;
        
        if (dashTimer > 0) dashTimer -= Time.deltaTime;

        // Use New Input System directly for Hardcoded Key
        bool dashPressed = false;
        if (Keyboard.current != null)
        {
            dashPressed = Keyboard.current.leftShiftKey.wasPressedThisFrame;
        }

        if (dashPressed && dashTimer <= 0 && !isDashing)
        {
            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;
        dashTimer = dashCooldown;

        // Determine Direction based on X scale (since logic flips scale for facing)
        float direction = transform.localScale.x > 0 ? 1f : -1f;

        // 1. Take Control of Movement
        if (playerMovement != null) playerMovement.IsExternalMovementActive = true;
        
        // 2. Grant Invulnerability
        if (playerCombat != null) playerCombat.SetTemporaryInvulnerability(dashDuration);
        
        // 3. Play Animation
        if (playerAnim != null) playerAnim.PlayDash(dashDuration);

        // 4. Apply Velocity (Ignore Gravity)
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        
        // v = d / t
        float speed = dashDistance / dashDuration;
        rb.linearVelocity = new Vector2(speed * direction, 0f); 

        Debug.Log($"[PlayerDash] Dashing! Dir: {direction}, Speed: {speed}");

        yield return new WaitForSeconds(dashDuration);

        // 4. Reset
        rb.linearVelocity = Vector2.zero; // Stop momentum
        rb.gravityScale = originalGravity;
        
        if (playerMovement != null) playerMovement.IsExternalMovementActive = false;
        
        isDashing = false;
    }
}
