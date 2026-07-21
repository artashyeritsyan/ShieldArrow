using System.Collections;
using UnityEngine;

public class CrossbowWeapon : WeaponController
{
    [Header("Crossbow")]
    [SerializeField] private int shotsInRow = 3;
    [SerializeField] private float delayBetweenShots = 0.3f;
    [SerializeField] private float reloadTime = 1.2f;

    private bool isBurstActive;

    public override bool Shoot()
    {
        if (!CanShoot() || isBurstActive)
            return false;

        StartCoroutine(BurstRoutine());
        return true;
    }

    private IEnumerator BurstRoutine()
    {
        canShoot = false;
        isBurstActive = true;

        int shotCount = Mathf.Max(shotsInRow, 1);

        for (int i = 0; i < shotCount; i++)
        {
            // Start one attack animation for this shot.
            NotifyAttack();

            // Wait until the animation reaches the firing moment.
            if (attackStartDelay > 0f)
            {
                yield return new WaitForSeconds(
                    attackStartDelay);
            }

            // Uses WeaponController projectile initialization.
            CreateProjectile(shootPoint.rotation);

            if (i < shotCount - 1)
            {
                float remainingShotDelay =
                    Mathf.Max(
                        delayBetweenShots -
                        attackStartDelay,
                        0f);

                if (remainingShotDelay > 0f)
                {
                    yield return new WaitForSeconds(
                        remainingShotDelay);
                }
            }
        }

        NotifyReloadStarted();

        if (reloadTime > 0f)
        {
            yield return new WaitForSeconds(
                reloadTime);
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

    public void SetShotsInRow(int newShotCount)
    {
        shotsInRow = Mathf.Max(newShotCount, 1);
    }

    public float GetDelayBetweenShots()
    {
        return delayBetweenShots;
    }

    public void SetDelayBetweenShots(float newDelay)
    {
        delayBetweenShots = Mathf.Max(newDelay, 0f);
    }

    public float GetReloadTime()
    {
        return reloadTime;
    }

    public void SetReloadTime(float newReloadTime)
    {
        reloadTime = Mathf.Max(newReloadTime, 0f);
    }
}