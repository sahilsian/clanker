using UnityEngine;

public class BossAnimation : MonoBehaviour
{
    [Header("References")]
    public Animator animator;

    [Header("Animation Names")]
    [Tooltip("Must match the State Name in the Animator Controller exactly")]
    public string idleAnim = "finalboss_Idle";
    public string claw1AttackAnim = "Claw1 attack";
    public string claw2AttackAnim = "Claw2 attack";
    public string claw3AttackAnim = "Claw3 attack";
    public string restAnim = "rest";
    
    [Header("Settings")]
    public float attackDuration = 1.0f; // Time to wait before returning to Idle

    private string currentAnimState = "";
    private Coroutine currentAttackCoroutine;

    private void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (animator == null) Debug.LogError("BossAnimation: No Animator found!");
    }

    public void PlayClaw1Attack()
    {
        // Stop any running attack to prevent conflict
        if (currentAttackCoroutine != null) StopCoroutine(currentAttackCoroutine);
        currentAttackCoroutine = StartCoroutine(AttackSequence(claw1AttackAnim));
    }

    public void PlayClaw2Attack()
    {
        if (currentAttackCoroutine != null) StopCoroutine(currentAttackCoroutine);
        currentAttackCoroutine = StartCoroutine(AttackSequence(claw2AttackAnim));
    }

    public void PlayClaw3Attack()
    {
        if (currentAttackCoroutine != null) StopCoroutine(currentAttackCoroutine);
        currentAttackCoroutine = StartCoroutine(AttackSequence(claw3AttackAnim));
    }

    public void PlayRest()
    {
         if (currentAttackCoroutine != null) StopCoroutine(currentAttackCoroutine);
         PlayAnimation(restAnim);
         // Rest state logic is handled by Manager (waiting for duration), so we just stay in this anim
    }

    private System.Collections.IEnumerator AttackSequence(string attackAnim)
    {
        // 1. Play Attack
        PlayAnimation(attackAnim);

        // 2. Wait for duration
        yield return new WaitForSeconds(attackDuration);

        // 3. Return to Idle
        PlayAnimation(idleAnim);
        currentAttackCoroutine = null;
    }

    public void PlayAnimation(string newState)
    {
        if (currentAnimState == newState) return;

        if (animator != null)
        {
            animator.Play(newState, -1, 0f); // Play from start
            currentAnimState = newState;
        }
    }
}
