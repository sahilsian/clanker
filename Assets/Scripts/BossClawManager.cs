using UnityEngine;

public class BossClawManager : MonoBehaviour
{
    [Header("Detection Layer")]
    public LayerMask playerLayer;

    [Header("Claw 1 Settings")]
    public GameObject claw1;
    [Range(0f, 10f)] public float claw1RangeX = 2f;
    [Range(0f, 10f)] public float claw1RangeY = 2f;
    [Range(0f, 10f)] public float claw1RangeZ = 2f;
    public Vector3 claw1Offset;

    [Header("Claw 2 Settings")]
    public GameObject claw2;
    [Range(0f, 10f)] public float claw2RangeX = 2f;
    [Range(0f, 10f)] public float claw2RangeY = 2f;
    [Range(0f, 10f)] public float claw2RangeZ = 2f;
    public Vector3 claw2Offset;

    [Header("Claw 3 Settings")]
    public GameObject claw3;
    [Range(0f, 10f)] public float claw3RangeX = 2f;
    [Range(0f, 10f)] public float claw3RangeY = 2f;
    [Range(0f, 10f)] public float claw3RangeZ = 2f;
    public Vector3 claw3Offset;

    [Header("Animation")]
    public BossAnimation bossAnimation;
    public float attackCooldown = 2.0f;

    [Header("Rest & Vulnerability")]
    public Collider2D vulnerableCheckCollider;
    public float restDuration = 5.0f;
    public BossMovement bossMovement;
    private bool isResting = false;
    private float restTimer;

    // Attack Tracking
    private bool c1Attacked = false;
    private bool c2Attacked = false;
    private bool c3Attacked = false;

    [Header("Debug Settings")]
    public bool showDebugLogs = true;

    // Internal state
    private bool claw1Detected;
    private bool claw2Detected;
    private bool claw3Detected;

    private float claw1Timer;
    private float claw2Timer;
    private float claw3Timer;

    private void Start()
    {
        if (bossAnimation == null) bossAnimation = GetComponent<BossAnimation>();
        if (bossMovement == null) bossMovement = GetComponent<BossMovement>();
        
        // Auto-find Collider if not assigned, assuming it's on the same object and is a Trigger
        if (vulnerableCheckCollider == null)
        {
            Collider2D[] colliders = GetComponents<Collider2D>();
            foreach (var col in colliders)
            {
                if (col.isTrigger && col != GetComponent<BoxCollider2D>()) // crude check, but better than nothing. 
                // Actually safer to just let user assign it or look for specific CircleCollider2D
                if (col is CircleCollider2D && col.isTrigger) 
                {
                    vulnerableCheckCollider = col;
                    break;
                }
            }
        }

        // DEBUG: Report status
        if (showDebugLogs)
        {
            Debug.Log($"[BossClawManager] Start. Movement: {(bossMovement!=null?"OK":"MISSING")}, Animation: {(bossAnimation!=null?"OK":"MISSING")}");
            Debug.Log($"[BossClawManager] Vulnerable Collider: {(vulnerableCheckCollider!=null ? vulnerableCheckCollider.name : "MISSING! Add a CircleCollider2D (Trigger) and assign it!")}");
        }
    }

    private void Update()
    {
        // 1. Handle Rest Phase
        if (isResting)
        {
            restTimer -= Time.deltaTime;
            if (restTimer <= 0)
            {
                ExitRestPhase();
            }
            return; // Skip detection logic while resting
        }

        // 2. Update Attack Timers
        if (claw1Timer > 0) claw1Timer -= Time.deltaTime;
        if (claw2Timer > 0) claw2Timer -= Time.deltaTime;
        if (claw3Timer > 0) claw3Timer -= Time.deltaTime;

        // 3. Check Detection
        claw1Detected = DetectPlayer(claw1, "Claw 1", new Vector3(claw1RangeX, claw1RangeY, claw1RangeZ), claw1Offset);
        claw2Detected = DetectPlayer(claw2, "Claw 2", new Vector3(claw2RangeX, claw2RangeY, claw2RangeZ), claw2Offset);
        claw3Detected = DetectPlayer(claw3, "Claw 3", new Vector3(claw3RangeX, claw3RangeY, claw3RangeZ), claw3Offset);

        // 4. Trigger Animations if Detected and Ready
        if (claw1Detected && claw1Timer <= 0 && !c1Attacked)
        {
            if (bossAnimation != null)
            {
                bossAnimation.PlayClaw1Attack();
                if(showDebugLogs) Debug.Log("Triggering Claw 1 Animation!");
                claw1Timer = attackCooldown;
                c1Attacked = true;
                CheckForRestCondition();
            }
        }

        if (claw2Detected && claw2Timer <= 0 && !c2Attacked)
        {
            if (bossAnimation != null)
            {
                bossAnimation.PlayClaw2Attack();
                if(showDebugLogs) Debug.Log("Triggering Claw 2 Animation!");
                claw2Timer = attackCooldown;
                c2Attacked = true;
                CheckForRestCondition();
            }
        }

        if (claw3Detected && claw3Timer <= 0 && !c3Attacked)
        {
            if (bossAnimation != null)
            {
                bossAnimation.PlayClaw3Attack();
                if(showDebugLogs) Debug.Log("Triggering Claw 3 Animation!");
                claw3Timer = attackCooldown;
                c3Attacked = true;
                CheckForRestCondition();
            }
        }
    }

