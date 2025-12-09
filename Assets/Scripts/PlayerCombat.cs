using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    [Header("Player Stats")]
    public int maxHealth = 5;
    private int currentHealth;

    private PlayerMovement playerMovement;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isInvulnerable = false;
    private PlayerSkeletalAnimation playerAnimation;
    [Header("Invulnerability")]
    public float invulDuration = 0.6f;

    [Header("Combat Settings")]
    public LayerMask enemyLayer;
    
    [Header("Light Attack (Punch - 'L')")]
    public Transform punchPoint;
    public float punchRange = 0.5f;
    public int punchDamage = 1; // Light damage

    [Header("Heavy Attack (Kick - 'K')")]
    public Transform kickPoint;
    public float kickRange = 0.7f;
    public int kickDamage = 3; // Heavy damage

    private void Start()
    {
        currentHealth = maxHealth;
        playerMovement = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
        playerAnimation = GetComponent<PlayerSkeletalAnimation>();
        if (playerAnimation == null) playerAnimation = GetComponentInChildren<PlayerSkeletalAnimation>();
    }

    // --- INPUT LISTENERS ---
    
    public void OnPunch(InputValue value)
    {
        if (value.isPressed)
        {
            // Prevent spamming/interrupting attacks
            if (playerAnimation != null && playerAnimation.IsAttacking) return;

            // Play punch animation
            playerAnimation?.PlayAttack("Punch");
            PerformAttack(punchPoint, punchRange, punchDamage, "Punch");
        }
    }

    public void OnKick(InputValue value)
    {
        if (value.isPressed)
        {
            // Prevent spamming/interrupting attacks
            if (playerAnimation != null && playerAnimation.IsAttacking) return;

            // Play kick animation
            playerAnimation?.PlayAttack("Kick");
            PerformAttack(kickPoint, kickRange, kickDamage, "Kick");
        }
    }

    // --- CORE ATTACK LOGIC ---
    private void PerformAttack(Transform attackPoint, float range, int damage, string attackType)
    {
        // Visual log for the player action
        Debug.Log($"<color=cyan>PLAYER ACTION:</color> Performed {attackType} (Damage: {damage})");

        // Trigger Frenzy Visual Effect if active
        PlayerFrenzy frenzy = GetComponent<PlayerFrenzy>();
        if (frenzy != null && frenzy.IsFrenzyActive)
        {
            frenzy.PlayAttackEffect(attackType, attackPoint.position);
        }

        // Detect enemies
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, range, enemyLayer);

        foreach (Collider2D hit in hitEnemies)
        {
            // 1. Check for Normal Enemies
            EnemyBase genericEnemy = hit.GetComponent<EnemyBase>();
            if (genericEnemy != null)
            {
                genericEnemy.ApplyDamage(damage, attackType);
            }

            // 2. Check for Boss (Boss handles damage differently)
            BossCar boss = hit.GetComponent<BossCar>();
            if (boss != null)
            {
                boss.TakeDamage(damage, attackType);
            }
        }
    }

    // Returns true if damage was actually applied (not invulnerable)
    public bool TakeDamage(int amount, string source = "Contact")
    {
        if (isInvulnerable || currentHealth <= 0) return false;

        currentHealth -= amount;
        Debug.Log($"<color=red>PLAYER HURT:</color> Took {amount} damage from {source}. HP: {currentHealth}/{maxHealth}");

        // Immediately mark invulnerable to prevent other hits in the same frame
        isInvulnerable = true;

        // Trigger hurt animation if available
        playerAnimation?.PlayHurt();
        // Lock player input for hurt duration if movement component exists
        if (playerMovement != null)
        {
            playerMovement.LockInput(playerMovement.hurtInputLockDuration);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(HurtInvulnerability());
        }

        return true;
    }

    private IEnumerator HurtInvulnerability()
    {
        if (spriteRenderer == null)
        {
            // sprite absent: just wait and then clear invulnerability
            yield return new WaitForSeconds(invulDuration);
            isInvulnerable = false;
            yield break;
        }

        float elapsed = 0f;
        float flashInterval = 0.1f;
        bool toggle = false;

        while (elapsed < invulDuration)
        {
            spriteRenderer.color = toggle ? Color.red : originalColor;
            toggle = !toggle;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        spriteRenderer.color = originalColor;
        isInvulnerable = false;
    }

    private void Die()
    {
        Debug.Log("<color=magenta>PLAYER DIED</color>");
        // TODO: Trigger death animation / respawn. For now just disable the GameObject
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Allows external scripts (like Dash) to trigger invulnerability.
    /// </summary>
    public void SetTemporaryInvulnerability(float duration)
    {
        if (isInvulnerable) return; // Already safe
        StartCoroutine(ExternalInvulRoutine(duration));
    }

    private IEnumerator ExternalInvulRoutine(float duration)
    {
        isInvulnerable = true;
        // Debug.Log($"<color=yellow>PLAYER INVULNERABLE</color> for {duration}s");
        
        // Ghostly effect
        if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);

        yield return new WaitForSeconds(duration);

        isInvulnerable = false;
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If player is stomping (hit enemy from above), don't take contact damage
        if (playerMovement != null && playerMovement.IsStomping) return;

        // Enemy contact
        EnemyBase enemy = collision.gameObject.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            bool damaged = TakeDamage(enemy.contactDamage, "EnemyContact");

            // Apply knockback away from the enemy only if damage was applied
            if (damaged && playerMovement != null)
            {
                Vector2 dir = (transform.position - collision.transform.position).normalized;
                playerMovement.ApplyKnockback(dir, playerMovement.knockbackForce);
            }

            return;
        }

        // Boss contact
        BossCar boss = collision.gameObject.GetComponent<BossCar>();
        if (boss != null)
        {
            bool damaged = TakeDamage(boss.contactDamage, "BossContact");

            // Apply knockback away from the boss only if damage was applied
            if (damaged && playerMovement != null)
            {
                Vector2 dir = (transform.position - collision.transform.position).normalized;
                playerMovement.ApplyKnockback(dir, playerMovement.knockbackForce);
            }

            return;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (punchPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(punchPoint.position, punchRange);
        }

        if (kickPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(kickPoint.position, kickRange);
        }
    }
}