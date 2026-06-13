using System.Collections;
using UnityEngine;

public class KnightController : PlayerController
{
    [Header("Weapon")]
    private WeaponScript weaponScript;

    [Header("Attack")]
    [SerializeField] float attackTime = 1;
    [SerializeField] float attackCooldown = 5;

    private bool canAttack;

    // Instead of Start()
    public override void AfterStart()
    {
        canAttack = true;
    }

    private void InitialiseWeapon()
    {
        weaponScript = transform.GetComponentInChildren<WeaponScript>();
        weapon = weaponScript.gameObject;
    }

    public override void Attack()
    {
        if (canAttack)
        {
            animator.SetLayerWeight(1, 0);
            animator.SetTrigger("Attack");
            StartCoroutine(ResetAttackCooldown());
            StartCoroutine(WaitForAttackFinished());
        }
    }

    IEnumerator WaitForAttackFinished()
    {
        SetCanMove(false);
        yield return new WaitForSeconds(attackTime);
        animator.SetLayerWeight(1, 1);
        SetCanMove(true);
    }
    IEnumerator ResetAttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

}
