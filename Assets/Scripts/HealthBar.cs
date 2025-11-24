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
        currentHealth = maxHealth;
        UpdateBar();
    }

    public void SetHealth(float newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0f, maxHealth);
        UpdateBar();
    }

    public void AddDamage(float amount)
    {
        SetHealth(currentHealth - amount);
    }

    public void AddHeal(float amount)
    {
        SetHealth(currentHealth + amount);
    }

    void UpdateBar()
    {
        if (fillImage != null && maxHealth > 0f)
        {
            fillImage.fillAmount = currentHealth / maxHealth;
        }
    }
}
