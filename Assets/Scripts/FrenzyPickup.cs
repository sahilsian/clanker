using UnityEngine;

public class FrenzyPickup : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Optional: Rotation speed for the pickup visual")]
    public float rotationSpeed = 100f;

    private void Update()
    {
        // Simple visual rotation
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object colliding is the player
        // We look for the PlayerFrenzy component specifically
        PlayerFrenzy playerFrenzy = other.GetComponent<PlayerFrenzy>();
        
        // If not found on the collider, check parent (common in Unity hierarchies)
        if (playerFrenzy == null)
        {
            playerFrenzy = other.GetComponentInParent<PlayerFrenzy>();
        }

        if (playerFrenzy != null)
        {
            playerFrenzy.ActivateFrenzy();
            Destroy(gameObject);
        }
    }
}
