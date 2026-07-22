using System.Collections;
using UnityEngine;

public class BowWeapon : WeaponController
{
    private const float SECOND = 1f;

    [Header("Bow")]
    [SerializeField]
    private int arrowsPerShot = 1;

    [SerializeField]
    private float spreadAngle = 20f;

    public override bool Shoot()
    {
        if (!CanShoot())
            return false;

        StartCoroutine(
            ShootSpread());

        return true;
    }

    private IEnumerator ShootSpread()
    {
        canShoot = false;

        /*
         * One animation event for the complete spread.
         */
        NotifyAttack();

        /*
         * The release delay becomes shorter when
         * the action-speed multiplier increases.
         */
        float scaledAttackStartDelay =
            GetScaledDuration(
                attackStartDelay);

        if (scaledAttackStartDelay > 0f)
        {
            yield return new WaitForSeconds(
                scaledAttackStartDelay);
        }

        int arrowCount =
            Mathf.Max(
                arrowsPerShot,
                1);

        if (arrowCount == 1)
        {
            CreateProjectile(
                shootPoint.rotation);
        }
        else
        {
            float angleStep =
                spreadAngle /
                (arrowCount - 1);

            float startingAngle =
                -spreadAngle * 0.5f;

            for (
                int i = 0;
                i < arrowCount;
                i++)
            {
                float currentAngle =
                    startingAngle +
                    angleStep * i;

                Quaternion rotation =
                    shootPoint.rotation *
                    Quaternion.Euler(
                        0f,
                        currentAngle,
                        0f);

                CreateProjectile(rotation);
            }
        }

        /*
         * EffectiveActionSpeed includes both:
         *
         * shootingSpeed
         * actionSpeedMultiplier
         */
        float totalAttackInterval =
            SECOND /
            EffectiveActionSpeed;

        /*
         * The attack-start delay is already part
         * of the complete attack interval.
         */
        float remainingCooldown =
            Mathf.Max(
                totalAttackInterval -
                scaledAttackStartDelay,
                0f);

        if (remainingCooldown > 0f)
        {
            yield return new WaitForSeconds(
                remainingCooldown);
        }

        canShoot = true;
    }

    public int GetArrowsPerShot()
    {
        return arrowsPerShot;
    }

    public void SetArrowsPerShot(
        int newArrowCount)
    {
        arrowsPerShot =
            Mathf.Max(
                newArrowCount,
                1);
    }

    public float GetSpreadAngle()
    {
        return spreadAngle;
    }

    public void SetSpreadAngle(
        float newSpreadAngle)
    {
        spreadAngle =
            Mathf.Max(
                newSpreadAngle,
                0f);
    }
}