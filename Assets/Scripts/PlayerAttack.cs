using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    public GameObject attackHitbox;
    public float attackDuration = 0.15f;
    public float attackCooldown = 0.3f;
    public Animator animator;

    private bool canAttack = true;

    void OnAttack(InputValue value)
    {
        if (!canAttack) return;
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        canAttack = false;

        if (animator != null)
            animator.SetTrigger("Attack");
        Debug.Log("Hitbox ¿ªÆô");
        attackHitbox.SetActive(true);
        yield return new WaitForSeconds(attackDuration);

        attackHitbox.SetActive(false);
        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }
}
