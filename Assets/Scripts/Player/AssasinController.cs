using System.Collections;
using UnityEngine;

public class AssasinController : PlayerController
{
    [Header("Attack Movement")]
    [SerializeField] private float attackStartDelay = 0.1f;
    [SerializeField] private float movementLockTime = 0.3f;
    [SerializeField] private float animationFinishTime = 0.1f;

    private Coroutine attackAnimationRoutine;

    protected override void AfterStart()
    {
        layerTargetWeight = 1f;
    }

    protected override void OnEquipmentAttack()
    {
        /*
         * The assassin equipment invokes OnAttack when the
         * attack actually starts.
         *
         * The base PlayerController already:
         * - triggers Attack
         * - plays the attack sound
         *
         * This controller handles only delayed movement locking
         * and animation-layer behavior.
         */

        if (attackAnimationRoutine != null)
        {
            StopCoroutine(attackAnimationRoutine);
        }

        attackAnimationRoutine =
            StartCoroutine(AttackAnimationRoutine());
    }

    private IEnumerator AttackAnimationRoutine()
    {
        layerCurrentWeight = 0.1f;
        layerTargetWeight = 0f;

        yield return new WaitForSeconds(
            attackStartDelay);

        SetCanMove(false);

        yield return new WaitForSeconds(
            movementLockTime);

        SetCanMove(true);

        yield return new WaitForSeconds(
            animationFinishTime);

        layerTargetWeight = 1f;
        attackAnimationRoutine = null;
    }

    public override void AdditiveAnimation()
    {
        // Add continuous assassin animation logic here if needed.
    }

    public override void Dead()
    {
        if (attackAnimationRoutine != null)
        {
            StopCoroutine(attackAnimationRoutine);
            attackAnimationRoutine = null;
        }

        SetCanMove(false);
        SetCanAttack(false);
    }
}