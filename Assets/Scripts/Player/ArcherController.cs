using System.Collections;
using UnityEngine;

public class ArcherController : PlayerController
{
    [Header("Weapon")]
    private WeaponScript weaponScript;

    [Header("Attack")]
    [SerializeField] float attackTime = 0.3f;
    [SerializeField] float attackCooldown = 5;
    [SerializeField] float animLayerTransitionTime = 10f;
    [SerializeField] float animFinishTime = 0.1f;
    private float layerTargetWeight = 0;
    private float layerCurrentWeight = 0;

    private bool canAttack;


    protected override void Start()
    {
        base.Start();
        InitialiseWeapon();
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
            layerCurrentWeight = 0.1f;
            layerTargetWeight = 0;

            animator.SetTrigger("Attack");
            animator.SetFloat("AttackSpeed", weaponScript.GetAttackSpeed());
            weaponScript.Shoot();

            StartCoroutine(ResetAttackCooldown());
            StartCoroutine(WaitForAttackFinished());
        }
    }
    IEnumerator WaitForAttackFinished()    {
        SetCanMove(false);

        yield return new WaitForSeconds(attackTime);
        SetCanMove(true);

        yield return new WaitForSeconds(animFinishTime);
        layerTargetWeight = 1;
    }

    IEnumerator ResetAttackCooldown()
    {
        canAttack = false;
        // 1 stands for 1 second
        yield return new WaitForSeconds(1 / weaponScript.GetAttackSpeed());
        canAttack = true;
    }

    
}
