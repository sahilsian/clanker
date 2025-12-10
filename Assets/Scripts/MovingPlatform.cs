using UnityEngine;
using System.Collections;

public class MovingPlatform : MonoBehaviour
{
    [Header("Waypoints")]
    [Tooltip("Array of Transform objects that define the platform's path")]
    public Transform[] waypoints;

    [Header("Movement Settings")]
    [Tooltip("Speed at which the platform moves between waypoints")]
    public float moveSpeed = 2f;

    [Tooltip("How long to wait at each waypoint before moving to the next")]
    public float waitTime = 1f;

    [Tooltip("If true, platform will loop back to the first waypoint. If false, it will ping-pong")]
    public bool loop = true;

    [Header("Debug")]
    [Tooltip("Show the platform's path in the editor")]
    public bool showPath = true;

    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private bool movingForward = true; // Used for ping-pong mode

    private void Start()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("MovingPlatform: No waypoints assigned!");
            enabled = false;
            return;
        }

        // Start at the first waypoint
        transform.position = waypoints[0].position;
        StartCoroutine(MovePlatform());
    }

    private IEnumerator MovePlatform()
    {
        while (true)
        {
            // Get current and next waypoint
            Transform targetWaypoint = waypoints[currentWaypointIndex];

            // Move towards the target waypoint
            while (Vector3.Distance(transform.position, targetWaypoint.position) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetWaypoint.position,
                    moveSpeed * Time.deltaTime
                );
                yield return null;
            }

            // Snap to exact position
            transform.position = targetWaypoint.position;

            // Wait at waypoint
            yield return new WaitForSeconds(waitTime);

            // Move to next waypoint
            if (loop)
            {
                // Loop mode: always move forward, wrap around at the end
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
            else
            {
                // Ping-pong mode: reverse direction at the ends
                if (movingForward)
                {
                    currentWaypointIndex++;
                    if (currentWaypointIndex >= waypoints.Length)
                    {
                        currentWaypointIndex = waypoints.Length - 2;
                        movingForward = false;
                    }
                }
                else
                {
                    currentWaypointIndex--;
                    if (currentWaypointIndex < 0)
                    {
                        currentWaypointIndex = 1;
                        movingForward = true;
                    }
                }
            }
        }
    }

    // Make the player stick to the platform
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Parent the player to the platform so it moves with it
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Unparent the player when they leave the platform
            collision.transform.SetParent(null);
        }
    }

    // Draw the path in the editor for visualization
    private void OnDrawGizmos()
    {
        if (!showPath || waypoints == null || waypoints.Length < 2)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            // Draw waypoint sphere
            Gizmos.DrawWireSphere(waypoints[i].position, 0.3f);

            // Draw line to next waypoint
            int nextIndex = loop ? (i + 1) % waypoints.Length : i + 1;
            if (nextIndex < waypoints.Length && waypoints[nextIndex] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[nextIndex].position);
            }
        }

        // In ping-pong mode, draw the return path
        if (!loop && waypoints.Length > 1)
        {
            int lastIndex = waypoints.Length - 1;
            if (waypoints[lastIndex] != null && waypoints[0] != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(waypoints[lastIndex].position, waypoints[0].position);
            }
        }
    }
}
