using UnityEngine;
using System.Collections.Generic;

public class FrenzyHitbox : MonoBehaviour
{
    [Header("Hitbox Stats")]
    public int damage = 5;
    public float knockbackForce = 10f;
    public LayerMask enemyLayer;


    private BoxCollider2D boxCollider;
    private HashSet<GameObject> hitObjects = new HashSet<GameObject>();

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void OnEnable()
    {
        Debug.Log($"[FrenzyHitbox] Enabled on {gameObject.name}");
        hitObjects.Clear();
    }

    private void FixedUpdate()
    {
        if (boxCollider == null) return;

        // Use the collider's bounds for the overlap check
        // Note: OverlapBox uses center and size. bounds.center and bounds.size work perfectly.
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCollider.bounds.center, boxCollider.bounds.size, transform.eulerAngles.z, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            if (hitObjects.Contains(hit.gameObject)) continue;

            // Log what we found
            Debug.Log($"[FrenzyHitbox] Overlap detected: {hit.name}");

            // Mark as hit so we don't damage twice in one swing
            hitObjects.Add(hit.gameObject);

            // 1. Check for EnemyBase
            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                Debug.Log($"[FrenzyHitbox] Hit {hit.name} for {damage} damage!");
                enemy.ApplyDamage(damage, "FrenzyAttack");
            }

            // 2. Check for BossCar
            BossCar boss = hit.GetComponent<BossCar>();
            if (boss != null)
            {
                Debug.Log($"[FrenzyHitbox] Hit Boss for {damage} damage!");
                boss.TakeDamage(damage, "FrenzyAttack");
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (boxCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(boxCollider.bounds.center, boxCollider.bounds.size);
        }
    }
}
