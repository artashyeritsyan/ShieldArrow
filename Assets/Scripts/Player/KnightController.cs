using System.Collections;
using UnityEngine;

public class KnightController : PlayerController
{
    [Header("Attack Movement")]
    [SerializeField] private float attackStartDelay = 0.1f;
    [SerializeField] private float movementLockTime = 0.3f;

    private Coroutine attackAnimationRoutine;

    protected override void AfterStart()
    {
        layerTargetWeight = 1f;
    }

    protected override void OnEquipmentAttack()
    {
        /*
         * The ShieldController invokes OnAttack when the shield
         * attack actually begins.
         *
         * The base PlayerController already:
         *
         * 1. Plays the sound.
         * 2. Triggers the Attack animation.
         *
         * KnightController only manages Knight-specific movement
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
        layerCurrentWeight = 0.5f;
        layerTargetWeight = 0f;

        yield return new WaitForSeconds(
            attackStartDelay);

        SetCanMove(false);

        yield return new WaitForSeconds(
            movementLockTime);

        SetCanMove(true);
        layerTargetWeight = 1f;

        attackAnimationRoutine = null;
    }

    public override void AdditiveAnimation()
    {
        /*
         * Add continuous shield-layer animation logic here.
         *
         * For example, this could control whether the shield
         * holding layer should remain active.
         */
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