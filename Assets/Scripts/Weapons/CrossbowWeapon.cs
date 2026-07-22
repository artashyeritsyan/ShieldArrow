using System.Collections;
using UnityEngine;

public class CrossbowWeapon : WeaponController
{
    [Header("Crossbow")]
    [SerializeField]
    private int shotsInRow = 3;

    [Tooltip(
        "Base time between the start of each " +
        "shot animation.")]
    [SerializeField]
    private float delayBetweenShots = 0.3f;

    [Tooltip(
        "Base reload duration before applying " +
        "the action-speed multiplier.")]
    [SerializeField]
    private float reloadTime = 1.2f;

    private bool isBurstActive;

    public override bool Shoot()
    {
        if (!CanShoot() ||
            isBurstActive)
        {
            return false;
        }

        StartCoroutine(
            BurstRoutine());

        return true;
    }

    private IEnumerator BurstRoutine()
    {
        canShoot = false;
        isBurstActive = true;

        int shotCount =
            Mathf.Max(
                shotsInRow,
                1);

        float scaledAttackStartDelay =
            GetScaledDuration(
                attackStartDelay);

        float scaledDelayBetweenShots =
            GetScaledDuration(
                delayBetweenShots);

        for (
            int i = 0;
            i < shotCount;
            i++)
        {
            /*
             * One animation event for every
             * individual crossbow shot.
             */
            NotifyAttack();

            if (scaledAttackStartDelay > 0f)
            {
                yield return new WaitForSeconds(
                    scaledAttackStartDelay);
            }

            CreateProjectile(
                shootPoint.rotation);

            if (i < shotCount - 1)
            {
                /*
                 * The attack delay is already part of
                 * the complete delay between shots.
                 */
                float remainingShotDelay =
                    Mathf.Max(
                        scaledDelayBetweenShots -
                        scaledAttackStartDelay,
                        0f);

                if (remainingShotDelay > 0f)
                {
                    yield return new WaitForSeconds(
                        remainingShotDelay);
                }
            }
        }

        NotifyReloadStarted();

        float scaledReloadTime =
            GetScaledDuration(
                reloadTime);

        if (scaledReloadTime > 0f)
        {
            yield return new WaitForSeconds(
                scaledReloadTime);
        }

        NotifyReloadFinished();

        isBurstActive = false;
        canShoot = true;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        isBurstActive = false;
    }

    public int GetShotsInRow()
    {
        return shotsInRow;
    }

    public void SetShotsInRow(
        int newShotCount)
    {
        shotsInRow =
            Mathf.Max(
                newShotCount,
                1);
    }

    public float GetDelayBetweenShots()
    {
        return delayBetweenShots;
    }

    public float GetEffectiveDelayBetweenShots()
    {
        return GetScaledDuration(
            delayBetweenShots);
    }

    public void SetDelayBetweenShots(
        float newDelay)
    {
        delayBetweenShots =
            Mathf.Max(
                newDelay,
                0f);
    }

    public float GetReloadTime()
    {
        return reloadTime;
    }

    public float GetEffectiveReloadTime()
    {
        return GetScaledDuration(
            reloadTime);
    }

    public void SetReloadTime(
        float newReloadTime)
    {
        reloadTime =
            Mathf.Max(
                newReloadTime,
                0f);
    }

    public bool GetIsBurstActive()
    {
        return isBurstActive;
    }
}