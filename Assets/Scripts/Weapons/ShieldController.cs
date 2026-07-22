using System;
using System.Collections;
using UnityEngine;

public class ShieldController :
    MonoBehaviour,
    ICombatEquipment
{
    private const float SECOND = 1f;

    [Header("Shield")]
    [SerializeField]
    private Collider shieldCollider;

    [Header("Attack")]
    [Tooltip(
        "Base shield attacks per second before " +
        "applying the action-speed multiplier.")]
    [SerializeField]
    private float attackSpeed = 1f;

    [Tooltip(
        "Time between starting the animation and " +
        "activating the shield collider.")]
    [SerializeField]
    private float attackStartDelay = 0f;

    [Tooltip(
        "How long the shield collider remains active.")]
    [SerializeField]
    private float activeTime = 0.25f;

    [Tooltip(
        "Maximum distance from which an enemy can " +
        "start a shield attack.")]
    [SerializeField]
    private float effectiveRange = 2f;

    [Header("Runtime Speed Modifier")]
    [Tooltip(
        "1 = normal speed, 1.5 = 50% faster, " +
        "2 = twice as fast.")]
    [SerializeField]
    private float actionSpeedMultiplier = 1f;

    private GameObject owner;
    private Team ownerTeam;
    private CoreStats ownerStats;

    private bool canAttack = true;
    private bool isAttacking;
    private bool isInitialized;

    public event Action OnAttack;
    public event Action OnReloadStarted;
    public event Action OnReloadFinished;

    public event Action OnShieldAttackStarted;
    public event Action OnShieldAttackFinished;
    public event Action OnProjectileReflected;

    public float BaseActionSpeed =>
        Mathf.Max(
            attackSpeed,
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

    private void Awake()
    {
        if (shieldCollider == null)
        {
            shieldCollider =
                GetComponent<Collider>();
        }

        if (shieldCollider != null)
        {
            shieldCollider.enabled = false;
        }
    }

    public void Initialize(
        GameObject shieldOwner,
        Team team,
        CoreStats stats)
    {
        owner = shieldOwner;
        ownerTeam = team;
        ownerStats = stats;

        isInitialized = true;
        canAttack = true;
        isAttacking = false;

        if (shieldCollider != null)
        {
            shieldCollider.enabled = false;
        }
    }

    public bool Use()
    {
        return Attack();
    }

    public bool Attack()
    {
        if (!CanAttack())
            return false;

        StartCoroutine(
            ShieldAttackRoutine());

        return true;
    }

    private bool CanAttack()
    {
        if (!canAttack ||
            isAttacking)
        {
            return false;
        }

        if (!isInitialized)
        {
            Debug.LogError(
                $"{name}: Shield is not initialized.");

            return false;
        }

        if (shieldCollider == null)
        {
            Debug.LogError(
                $"{name}: Shield collider is missing.");

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

    private IEnumerator ShieldAttackRoutine()
    {
        canAttack = false;

        /*
         * Start the player's or enemy's shield
         * attack animation.
         */
        OnAttack?.Invoke();

        float scaledAttackStartDelay =
            GetScaledDuration(
                attackStartDelay);

        if (scaledAttackStartDelay > 0f)
        {
            yield return new WaitForSeconds(
                scaledAttackStartDelay);
        }

        isAttacking = true;
        shieldCollider.enabled = true;

        OnShieldAttackStarted?.Invoke();

        /*
         * The active reflection window is also scaled,
         * keeping it synchronized with a faster animation.
         */
        float scaledActiveTime =
            GetScaledDuration(
                activeTime);

        if (scaledActiveTime > 0f)
        {
            yield return new WaitForSeconds(
                scaledActiveTime);
        }

        shieldCollider.enabled = false;
        isAttacking = false;

        OnShieldAttackFinished?.Invoke();

        float totalAttackInterval =
            SECOND /
            EffectiveActionSpeed;

        /*
         * The start delay and active time are already
         * included in the complete attack interval.
         */
        float remainingCooldown =
            Mathf.Max(
                totalAttackInterval -
                scaledAttackStartDelay -
                scaledActiveTime,
                0f);

        if (remainingCooldown > 0f)
        {
            yield return new WaitForSeconds(
                remainingCooldown);
        }

        canAttack = true;
    }

    private float GetScaledDuration(
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

    public bool CanReflectProjectile(
        Team projectileTeam)
    {
        return
            isAttacking &&
            projectileTeam != ownerTeam;
    }

    public void NotifyProjectileReflected()
    {
        OnProjectileReflected?.Invoke();
    }

    public GameObject GetOwner()
    {
        return owner;
    }

    public Team GetOwnerTeam()
    {
        return ownerTeam;
    }

    public CoreStats GetOwnerStats()
    {
        return ownerStats;
    }

    public bool GetCanAttack()
    {
        return canAttack;
    }

    public bool GetIsAttacking()
    {
        return isAttacking;
    }

    public float GetAttackSpeed()
    {
        return attackSpeed;
    }

    public float GetEffectiveAttackSpeed()
    {
        return EffectiveActionSpeed;
    }

    public float GetActionSpeedMultiplier()
    {
        return ActionSpeedMultiplier;
    }

    public void SetAttackSpeed(
        float newAttackSpeed)
    {
        attackSpeed =
            Mathf.Max(
                newAttackSpeed,
                0.01f);
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

    public float GetActiveTime()
    {
        return activeTime;
    }

    public void SetActiveTime(
        float newActiveTime)
    {
        activeTime =
            Mathf.Max(
                newActiveTime,
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

    private void OnEnable()
    {
        if (isInitialized)
        {
            canAttack = true;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        canAttack = false;
        isAttacking = false;

        if (shieldCollider != null)
        {
            shieldCollider.enabled = false;
        }
    }
}