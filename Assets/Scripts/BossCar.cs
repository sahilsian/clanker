using UnityEngine;
using System.Collections;

public class BossCar : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 3f;
    public float chargeSpeed = 12f;
    public float stunDuration = 3.0f;
    public int maxHealth = 10; // Increased health since we now use damage numbers
    public int contactDamage = 2;
    public float chargeTiltAngle = 15f; 
    
    [Header("Tracking Feel")]
    public float trackingSmoothTime = 0.5f;
    public float detectionRadius = 10f;
    public float chargeRange = 6f;
    public float flipThreshold = 1.0f;

    [Header("References")]
    public Transform player;
    public SpriteRenderer spriteRenderer;
    public Color chargeColor = Color.red;
    public Color stunColor = Color.yellow;

    private Rigidbody2D rb;
    private bool isStunned = false;
    private bool isCharging = false;
    
    private Coroutine currentBehavior;
    private float currentVelocityX; 

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentBehavior = StartCoroutine(BossLoop());
    }

    // ... [Movement/Charge Logic remains the same as previous] ...
    private IEnumerator BossLoop()
    {
        while (maxHealth > 0)
        {
            // APPROACH
            transform.rotation = Quaternion.identity; 
            bool readyToCharge = false;
            while (!readyToCharge)
            {
                float distToPlayer = player.position.x - transform.position.x;
                float absDist = Mathf.Abs(distToPlayer);
                if (absDist <= chargeRange) readyToCharge = true;
                else
                {
                    if (absDist > flipThreshold)
                    {
                        float facingDir = Mathf.Sign(distToPlayer);
                        transform.localScale = new Vector3(-facingDir, 1, 1);
                    }
                    float targetSpeed = 0f;
                    if (absDist <= detectionRadius) targetSpeed = Mathf.Sign(distToPlayer) * moveSpeed;
                    float newX = Mathf.SmoothDamp(rb.linearVelocity.x, targetSpeed, ref currentVelocityX, trackingSmoothTime);
                    rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
                }
                yield return null;
            }

            // CHARGE
            isCharging = true; 
            rb.linearVelocity = Vector2.zero; 
            currentVelocityX = 0f; 
            spriteRenderer.color = chargeColor; 
            float chargeDirection = Mathf.Sign(player.position.x - transform.position.x);
            transform.localScale = new Vector3(-chargeDirection, 1, 1);
            transform.rotation = Quaternion.Euler(0, 0, -chargeDirection * chargeTiltAngle);
            yield return new WaitForSeconds(0.5f); 
            rb.linearVelocity = new Vector2(chargeDirection * chargeSpeed, 0);
            yield return new WaitForSeconds(1.5f); 
            
            // RESET
            rb.linearVelocity = Vector2.zero;
            transform.rotation = Quaternion.identity; 
            isCharging = false; 
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(1.0f);
        }
    }

    // --- UPDATED DAMAGE LOGIC ---
    
    // Overload to keep PlayerMovement.cs working (for Stomps)
    public void TakeDamage(string type)
    {
        TakeDamage(1, type); // Default to 1 damage if amount isn't specified
    }

    public void TakeDamage(int damageAmount, string type)
    {
        // 1. STOMP LOGIC (Trigger Stun)
        if (type == "Stomp")
        {
            if (isCharging) 
            {
                Debug.Log("<color=yellow>BOSS STUNNED</color> by Stomp!");
                if (currentBehavior != null) StopCoroutine(currentBehavior);
                rb.linearVelocity = Vector2.zero;
                isCharging = false;
                currentBehavior = StartCoroutine(StunRoutine());
            }
        }
        
        // 2. DAMAGE LOGIC (Kick/Punch)
        else if (type == "Kick" || type == "Punch")
        {
            if (isStunned)
            {
                maxHealth -= damageAmount;
                Debug.Log($"BOSS HIT by {type}! Damage: {damageAmount}. HP: {maxHealth}");

                if (maxHealth <= 0)
                {
                    Die();
                }
                else
                {
                    StartCoroutine(FlashHitFeedback());
                }
            }
            else
            {
                Debug.Log("Boss armor deflected the attack! (Must stun first)");
            }
        }
    }

    private IEnumerator FlashHitFeedback()
    {
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        if(isStunned) spriteRenderer.color = stunColor;
    }

    private IEnumerator StunRoutine()
    {
        isStunned = true;
        spriteRenderer.color = stunColor; 
        transform.rotation = Quaternion.identity; 
        yield return new WaitForSeconds(stunDuration);
        isStunned = false;
        spriteRenderer.color = Color.white;
        currentBehavior = StartCoroutine(BossLoop());
    }

    private void Die()
    {
        Debug.Log("Boss Destroyed!");
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chargeRange);
    }
}