    private void CheckForRestCondition()
    {
        // If all 3 claws have attacked at least once
        if (c1Attacked && c2Attacked && c3Attacked)
        {
            EnterRestPhase();
        }
    }

    private void EnterRestPhase()
    {
        isResting = true;
        restTimer = restDuration;
        
        if (showDebugLogs) Debug.Log("Boss Entering Rest Phase (Vulnerable)!");

        // Play Rest Animation
        if (bossAnimation != null) bossAnimation.PlayRest();

        if (vulnerableCheckCollider != null)
        {
             vulnerableCheckCollider.enabled = true;
             if (showDebugLogs) Debug.Log("[BossClawManager] Vulnerable Collider ENABLED. Boss should take damage now.");
        }
        else
        {
             if (showDebugLogs) Debug.LogError("[BossClawManager] Cannot enable Vulnerability: Collider is NULL!");
        }

        // Stop Movement
        if (bossMovement != null) bossMovement.enabled = false;

        // Clear Detection State (so gizmos don't stay green)
        claw1Detected = false;
        claw2Detected = false;
        claw3Detected = false;

        // Disable Claw Colliders (so player touches don't hurt them during rest)
        SetClawColliders(false);
    }

    private void ExitRestPhase()
    {
        isResting = false;
        
        // Reset Attack Flags
        c1Attacked = false;
        c2Attacked = false;
        c3Attacked = false;

        // Reset Timers
        claw1Timer = attackCooldown; 
        claw2Timer = attackCooldown;
        claw3Timer = attackCooldown;

        if (showDebugLogs) Debug.Log("Boss Exiting Rest Phase (Attacking again)!");

        // Return to Idle
        if (bossAnimation != null) bossAnimation.PlayAnimation(bossAnimation.idleAnim);

        // Disable Vulnerability
        if (vulnerableCheckCollider != null) vulnerableCheckCollider.enabled = false;

        // Resume Movement
        if (bossMovement != null) bossMovement.enabled = true;

        // Re-enable Claw Colliders
        SetClawColliders(true);
    }

    private void SetClawColliders(bool state)
    {
        if (claw1 != null) { Collider2D c = claw1.GetComponent<Collider2D>(); if (c != null) c.enabled = state; }
        if (claw2 != null) { Collider2D c = claw2.GetComponent<Collider2D>(); if (c != null) c.enabled = state; }
        if (claw3 != null) { Collider2D c = claw3.GetComponent<Collider2D>(); if (c != null) c.enabled = state; }
    }

    private bool DetectPlayer(GameObject claw, string clawName, Vector3 boxSize, Vector3 offset)
    {
        if (claw == null) return false;

        // Convert to 2D
        Vector2 center = claw.transform.position + offset;
        Vector2 size = new Vector2(boxSize.x, boxSize.y);
        float angle = claw.transform.eulerAngles.z;
        
        bool foundPlayer = false;

        // Use OverlapBoxAll for 2D Physics
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, angle, playerLayer);

        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"{clawName} hit object: {hit.name} (Tag: {hit.tag}, Layer: {LayerMask.LayerToName(hit.gameObject.layer)})");
                }

                if(hit.CompareTag("Player"))
                {
                    Debug.Log($"{clawName} Detected Player! Attacking!");
                    foundPlayer = true;
                }
            }
        }
        
        return foundPlayer;
    }

    private void OnDrawGizmos()
    {
        // Draw Claw 1
        Gizmos.color = claw1Detected ? Color.green : Color.red;
        DrawClawGizmo(claw1, new Vector3(claw1RangeX, claw1RangeY, claw1RangeZ), claw1Offset);

        // Draw Claw 2
        Gizmos.color = claw2Detected ? Color.green : Color.red;
        DrawClawGizmo(claw2, new Vector3(claw2RangeX, claw2RangeY, claw2RangeZ), claw2Offset);

        // Draw Claw 3
        Gizmos.color = claw3Detected ? Color.green : Color.red;
        DrawClawGizmo(claw3, new Vector3(claw3RangeX, claw3RangeY, claw3RangeZ), claw3Offset);
    }

    private void DrawClawGizmo(GameObject claw, Vector3 size, Vector3 offset)
    {
        if (claw != null)
        {
            // Use only Z rotation for 2D consistency in gizmos (optional but helps visualization)
            Quaternion rotation = Quaternion.Euler(0, 0, claw.transform.eulerAngles.z);
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(claw.transform.position + offset, rotation, size);
            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }
}
