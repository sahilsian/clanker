using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public int damage = 15;
    private void OnTriggerStay2D(Collider2D other)
    {
        // Reserved for any future continuous-hit logic
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Apply damage the first time the hitbox collides with an enemy
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }
}
