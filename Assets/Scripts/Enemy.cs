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
        currentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        originalPosition = transform.position;
    }

    public void TakeDamage(int damage)
    {
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
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    IEnumerator Shake()
    {
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
        Destroy(gameObject);
    }
}
