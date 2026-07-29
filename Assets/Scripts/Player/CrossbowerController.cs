using System.Collections;
using UnityEngine;

public class CrossbowerController :
    PlayerController
{
    [Header("Attack Movement")]
    [SerializeField]
    private float movementLockTime = 0.1f;

    [SerializeField]
    private float animationFinishTime = 0.1f;

    private Coroutine attackAnimationRoutine;
    private bool isReloading;

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

    protected override void OnEquipmentReloadStarted()
    {
        isReloading = true;

        if (attackAnimationRoutine != null)
        {
            StopCoroutine(
                attackAnimationRoutine);

            attackAnimationRoutine = null;
        }

        //SetCanMove(false);
        layerTargetWeight = 0f;
    }

    protected override void OnEquipmentReloadFinished()
    {
        isReloading = false;

        SetCanMove(true);
        layerTargetWeight = 1f;
    }

    private IEnumerator AttackAnimationRoutine()
    {
        layerCurrentWeight = 0.1f;
        layerTargetWeight = 0f;

        //SetCanMove(false);

        float scaledMovementLock =
            GetScaledEquipmentDuration(
                movementLockTime);

        if (scaledMovementLock > 0f)
        {
            yield return new WaitForSeconds(
                scaledMovementLock);
        }

        if (!isReloading)
        {
            SetCanMove(true);
        }

        float scaledFinishTime =
            GetScaledEquipmentDuration(
                animationFinishTime);

        if (scaledFinishTime > 0f)
        {
            yield return new WaitForSeconds(
                scaledFinishTime);
        }

        if (!isReloading)
        {
            layerTargetWeight = 1f;
        }

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

        isReloading = false;

        SetCanMove(false);
        SetCanAttack(false);
    }
}