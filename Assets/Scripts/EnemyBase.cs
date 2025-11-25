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
        currentHealth = maxHealth;
        
        // Auto-fetch SpriteRenderer if not manually assigned
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Store the starting color (usually white) so we can return to it after flashing red
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    // This is the function PlayerCombat and PlayerMovement call
    public void TakeDamage(int damageAmount, string attackType)
    {
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
        Debug.Log($"<color=red>ENEMY DEFEATED</color> by {lastHitType}!");
        
        // TODO: Instantiate an explosion particle effect here if you have one
        
        Destroy(gameObject);
    }

    private IEnumerator FlashRed()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hitColor; // Flash Red
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor; // Return to normal
        }
    }
}