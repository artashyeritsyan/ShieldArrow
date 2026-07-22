using System.Collections;
using UnityEngine;

public class KnightController :
    PlayerController
{
    [Header("Attack Movement")]
    [Tooltip(
        "Movement delay relative to the beginning " +
        "of the shield animation.")]
    [SerializeField]
    private float attackStartDelay = 0.1f;

    [SerializeField]
    private float movementLockTime = 0.3f;

    private Coroutine attackAnimationRoutine;

    protected override void AfterStart()
    {
        layerTargetWeight = 1f;
    }

    protected override void OnEquipmentAttack()
    {
        if (attackAnimationRoutine != null)
        {
            StopCoroutine(
                attackAnimationRoutine);
        }

        attackAnimationRoutine =
            StartCoroutine(
                AttackAnimationRoutine());
    }

    private IEnumerator AttackAnimationRoutine()
    {
        layerCurrentWeight = 0.5f;
        layerTargetWeight = 0f;

        float scaledStartDelay =
            GetScaledEquipmentDuration(
                attackStartDelay);

        if (scaledStartDelay > 0f)
        {
            yield return new WaitForSeconds(
                scaledStartDelay);
        }

        SetCanMove(false);

        float scaledMovementLock =
            GetScaledEquipmentDuration(
                movementLockTime);

        if (scaledMovementLock > 0f)
        {
            yield return new WaitForSeconds(
                scaledMovementLock);
        }

        SetCanMove(true);
        layerTargetWeight = 1f;

        attackAnimationRoutine = null;
    }

    public override void AdditiveAnimation()
    {
    }

    public override void Dead()
    {
        if (attackAnimationRoutine != null)
        {
            StopCoroutine(
                attackAnimationRoutine);

            attackAnimationRoutine = null;
        }

        SetCanMove(false);
        SetCanAttack(false);
    }
}