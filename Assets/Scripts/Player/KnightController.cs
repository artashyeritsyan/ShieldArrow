using System.Collections;
using UnityEngine;

public class KnightController : PlayerController
{
    [Header("Weapon")]
    private WeaponScript weaponScript;

    [Header("Attack")]
    [SerializeField] float attacStartDelay = 0.1f;
    [SerializeField] float attackTime = 0.3f;
    [SerializeField] float attackCooldown = 5;
    [SerializeField] float animLayerTransitionTime = 10f;
    private float layerTargetWeight = 1;
    private float layerCurrentWeight = 0;

    private bool canAttack;

    // Instead of Start()
    protected override void Start()
    {
        base.Start();

        canAttack = true;

        layerTargetWeight = 1;
    }

    protected override void Update()
    {
        base.Update();

        layerCurrentWeight = Mathf.Lerp(layerCurrentWeight, layerTargetWeight, animLayerTransitionTime * Time.deltaTime);
        animator.SetLayerWeight(1, layerCurrentWeight);
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
            layerCurrentWeight = 0.5f;
            layerTargetWeight = 0;
            animator.SetTrigger("Attack");

            StartCoroutine(ResetAttackCooldown());
            StartCoroutine(WaitForAttackFinished());
        }
    }

    IEnumerator WaitForAttackFinished()
    {
        yield return new WaitForSeconds(attacStartDelay);
        SetCanMove(false);

        yield return new WaitForSeconds(attackTime);
        layerTargetWeight = 1;
        SetCanMove(true);
    }

    IEnumerator ResetAttackCooldown()
    {
        canAttack = false;

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

}
