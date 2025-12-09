using UnityEngine;
using UnityEngine.InputSystem; // REQUIRED for the new system

public class GrapplingHook : MonoBehaviour
{
    [Header("Settings")]
    public float detectRange = 5f;
    public LayerMask grappleLayer;
    public float swingForce = 10f;
    public float jumpOffForce = 10f;
    
    [Header("Rope Climbing")]
    public float climbSpeed = 3f; // How fast the player climbs up/down the rope
    public float minRopeLength = 0.5f; // Minimum rope length when fully climbed up

    [Header("References")]
    public DistanceJoint2D distanceJoint;
    public LineRenderer ropeRenderer;
    public PlayerSkeletalAnimation playerAnimation;
    public PlayerMovement playerMovement;
    [Header("Debug")]
    public bool enableDebugLogs = true;
    public string debugTag = "[Grapple]";
    
    private Rigidbody2D rb;
    private Vector2 targetAnchor;
    private bool isGrappling = false;
    private float maxRopeLength; // Stores the initial rope length (max we can extend to)
    // For debug: track E key state to log press/release events
    private bool prevEPressed = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        distanceJoint.enabled = false;
        if (ropeRenderer != null) ropeRenderer.enabled = false;
        if (playerAnimation == null) playerAnimation = GetComponent<PlayerSkeletalAnimation>();
        if (playerAnimation == null) playerAnimation = GetComponentInChildren<PlayerSkeletalAnimation>();
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
        if (enableDebugLogs) Debug.Log($"{debugTag} GrapplingHook started. detectRange={detectRange}, swingForce={swingForce}");
    }

    void Update()
    {
        // Make sure keyboard exists to prevent errors
        if (Keyboard.current == null) return;

        // Detect E key press/release state changes for debugging (avoids spamming every frame)
        bool ePressed = Keyboard.current.eKey.isPressed;
        if (enableDebugLogs && ePressed != prevEPressed)
        {
            if (ePressed)
            {
                if (Keyboard.current.eKey.wasPressedThisFrame)
                    Debug.Log($"{debugTag} E key just pressed (wasPressedThisFrame=true)");
                else
                    Debug.Log($"{debugTag} E key pressed (held)");
            }
            else
            {
                Debug.Log($"{debugTag} E key released");
            }
        }
        prevEPressed = ePressed;

        // 1. DETECT & START GRAPPLE (Key: E)
        // Old: Input.GetKeyDown(KeyCode.E)
        if (Keyboard.current.eKey.wasPressedThisFrame) 
        {
            TryStartGrapple();
        }

        // 2. STOP GRAPPLE / JUMP OFF (Key: Space)
        // Old: Input.GetKeyDown(KeyCode.Space)
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrappling)
        {
            JumpOff();
        }

        // 3. SWING CONTROL & ROPE CLIMBING
        if (isGrappling)
        {
            HandleSwingMovement();
            HandleRopeClimbing();
            UpdateRopeVisuals();
        }
    }

    void TryStartGrapple()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectRange, grappleLayer);

        Collider2D best = null;
        Vector2 bestPoint = Vector2.zero;
        float bestDist = float.MaxValue;

        foreach (var c in colliders)
        {
            if (c == null) continue;
            // Ignore self (player) colliders to avoid "connects to itself" errors
            if (c.attachedRigidbody == rb) 
            {
                if (enableDebugLogs) Debug.Log($"{debugTag} Skipping self collider {c.name}");
                continue;
            }

            // Use ClosestPoint to find the best anchor point on the collider
            Vector2 point = c.ClosestPoint(transform.position);
            float d = Vector2.Distance(transform.position, point);
            if (d < bestDist)
            {
                bestDist = d;
                best = c;
                bestPoint = point;
            }
        }

        if (best != null)
        {
            targetAnchor = bestPoint;

            // Connect the joint to world-space (no connected body) and set anchor
            distanceJoint.connectedBody = null;
            distanceJoint.autoConfigureConnectedAnchor = false;
            distanceJoint.connectedAnchor = targetAnchor;
            distanceJoint.distance = Vector2.Distance(transform.position, targetAnchor);
            maxRopeLength = distanceJoint.distance; // Store initial length as max
            distanceJoint.enabled = true;
            isGrappling = true;

            if (ropeRenderer != null) ropeRenderer.enabled = true;
            if (playerAnimation != null) playerAnimation.SetSwinging(true);
            if (playerMovement != null) playerMovement.ResetDoubleJump();
            if (enableDebugLogs) Debug.Log($"{debugTag} Grapple attached to {best.name} at {targetAnchor}, distance={distanceJoint.distance:F2}");
        }
        else
        {
            if (enableDebugLogs) Debug.Log($"{debugTag} No grapple target found within range {detectRange}");
        }
    }

    void JumpOff()
    {
        // Impulse away from the anchor point
        Vector2 fromAnchor = (Vector2)transform.position - targetAnchor;
        if (fromAnchor.sqrMagnitude < 0.0001f) fromAnchor = Vector2.up;

        rb.AddForce(fromAnchor.normalized * jumpOffForce, ForceMode2D.Impulse);

        if (enableDebugLogs) Debug.Log($"{debugTag} JumpOff invoked. Applied impulse {fromAnchor.normalized * jumpOffForce}");

        // Disable joint and visuals
        distanceJoint.enabled = false;
        isGrappling = false;
        if (ropeRenderer != null) ropeRenderer.enabled = false;
        if (playerAnimation != null) playerAnimation.SetSwinging(false);
        if (enableDebugLogs) Debug.Log($"{debugTag} Grapple detached and visuals disabled");
    }

    void HandleSwingMovement()
    {
        // Read A/D keys for left/right swing control using the new input system keyboard
        if (Keyboard.current == null) return;

        float left = Keyboard.current.aKey.isPressed ? 1f : 0f;
        float right = Keyboard.current.dKey.isPressed ? 1f : 0f;
        float dir = right - left;

        if (Mathf.Abs(dir) < 0.001f) return;

        Vector2 toPlayer = (Vector2)transform.position - targetAnchor;
        if (toPlayer.sqrMagnitude < 0.0001f) return;

        // Tangent vector (perpendicular) to the radial vector from anchor to player
        Vector2 tangent = Vector2.Perpendicular(toPlayer).normalized;

        // Apply force along tangent to swing
        rb.AddForce(tangent * dir * swingForce);
        if (enableDebugLogs) Debug.Log($"{debugTag} Swing input dir={dir}, appliedForce={(tangent * dir * swingForce)}");
    }

    void HandleRopeClimbing()
    {
        // Read W/S keys for climbing up/down the rope
        if (Keyboard.current == null) return;

        float climbUp = Keyboard.current.wKey.isPressed ? 1f : 0f;
        float climbDown = Keyboard.current.sKey.isPressed ? 1f : 0f;
        float climbDir = climbUp - climbDown; // Positive = climb up (shorten rope), Negative = climb down (lengthen rope)

        if (Mathf.Abs(climbDir) < 0.001f) return;

        // Adjust the rope length
        float newDistance = distanceJoint.distance - (climbDir * climbSpeed * Time.deltaTime);
        
        // Clamp between min and max rope length
        newDistance = Mathf.Clamp(newDistance, minRopeLength, maxRopeLength);
        
        distanceJoint.distance = newDistance;

        if (enableDebugLogs && Time.frameCount % 30 == 0) 
            Debug.Log($"{debugTag} Rope climbing: dir={climbDir}, newDistance={newDistance:F2}, min={minRopeLength}, max={maxRopeLength:F2}");
    }

    void UpdateRopeVisuals()
    {
        if (ropeRenderer == null) return;

        ropeRenderer.positionCount = 2;
        // First point: anchor in world space
        ropeRenderer.SetPosition(0, targetAnchor);
        // Second point: player's current position
        ropeRenderer.SetPosition(1, transform.position);
        // (Optional) occasional log to verify positions - avoid spamming every frame
        // if (enableDebugLogs && Time.frameCount % 60 == 0) Debug.Log($"{debugTag} Rope positions - anchor: {targetAnchor}, player: {transform.position}");
    }

    void OnDisable()
    {
        if (distanceJoint != null) distanceJoint.enabled = false;
        if (ropeRenderer != null) ropeRenderer.enabled = false;
        if (playerAnimation != null) playerAnimation.SetSwinging(false);
        isGrappling = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }

}