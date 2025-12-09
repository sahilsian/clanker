using UnityEngine;

public class BossMovement : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 3f;
    public float detectionRange = 10f;
    public float stopDistance = 2f;
    public bool invertFacing = false; // Check this if sprite faces left by default

    [Header("References")]
    public Transform player;

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= detectionRange && distance > stopDistance)
        {
            MoveTowardsPlayer();
        }
    }

    private void MoveTowardsPlayer()
    {
        // 1. Move
        // Move towards player position
        transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

        // 2. Face Player logic REMOVED as requested (keep original orientation)
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
