using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 50;
    private int currentHealth;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    public float flashDuration = 0.1f;
    public float shakeDuration = 0.1f;
    public float shakeAmount = 0.1f;    
    private Vector3 originalPosition;

    private void Start()
    {
        // Initialize health and cache visuals for feedback
        currentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        originalPosition = transform.position;
    }

    public void TakeDamage(int damage)
    {
        // Reduce health, play feedback, and destroy the enemy if depleted
        currentHealth -= damage;
        Debug.Log("Enemy HP: " + currentHealth);

        StartCoroutine(FlashRed());
        StartCoroutine(Shake());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator FlashRed()
    {
        // Flash the sprite red briefly to show a hit
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    IEnumerator Shake()
    {
        // Jitter the enemy position slightly to emphasize the hit
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-1f, 1f) * shakeAmount;
            float offsetY = Random.Range(-1f, 1f) * shakeAmount;

            transform.position = originalPosition + new Vector3(offsetX, offsetY, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
    }

    void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnemyDefeated();
        }
        else {
            Debug.LogError("GameManager.Instance is NULL!!!  doesn't have GameManager£¿");

        }
        // Remove the enemy object from the scene
        Debug.Log("EnemyDefeated() sent to GameManager");

        Destroy(gameObject);
    }
}
