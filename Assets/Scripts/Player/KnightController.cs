using System.Collections;
using UnityEngine;

public class KnightController : PlayerController
{
    [Header("Shield")]
    [SerializeField] private ShieldController shieldController;

    [Header("Attack Movement")]
    [SerializeField] private float attackStartDelay = 0.1f;
    [SerializeField] private float movementLockTime = 0.3f;

    private Coroutine attackAnimationRoutine;

    protected override void AfterStart()
    {
        layerTargetWeight = 1f;

        if (shieldController == null)
        {
            shieldController = GetComponentInChildren<ShieldController>();
        }

        if (shieldController == null)
        {
            Debug.LogError($"{name}: ShieldController is missing.");

            return;
        }

        //DamageData damageData =
        //    new DamageData
        //    {
        //        baseDamage = stats.core.damage,
        //        canCrit = true,
        //        critChance = 0.2f,
        //        critMultiplier = 2f
        //    };

        shieldController.Initialize(gameObject, Team, stats.core);
    }

    protected override void OnEquipmentAttack()
    {
        if (attackAnimationRoutine != null)
        {
            StopCoroutine(attackAnimationRoutine);
        }

        attackAnimationRoutine = StartCoroutine(AttackAnimationRoutine());
    }

    private IEnumerator AttackAnimationRoutine()
    {
        layerCurrentWeight = 0.5f;
        layerTargetWeight = 0f;

        float scaledStartDelay = GetScaledEquipmentDuration(attackStartDelay);

        if (scaledStartDelay > 0f)
        {
            yield return new WaitForSeconds(scaledStartDelay);
        }

        SetCanMove(false);

        float scaledMovementLock = GetScaledEquipmentDuration(movementLockTime);

        if (scaledMovementLock > 0f)
        {
            yield return new WaitForSeconds(scaledMovementLock);
        }

        SetCanMove(true);
        layerTargetWeight = 1f;

        attackAnimationRoutine = null;
    }

    public override void AdditiveAnimation()
    {
    }
}