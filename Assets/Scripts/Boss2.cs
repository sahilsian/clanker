using UnityEngine;

public class Boss2 : MonoBehaviour
{
    public enum DroneState
    {
        Idle,       // Waiting at position
        FlyToPlayer,// Moving horizontally to player X
        Lunge,      // Diving at player (X and Y)
        Stunned,    // Hit pillar, falling/stuck on ground
        Return      // Flying back to start position
    }

    [Header("Stats")]
    public int maxHealth = 15;
    [SerializeField] private int currentHealth;
    public int contactDamage = 1;

    [Header("References")]
    public Transform player;

    [Header("Detection Zone")]
    [Range(1f, 40f)]
    public float detectionWidth = 10f;  // Horizontal detection range
    [Range(1f, 50f)]
    public float detectionRange = 15f; // Vertical detection range (downward)

    [Header("Movement")]
    public float flySpeed = 8f;
    
    [Header("Lunge Attack")]
    [Range(0.5f, 5f)]
    public float chargeRange = 2f;     // How close before lunging
    public float lungeSpeed = 15f;     // Speed of the lunge
    public float lungeCooldown = 2f;   // Time before can lunge again
    private float lungeCooldownTimer = 0f;

    [Header("Vulnerable (After Miss)")]
    public float vulnerableDuration = 3f;  // Time player can stomp
    public bool canBeStomped = false;      // Is currently vulnerable?
    private Collider2D myCollider;

