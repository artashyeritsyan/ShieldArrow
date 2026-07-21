using System;
using UnityEngine;

public interface ICombatEquipment
{
    event Action OnAttack;
    event Action OnReloadStarted;
    event Action OnReloadFinished;

    void Initialize(
        GameObject owner,
        Team ownerTeam,
        CoreStats ownerStats);

    bool Use();
}
public abstract class WeaponController :
    MonoBehaviour,
    ICombatEquipment
{
    [Header("Weapon")]
    [SerializeField] protected Transform shootPoint;
    [SerializeField] protected GameObject projectilePrefab;

    [Header("Parameters")]
    [SerializeField] protected float shootingSpeed = 2f;

    [Tooltip(
        "Time between starting the attack animation " +
        "and performing the actual attack.")]
    [SerializeField] protected float attackStartDelay = 0f;

    protected GameObject owner;
    protected Team ownerTeam;
    protected CoreStats ownerStats;

    protected bool canShoot = true;
    protected bool isInitialized;

    /*
     * OnAttack means:
     * Start the character attack animation now.
     */
    public event Action OnAttack;

    public event Action OnReloadStarted;
    public event Action OnReloadFinished;

    public virtual void Initialize(
        GameObject weaponOwner,
        Team team,
        CoreStats attackerStats)
    {
        owner = weaponOwner;
        ownerTeam = team;
        ownerStats = attackerStats;

        isInitialized = true;
        canShoot = true;
    }

    /*
     * PlayerController and EnemyController call Use().
     *
     * The existing derived weapon Shoot() implementation
     * still controls the actual weapon behavior.
     */
    public virtual bool Use()
    {
        return Shoot();
    }

    public abstract bool Shoot();

    protected bool CanShoot()
    {
        if (!canShoot)
            return false;

        if (!isInitialized)
        {
            Debug.LogError(
                $"{name}: Weapon is not initialized.");

            return false;
        }

        if (shootPoint == null)
        {
            Debug.LogError(
                $"{name}: Shoot point is missing.");

            return false;
        }

        if (projectilePrefab == null)
        {
            Debug.LogError(
                $"{name}: Projectile prefab is missing.");

            return false;
        }

        if (ownerStats == null)
        {
            Debug.LogError(
                $"{name}: Owner stats are missing.");

            return false;
        }

        return true;
    }

    protected void CreateProjectile(
        Quaternion rotation)
    {
        GameObject projectileObject =
            Instantiate(
                projectilePrefab,
                shootPoint.position,
                rotation);

        ArrowScript arrow =
            projectileObject.GetComponent<ArrowScript>();

        if (arrow == null)
        {
            Debug.LogError(
                $"{projectilePrefab.name} does not " +
                $"contain ArrowScript.");

            Destroy(projectileObject);
            return;
        }

        DamageData damageData =
            new DamageData
            {
                baseDamage = ownerStats.damage,
                canCrit = true,
                critChance = 0.2f,
                critMultiplier = 2f
            };

        arrow.Initialize(
            owner,
            ownerTeam,
            damageData,
            ownerStats);
    }

    /*
     * Derived weapons call this when the character
     * should begin its attack animation.
     */
    protected void NotifyAttack()
    {
        OnAttack?.Invoke();
    }

    protected void NotifyReloadStarted()
    {
        OnReloadStarted?.Invoke();
    }

    protected void NotifyReloadFinished()
    {
        OnReloadFinished?.Invoke();
    }

    public float GetShootingSpeed()
    {
        return shootingSpeed;
    }

    public void SetShootingSpeed(
        float newShootingSpeed)
    {
        shootingSpeed =
            Mathf.Max(
                newShootingSpeed,
                0.01f);
    }

    public float GetAttackStartDelay()
    {
        return attackStartDelay;
    }

    public void SetAttackStartDelay(
        float newDelay)
    {
        attackStartDelay =
            Mathf.Max(newDelay, 0f);
    }

    public bool GetCanShoot()
    {
        return canShoot;
    }

    protected virtual void OnEnable()
    {
        canShoot = true;
    }

    protected virtual void OnDisable()
    {
        StopAllCoroutines();
        canShoot = false;
    }
}