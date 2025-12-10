using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player; // drag your player here
    public Vector3 offset;   // optional offset to keep background centered

    void LateUpdate()
    {
        // Keep this object aligned with the player (with optional offset)
        if (player != null)
        {
            // Keep the background at the player's position plus offset
            transform.position = new Vector3(player.position.x + offset.x, player.position.y + offset.y, transform.position.z);
        }
    }
}
