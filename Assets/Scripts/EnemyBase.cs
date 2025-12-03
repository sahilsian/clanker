using UnityEngine;
using System.Collections;

public class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 5;
    public int contactDamage = 1;
    [SerializeField] private int currentHealth;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color hitColor = Color.red;
    private Color originalColor;

    private void Start()
    {
        // Initialize health and cache renderer/color for flash feedback
        currentHealth = maxHealth;
        
        // Auto-fetch SpriteRenderer if not manually assigned
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Store the starting color (usually white) so we can return to it after flashing red
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    // This is the function PlayerCombat and PlayerMovement call
    public void TakeDamage(int damageAmount, string attackType)
    {
        // Apply damage, trigger feedback, and destroy the enemy when health is gone
        currentHealth -= damageAmount;

        Debug.Log($"<color=orange>ENEMY HIT:</color> Registered <b>{attackType}</b>. Damage: {damageAmount}. Health remaining: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die(attackType);
        }
        else
        {
            StartCoroutine(FlashRed());
        }
    }

    private void Die(string lastHitType)
    {
        // Handle enemy death and log which attack finished it
        Debug.Log($"<color=red>ENEMY DEFEATED</color> by {lastHitType}!");
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance 是 NULL！请检查场景里是否有 GameManager 并正确挂脚本");
        }
        else
        {
            GameManager.Instance.EnemyDefeated();
            Debug.Log("EnemyBase 已通知 GameManager 敌人死亡");
        }
        // TODO: Instantiate an explosion particle effect here if you have one

        Destroy(gameObject);
    }

    private IEnumerator FlashRed()
    {
        // Flash red briefly to show that damage was taken
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hitColor; // Flash Red
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor; // Return to normal
        }
    }
}
