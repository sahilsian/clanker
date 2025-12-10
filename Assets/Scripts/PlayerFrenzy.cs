using UnityEngine;
using System.Collections;

public class PlayerFrenzy : MonoBehaviour
{
    [Header("Settings")]
    public float frenzyDuration = 5f;
    public Color frenzyColor = Color.red;

    [Header("Frenzy Objects")]
    [Tooltip("Assign the pre-positioned child object for the Big Punch (Must have FrenzyHitbox script)")]
    public GameObject punchObject; 
    [Tooltip("Assign the pre-positioned child object for the Big Kick (Must have FrenzyHitbox script)")]
    public GameObject kickObject;
    [Tooltip("Distance to move the object forward when summoned")]
    public float summonDistance = 1.0f;
    
    [Header("Animation Settings")]
    public Sprite[] punchEffectSprites;
    public Sprite[] kickEffectSprites;
    public float effectFrameRate = 12f; // Frames per second for effect animation
    public float defaultEffectDuration = 0.3f; // Duration for single-sprite effects

    // Internal State
    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;
    private Coroutine frenzyCoroutine;
    public bool IsFrenzyActive { get; private set; } = false;
    private bool isAttacking = false; // Prevent simultaneous attacks

    // Store initial local positions to apply offset correctly
    private Vector3 punchInitialLocalPos;
    private Vector3 kickInitialLocalPos;

    private void Start()
    {
        // Get all sprite renderers in children (in case of multiple parts)
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        
        if (spriteRenderers.Length > 0)
        {
            originalColors = new Color[spriteRenderers.Length];
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                originalColors[i] = spriteRenderers[i].color;
            }
        }

        // Store initial positions, ensure effects are hidden, and setup physics
        Collider2D playerCollider = GetComponent<Collider2D>();

        if (punchObject != null) 
        {
            punchInitialLocalPos = punchObject.transform.localPosition;
            
            Collider2D punchCollider = punchObject.GetComponent<Collider2D>();
            if (punchCollider != null)
            {
                punchCollider.isTrigger = true; // Force Trigger to prevent pushing
                if (playerCollider != null) Physics2D.IgnoreCollision(playerCollider, punchCollider);
            }

            punchObject.SetActive(false);
        }
        if (kickObject != null) 
        {
            kickInitialLocalPos = kickObject.transform.localPosition;
            
            Collider2D kickCollider = kickObject.GetComponent<Collider2D>();
            if (kickCollider != null)
            {
                kickCollider.isTrigger = true; // Force Trigger to prevent pushing
                if (playerCollider != null) Physics2D.IgnoreCollision(playerCollider, kickCollider);
            }

            kickObject.SetActive(false);
        }
    }

    public void ActivateFrenzy()
    {
        if (IsFrenzyActive)
        {
            // If already active, just restart the timer
            if (frenzyCoroutine != null) StopCoroutine(frenzyCoroutine);
            frenzyCoroutine = StartCoroutine(FrenzyTimer());
            Debug.Log("Frenzy Mode Extended!");
            return;
        }

        Debug.Log("Entered frenzy mode");
        IsFrenzyActive = true;
        
        // Apply visual effect (Tint)
        if (spriteRenderers != null)
        {
            foreach (var sr in spriteRenderers)
            {
                // Don't tint the frenzy objects themselves if they happen to be in the list
                if ((punchObject != null && sr.gameObject == punchObject) || 
                    (kickObject != null && sr.gameObject == kickObject)) 
                    continue;

                sr.color = frenzyColor;
            }
        }

        frenzyCoroutine = StartCoroutine(FrenzyTimer());
    }

    private IEnumerator FrenzyTimer()
    {
        yield return new WaitForSeconds(frenzyDuration);
        EndFrenzy();
    }

    private void EndFrenzy()
    {
        IsFrenzyActive = false;
        Debug.Log("Frenzy mode ended");

        // Restore original colors
        if (spriteRenderers != null && originalColors != null)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (i < originalColors.Length)
                {
                    spriteRenderers[i].color = originalColors[i];
                }
            }
        }
    }

    // Call this from PlayerCombat when an attack is performed
    public void PlayAttackEffect(string attackType, Vector3 position)
    {
        if (!IsFrenzyActive) return;
        if (isAttacking) return; // Prevent simultaneous attacks

        Debug.Log($"[PlayerFrenzy] Attempting to activate object for: {attackType}");

        GameObject targetObject = null;
        Sprite[] spritesToPlay = null;

        if (attackType == "Punch")
        {
            targetObject = punchObject;
            spritesToPlay = punchEffectSprites;
        }
        else if (attackType == "Kick")
        {
            targetObject = kickObject;
            spritesToPlay = kickEffectSprites;
        }

        if (targetObject != null)
        {
            Debug.Log($"[PlayerFrenzy] Activating {targetObject.name}");
            StartCoroutine(AnimateAndActivate(targetObject, spritesToPlay));
        }
        else
        {
            Debug.LogWarning($"[PlayerFrenzy] No object assigned for {attackType}!");
        }
    }

    private IEnumerator AnimateAndActivate(GameObject obj, Sprite[] sprites)
    {
        isAttacking = true; // Lock attacks

        // Determine total duration
        float totalDuration = defaultEffectDuration;
        if (sprites != null && sprites.Length > 1)
        {
            totalDuration = sprites.Length * (1f / effectFrameRate);
        }

        // Setup positions
        Vector3 startPos = (obj == punchObject) ? punchInitialLocalPos : kickInitialLocalPos;
        // Move forward along local X (parent scale handles flipping)
        Vector3 targetPos = startPos + new Vector3(summonDistance, 0, 0);

        obj.SetActive(true);
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        
        if (sr != null)
        {
            sr.sortingOrder = 100;
            sr.enabled = true;
        }

        float elapsed = 0f;
        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / totalDuration);

            // 1. Smooth Movement
            obj.transform.localPosition = Vector3.Lerp(startPos, targetPos, t);

            // 2. Sprite Animation
            if (sr != null && sprites != null && sprites.Length > 0)
            {
                // Calculate which frame index we should be on
                int frameIndex = Mathf.FloorToInt(elapsed * effectFrameRate);
                // Clamp to last frame to avoid index out of bounds
                frameIndex = Mathf.Clamp(frameIndex, 0, sprites.Length - 1);
                sr.sprite = sprites[frameIndex];
            }

            yield return null;
        }

        // Ensure we reach the final state
        obj.transform.localPosition = targetPos;
        
        if (sr != null) sr.enabled = false;
        obj.SetActive(false);
        
        // Reset position for next time (optional, but good practice)
        obj.transform.localPosition = startPos;

        isAttacking = false; // Unlock attacks
    }
}
