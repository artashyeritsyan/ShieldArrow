using System;
using System.Collections;
using UnityEngine;

public class CrossbowWeapon : MonoBehaviour, ICombatEquipment
{
    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject projectilePrefab;

    [SerializeField] private int shotsInRow = 3;
    [SerializeField] private float delayBetweenShots = 0.3f;
    [SerializeField] private float reloadTime = 1.2f;

    private bool isBusy;

    public event Action OnAttack;
    public event Action OnReloadStarted;
    public event Action OnReloadFinished;

    public bool Use()
    {
        if (isBusy)
            return false;

        StartCoroutine(BurstRoutine());
        return true;
    }

    private IEnumerator BurstRoutine()
    {
        isBusy = true;

        for (int i = 0; i < shotsInRow; i++)
        {
            FireProjectile();

            // One attack animation for every shot.
            OnAttack?.Invoke();

            if (i < shotsInRow - 1)
            {
                yield return new WaitForSeconds(
                    delayBetweenShots);
            }
        }

        OnReloadStarted?.Invoke();

        yield return new WaitForSeconds(reloadTime);

        OnReloadFinished?.Invoke();

        isBusy = false;
    }

    private void FireProjectile()
    {
        Instantiate(
            projectilePrefab,
            shootPoint.position,
            shootPoint.rotation);
    }
}