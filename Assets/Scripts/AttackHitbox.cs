using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public int damage = 15;
    private void OnTriggerStay2D(Collider2D other)
    {
     
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
      

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
