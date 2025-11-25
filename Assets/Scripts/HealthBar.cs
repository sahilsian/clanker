using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("UI")]
    public Image fillImage;

    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;

    void Start()
    {
        // Initialize the bar to full health
        currentHealth = maxHealth;
        UpdateBar();
    }

    public void SetHealth(float newHealth)
    {
        // Set health to a clamped value and refresh the UI fill
        currentHealth = Mathf.Clamp(newHealth, 0f, maxHealth);
        UpdateBar();
    }

    public void AddDamage(float amount)
    {
        // Subtract health by the given amount
        SetHealth(currentHealth - amount);
    }

    public void AddHeal(float amount)
    {
        // Restore health by the given amount
        SetHealth(currentHealth + amount);
    }

    void UpdateBar()
    {
        // Update the image fill based on the current health ratio
        if (fillImage != null && maxHealth > 0f)
        {
            fillImage.fillAmount = currentHealth / maxHealth;
        }
    }
}
