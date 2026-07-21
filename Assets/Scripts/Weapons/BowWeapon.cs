using System.Collections;
using UnityEngine;

public class BowWeapon : WeaponController
{
    private const float SECOND = 1f;

    [Header("Bow")]
    [SerializeField] private int arrowsPerShot = 3;
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

        int arrowCount = Mathf.Max(arrowsPerShot, 1);

        if (arrowCount == 1)
        {
            CreateProjectile(shootPoint.rotation);
        }
        else
        {
            float angleStep =
                spreadAngle / (arrowCount - 1);

            float startingAngle =
                -spreadAngle * 0.5f;

            for (int i = 0; i < arrowCount; i++)
            {
                float currentAngle =
                    startingAngle + angleStep * i;

                Quaternion rotation =
                    shootPoint.rotation *
                    Quaternion.Euler(
                        0f,
                        currentAngle,
                        0f);

                CreateProjectile(rotation);
            }
        }

        // Only one animation because all arrows fire together.
        NotifyShotFired();

        float cooldown =
            SECOND / Mathf.Max(shootingSpeed, 0.01f);

        yield return new WaitForSeconds(cooldown);

        canShoot = true;
    }

    public int GetArrowsPerShot()
    {
        return arrowsPerShot;
    }

    public void SetArrowsPerShot(int newArrowCount)
    {
        arrowsPerShot = Mathf.Max(newArrowCount, 1);
    }

    public float GetSpreadAngle()
    {
        return spreadAngle;
    }

    public void SetSpreadAngle(float newSpreadAngle)
    {
        spreadAngle = Mathf.Max(newSpreadAngle, 0f);
    }
}