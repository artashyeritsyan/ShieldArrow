using System;
using UnityEngine;

using System;
using UnityEngine;

public interface ICombatEquipment
{
    event Action OnAttack;
    event Action OnReloadStarted;
    event Action OnReloadFinished;

    /*
     * Base equipment speed configured in the Inspector.
     *
     * WeaponController:
     * shootingSpeed
     *
     * ShieldController:
     * attackSpeed
     */
    float BaseActionSpeed { get; }

    /*
     * Runtime multiplier used for upgrades,
     * buffs and debuffs.
     */
    float ActionSpeedMultiplier { get; }

    /*
     * Final speed used by animation and equipment logic.
     *
     * BaseActionSpeed * ActionSpeedMultiplier
     */
    float EffectiveActionSpeed { get; }

    /*
     * Distance at which an enemy may use this equipment.
     */
    float EffectiveRange { get; }

    void Initialize(
        GameObject owner,
        Team ownerTeam,
        CoreStats ownerStats);

    bool Use();

    void SetActionSpeedMultiplier(
        float multiplier);

    void AddActionSpeedMultiplier(
        float amount);
}


public abstract class WeaponController :
    MonoBehaviour,
    ICombatEquipment
{
    [Header("Weapon")]
    [SerializeField]
    protected Transform shootPoint;

    [SerializeField]
    protected GameObject projectilePrefab;

    [Header("Parameters")]
    [Tooltip(
        "Base attacks per second before applying " +
        "the action-speed multiplier.")]
    [SerializeField]
    protected float shootingSpeed = 2f;

    [Tooltip(
        "Time between starting the attack animation " +
        "and performing the actual attack.")]
    [SerializeField]
    protected float attackStartDelay = 0f;

    [Tooltip(
        "Maximum distance from which an enemy can " +
        "use this weapon.")]
    [SerializeField]
    protected float effectiveRange = 12f;

    [Header("Runtime Speed Modifier")]
    [Tooltip(
        "1 = normal speed, 1.5 = 50% faster, " +
        "2 = twice as fast.")]
    [SerializeField]
    protected float actionSpeedMultiplier = 1f;

    protected GameObject owner;
    protected Team ownerTeam;
    protected CoreStats ownerStats;

    protected bool canShoot = true;
    protected bool isInitialized;

    public event Action OnAttack;
    public event Action OnReloadStarted;
    public event Action OnReloadFinished;

    public float BaseActionSpeed =>
        Mathf.Max(
            shootingSpeed,
            0.01f);

    public float ActionSpeedMultiplier =>
        Mathf.Max(
            actionSpeedMultiplier,
            0.01f);

    public float EffectiveActionSpeed =>
        Mathf.Max(
            BaseActionSpeed *
            ActionSpeedMultiplier,
            0.01f);

    public float EffectiveRange =>
        Mathf.Max(
            effectiveRange,
            0f);

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
     * The derived weapon still controls its actual
     * attack implementation through Shoot().
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

    /*
     * Scales animation-related and weapon-related
     * duration values using the upgrade multiplier.
     *
     * Multiplier 2:
     * 0.4 seconds becomes 0.2 seconds.
     */
    protected float GetScaledDuration(
        float baseDuration)
    {
        return Mathf.Max(
            baseDuration /
            ActionSpeedMultiplier,
            0f);
    }

    public void SetActionSpeedMultiplier(
        float multiplier)
    {
        actionSpeedMultiplier =
            Mathf.Max(
                multiplier,
                0.01f);
    }

    public void AddActionSpeedMultiplier(
        float amount)
    {
        actionSpeedMultiplier =
            Mathf.Max(
                actionSpeedMultiplier +
                amount,
                0.01f);
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

    public float GetEffectiveShootingSpeed()
    {
        return EffectiveActionSpeed;
    }

    public float GetActionSpeedMultiplier()
    {
        return ActionSpeedMultiplier;
    }

    public float GetAttackStartDelay()
    {
        return attackStartDelay;
    }

    public float GetScaledAttackStartDelay()
    {
        return GetScaledDuration(
            attackStartDelay);
    }

    public void SetAttackStartDelay(
        float newDelay)
    {
        attackStartDelay =
            Mathf.Max(
                newDelay,
                0f);
    }

    public float GetEffectiveRange()
    {
        return EffectiveRange;
    }

    public void SetEffectiveRange(
        float newRange)
    {
        effectiveRange =
            Mathf.Max(
                newRange,
                0f);
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