    private Rigidbody2D rb;
    private DroneState currentState;
    private Vector3 targetPosition;
    private Vector3 startPosition;
    private bool hasHitThisLunge = false; // Prevents multiple hits per lunge
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        rb.gravityScale = 0; // Always float
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Prevent tumbling
        startPosition = transform.position;
        currentHealth = maxHealth;

        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        currentState = DroneState.Idle;
    }

    private void Update()
    {
        switch (currentState)
        {
            case DroneState.Idle:
                IdleUpdate();
                break;
            case DroneState.FlyToPlayer:
                FlyToPlayerUpdate();
                break;
            case DroneState.Lunge:
                LungeUpdate();
                break;
            case DroneState.Return:
                ReturnUpdate();
                break;
        }
    }

    private void IdleUpdate()
    {
        // Stay in place
        rb.linearVelocity = Vector2.zero;

        // Tick cooldown
        if (lungeCooldownTimer > 0)
        {
            lungeCooldownTimer -= Time.deltaTime;
            return; // Don't detect while on cooldown
        }

        // Check if player is in detection zone
        if (player != null)
        {
            float distX = Mathf.Abs(player.position.x - transform.position.x);
            float distY = transform.position.y - player.position.y; // Player should be below

            if (distX < detectionWidth && distY > 0 && distY < detectionRange)
            {
                // Player detected! Lock target X only (keep same Y)
                targetPosition = new Vector3(player.position.x, transform.position.y, transform.position.z);
                currentState = DroneState.FlyToPlayer;
                Debug.Log("Boss2: Player detected! Flying to player X position.");
            }
        }
    }

    private void FlyToPlayerUpdate()
    {
        // Move toward target X position only (horizontal movement)
        float directionX = Mathf.Sign(targetPosition.x - transform.position.x);
        rb.linearVelocity = new Vector2(directionX * flySpeed, 0);

        // Check if we're close enough to lunge (within charge range)
        float distToPlayerX = Mathf.Abs(player.position.x - transform.position.x);
        if (distToPlayerX < chargeRange)
        {
            // Lock player's current position and LUNGE!
            targetPosition = player.position;
            currentState = DroneState.Lunge;
            hasHitThisLunge = false; // Reset hit flag for new lunge
            Debug.Log("Boss2: In charge range! LUNGE!");
        }
    }

    private void LungeUpdate()
    {
        // Move toward target position (both X and Y)
        Vector2 direction = ((Vector2)targetPosition - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * lungeSpeed;

        // Check if we reached the target (missed the player)
        if (Vector2.Distance(transform.position, targetPosition) < 0.5f)
        {
            rb.linearVelocity = Vector2.zero;
            Debug.Log("Boss2: Missed player! Now vulnerable to stomp.");
            StartCoroutine(VulnerableRoutine());
        }
    }

    private void ReturnUpdate()
    {
        // Move back to start position
        Vector2 direction = ((Vector2)startPosition - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * flySpeed;

        // Check if we reached start
        if (Vector2.Distance(transform.position, startPosition) < 0.5f)
        {
            transform.position = startPosition;
            rb.linearVelocity = Vector2.zero;
            currentState = DroneState.Idle;
            lungeCooldownTimer = lungeCooldown; // Start cooldown
            Debug.Log($"Boss2: Back at start. Cooldown: {lungeCooldown}s");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Boss2: OnCollisionEnter2D with {collision.gameObject.name} (Layer: {collision.gameObject.layer}), State: {currentState}");
        
        if (currentState != DroneState.Lunge) return;

        HandleCollision(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Boss2: OnTriggerEnter2D with {other.gameObject.name} (Layer: {other.gameObject.layer}), State: {currentState}");
        
        if (currentState != DroneState.Lunge) return;

        HandleCollision(other.gameObject);
    }

    private void HandleCollision(GameObject other)
    {
        // Absolute prevention of multiple hits
        if (hasHitThisLunge) return;
        if (currentState != DroneState.Lunge) return;

        // If we hit the player during Lunge, deal damage and return
        if (other.CompareTag("Player"))
        {
            // Lock immediately to prevent any more damage
            hasHitThisLunge = true;
            currentState = DroneState.Return;
            rb.linearVelocity = Vector2.zero;

            PlayerCombat pc = other.GetComponent<PlayerCombat>();
            if (pc != null)
            {
                pc.TakeDamage(contactDamage, "Boss2Lunge");
                Debug.Log("Boss2: Hit player! Returning to start.");
            }
            return;
        }
    }

    private System.Collections.IEnumerator VulnerableRoutine()
    {
        currentState = DroneState.Stunned;
        canBeStomped = true;
        rb.linearVelocity = Vector2.zero;
        Debug.Log($"Boss2: Vulnerable for {vulnerableDuration}s - STOMP NOW!");

        // Wobble effect and BLINK yellow while vulnerable (waiting for stomp)
        float elapsed = 0f;
        while (elapsed < vulnerableDuration && canBeStomped)
        {
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 15f) * 8f);
            
            // Blink between yellow and original color
            if (spriteRenderer != null)
            {
                spriteRenderer.color = (Mathf.Sin(Time.time * 10f) > 0) ? Color.yellow : originalColor;
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset rotation and restore original color
        transform.rotation = Quaternion.identity;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        canBeStomped = false;
        
        if (currentState == DroneState.Stunned)
        {
            currentState = DroneState.Return;
            Debug.Log("Boss2: Vulnerability ended. Returning to start.");
        }
    }

    // --- STOMP INTERFACE (called by PlayerControl.cs) ---
    public bool CanBeStomp()
    {
        return canBeStomped && currentState == DroneState.Stunned;
    }

    public void TakeStomp(int damage)
    {
        if (!canBeStomped) return;

        Debug.Log("Boss2: STOMPED! Now fully stunned and vulnerable to attacks.");
        
        // Stop wobble and become truly stunned (allow punches/kicks)
        canBeStomped = false; // No more stomping needed
        StopAllCoroutines();  // Stop the wobble
        transform.rotation = Quaternion.identity;
        
        // Start a longer stun where player can punch/kick
        StartCoroutine(StunnedAfterStomp());
    }

    private System.Collections.IEnumerator StunnedAfterStomp()
    {
        // Keep yellow color during extended stun
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.yellow;
        }
        
        // Stay stunned for longer, player can now punch/kick
        yield return new WaitForSeconds(vulnerableDuration);
        
        // Restore original color when stun ends
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        currentState = DroneState.Return;
        Debug.Log("Boss2: Stun ended. Returning to start.");
    }

    // Method for taking damage from punches/kicks (call from PlayerCombat)
    public void TakeDamage(int damage, string attackType)
    {
        if (currentState != DroneState.Stunned) 
        {
            Debug.Log("Boss2: Not stunned, damage ignored!");
            return;
        }

        currentHealth -= damage;
        Debug.Log($"Boss2: Took {damage} damage from {attackType}. HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(FlashDamage());
        }
    }

    private void Die()
    {
        Debug.Log("Boss2: DEFEATED!");
        Destroy(gameObject);
    }

    private System.Collections.IEnumerator FlashDamage()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color original = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = original;
        }
    }

    private void OnDrawGizmos()
    {
        // Detection Zone Visualization
        Vector3 pos = Application.isPlaying ? startPosition : transform.position;
        
        Gizmos.color = Color.green;

        // Draw rectangular detection zone below the drone
        Vector3 topLeft = pos + Vector3.left * detectionWidth;
        Vector3 topRight = pos + Vector3.right * detectionWidth;
        Vector3 bottomLeft = topLeft + Vector3.down * detectionRange;
        Vector3 bottomRight = topRight + Vector3.down * detectionRange;

        // Draw the box
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topLeft, bottomLeft);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomLeft, bottomRight);

        // Draw center line
        Gizmos.color = Color.red;
        Gizmos.DrawLine(pos, pos + Vector3.down * detectionRange);

        // Draw charge range circle
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pos, chargeRange);
    }
}
