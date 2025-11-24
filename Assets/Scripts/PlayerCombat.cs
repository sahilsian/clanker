using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Player Stats")]
    public int maxHealth = 5;
    private int currentHealth;

    private PlayerMovement playerMovement;

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

    // --- INPUT LISTENERS ---
    
    public void OnPunch(InputValue value)
    {
        if (value.isPressed)
        {
            PerformAttack(punchPoint, punchRange, punchDamage, "Punch");
        }
    }

    public void OnKick(InputValue value)
    {
        if (value.isPressed)
        {
            PerformAttack(kickPoint, kickRange, kickDamage, "Kick");
        }
    }

    // --- CORE ATTACK LOGIC ---
    private void PerformAttack(Transform attackPoint, float range, int damage, string attackType)
    {
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

    private void Start()
    {
        currentHealth = maxHealth;
        playerMovement = GetComponent<PlayerMovement>();
    }

    public void TakeDamage(int amount, string source = "Contact")
    {
        currentHealth -= amount;
        Debug.Log($"<color=red>PLAYER HURT:</color> Took {amount} damage from {source}. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("<color=magenta>PLAYER DIED</color>");
        // TODO: Trigger death animation / respawn. For now just disable the GameObject
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If player is stomping (hit enemy from above), don't take contact damage
        if (playerMovement != null && playerMovement.IsStomping) return;

        // Enemy contact
        EnemyBase enemy = collision.gameObject.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            TakeDamage(enemy.contactDamage, "EnemyContact");
            return;
        }

        // Boss contact
        BossCar boss = collision.gameObject.GetComponent<BossCar>();
        if (boss != null)
        {
            TakeDamage(boss.contactDamage, "BossContact");
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