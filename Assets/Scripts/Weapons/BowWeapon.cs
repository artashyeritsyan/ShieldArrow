using System.Collections;
using UnityEngine;

public class BowWeapon : WeaponController
{
    private const float SECOND = 1f;

    [Header("Bow")]
    [SerializeField] private int arrowsPerShot = 1;
    [SerializeField] private float spreadAngle = 20f;

    public override bool Shoot()
    {
        if (!CanShoot())
            return false;

        StartCoroutine(ShootSpread());

        return true;
    }

    private IEnumerator ShootSpread()
    {
        canShoot = false;

        /*
         * Start the player's or enemy's attack animation.
         * This happens only once for the entire spread.
         */
        NotifyAttack();

        /*
         * Wait until the bow animation reaches
         * the arrow-release moment.
         */
        if (attackStartDelay > 0f)
        {
            yield return new WaitForSeconds(
                attackStartDelay);
        }

        int arrowCount =
            Mathf.Max(arrowsPerShot, 1);

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

            for (int i = 0;
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

        float totalAttackInterval =
            SECOND /
            Mathf.Max(
                shootingSpeed,
                0.01f);

        /*
         * attackStartDelay is treated as part of
         * the complete attack interval.
         */
        float remainingCooldown =
            Mathf.Max(
                totalAttackInterval -
                attackStartDelay,
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