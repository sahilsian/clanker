using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    [Header("UI")]
    public HealthBar healthBar;

    [Header("Player Stats")]
    public int maxHealth = 5;
    private int currentHealth;

    private PlayerMovement playerMovement;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isInvulnerable = false;
    private PlayerAnimation playerAnimation;
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
        // Initialize health, cache components, and wire up the health UI
        currentHealth = maxHealth;
        playerMovement = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
        playerAnimation = GetComponent<PlayerAnimation>();
        if (playerAnimation == null) playerAnimation = GetComponentInChildren<PlayerAnimation>();

        // Hook health UI if present
        if (healthBar == null) healthBar = FindObjectOfType<HealthBar>();
        if (healthBar != null)
        {
            healthBar.maxHealth = maxHealth;
            healthBar.SetHealth(currentHealth);
        }
    }

    // --- INPUT LISTENERS ---
    
    public void OnPunch(InputValue value)
    {
        // Light attack input handler
        if (value.isPressed)
        {
            // Play punch animation
            playerAnimation?.PlayAttack("Punch");
            PerformAttack(punchPoint, punchRange, punchDamage, "Punch");
        }
    }

    public void OnKick(InputValue value)
    {
        // Heavy attack input handler
        if (value.isPressed)
        {
            // Play kick animation
            playerAnimation?.PlayAttack("Kick");
            PerformAttack(kickPoint, kickRange, kickDamage, "Kick");
        }
    }

    // --- CORE ATTACK LOGIC ---
    private void PerformAttack(Transform attackPoint, float range, int damage, string attackType)
    {
        // Detect enemies in range and deliver damage by type
        // Visual log for the player action
        Debug.Log($"<color=cyan>PLAYER ACTION:</color> Performed {attackType} (Damage: {damage})");

        // Detect enemies
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, range, enemyLayer);

        foreach (Collider2D hit in hitEnemies)
        {
            // 1. Check for Normal Enemies
            EnemyBase genericEnemy = hit.GetComponent<EnemyBase>();
            if (genericEnemy != null)
            {
                genericEnemy.TakeDamage(damage, attackType);
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
        // Apply incoming damage, play hurt feedback, and handle death
        if (isInvulnerable || currentHealth <= 0) return false;

        currentHealth -= amount;
        Debug.Log($"<color=red>PLAYER HURT:</color> Took {amount} damage from {source}. HP: {currentHealth}/{maxHealth}");

        // Immediately mark invulnerable to prevent other hits in the same frame
        isInvulnerable = true;

        // Update UI
        if (healthBar != null) healthBar.SetHealth(currentHealth);

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
        // Flash the sprite and block further damage for a short duration
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
        // Disable the player and trigger the game over sequence
        Debug.Log("<color=magenta>PLAYER DIED</color>");
        // Disable the Player and show Game Over UI
        gameObject.SetActive(false);
        if (healthBar != null) healthBar.SetHealth(0);
        GameManager.Instance?.ShowGameOver();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Apply contact damage and knockback when colliding with enemies or boss
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
        // Draw hitbox ranges when selected in the editor
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
