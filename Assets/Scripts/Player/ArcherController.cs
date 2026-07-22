using System.Collections;
using UnityEngine;

public class ArcherController :
    PlayerController
{
    [Header("Attack Movement")]
    [SerializeField]
    private float movementLockTime = 0.3f;

    [SerializeField]
    private float animationFinishTime = 0.1f;

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
        layerCurrentWeight = 0.1f;
        layerTargetWeight = 0f;

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

        float scaledFinishTime =
            GetScaledEquipmentDuration(
                animationFinishTime);

        if (scaledFinishTime > 0f)
        {
            yield return new WaitForSeconds(
                scaledFinishTime);
        }

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