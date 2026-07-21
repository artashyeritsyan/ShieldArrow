using System.Collections;
using UnityEngine;

public class ArcherController : PlayerController
{
    [Header("Attack Movement")]
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
         * The base PlayerController already:
         *
         * 1. Receives OnAttack from the bow.
         * 2. Plays the attack sound.
         * 3. Triggers animator.SetTrigger("Attack").
         *
         * This class only handles Archer-specific movement
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
        /*
         * Add Archer-specific continuous animation
         * updates here when needed.
         *
         * Do not trigger the main attack animation here.
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