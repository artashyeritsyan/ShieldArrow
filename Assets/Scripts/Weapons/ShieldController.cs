using System;
using System.Collections;
using UnityEngine;

public class ShieldController :
    MonoBehaviour,
    ICombatEquipment
{
    private const float SECOND = 1f;

    [Header("Shield")]
    [SerializeField] private Collider shieldCollider;

    [Header("Attack")]
    [SerializeField] private float attackSpeed = 1f;
    [SerializeField] private float attackStartDelay = 0f;
    [SerializeField] private float activeTime = 0.25f;

    private GameObject owner;
    private Team ownerTeam;
    private CoreStats ownerStats;

    private bool canAttack = true;
    private bool isAttacking;
    private bool isInitialized;

    /*
     * Generic combat-equipment events.
     *
     * PlayerController can subscribe to these without knowing
     * whether the equipment is a bow, crossbow, dagger, or shield.
     */
    public event Action OnAttack;
    public event Action OnReloadStarted;
    public event Action OnReloadFinished;

    /*
     * Shield-specific events.
     */
    public event Action OnShieldAttackStarted;
    public event Action OnShieldAttackFinished;
    public event Action OnProjectileReflected;

    private void Awake()
    {
        /*
         * Your original condition was reversed.
         *
         * Previously, GetComponent was called only when
         * shieldCollider was already assigned.
         */
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

    /*
     * Generic interface entry point.
     *
     * PlayerController calls combatEquipment.Use()
     * for every equipment type.
     */
    public bool Use()
    {
        return Attack();
    }

    public bool Attack()
    {
        if (!CanAttack())
            return false;

        StartCoroutine(ShieldAttackRoutine());

        return true;
    }

    private bool CanAttack()
    {
        if (!canAttack || isAttacking)
            return false;

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
         * Generic event:
         * tells PlayerController to start the attack animation.
         */
        OnAttack?.Invoke();

        /*
         * Wait until the shield animation reaches
         * the active hit/reflect moment.
         */
        if (attackStartDelay > 0f)
        {
            yield return new WaitForSeconds(
                attackStartDelay);
        }

        isAttacking = true;
        shieldCollider.enabled = true;

        OnShieldAttackStarted?.Invoke();

        if (activeTime > 0f)
        {
            yield return new WaitForSeconds(
                activeTime);
        }

        shieldCollider.enabled = false;
        isAttacking = false;

        OnShieldAttackFinished?.Invoke();

        float totalAttackInterval =
            SECOND /
            Mathf.Max(
                attackSpeed,
                0.01f);

        /*
         * Delay and active time are treated as part
         * of the full attack interval.
         */
        float remainingCooldown =
            Mathf.Max(
                totalAttackInterval -
                attackStartDelay -
                activeTime,
                0f);

        if (remainingCooldown > 0f)
        {
            yield return new WaitForSeconds(
                remainingCooldown);
        }

        canAttack = true;
    }

    public bool CanReflectProjectile(
        Team projectileTeam)
    {
        return
            isAttacking &&
            projectileTeam != ownerTeam;
    }

    // TODO:
    // Change this part OR NOT, but add there the Audio manager to play reflection sound
    // Or Change the Audio manager, to subscribe to events,
    // and to keep sounds in on file not spread into many files
    // OR some Effects for it
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

    public void SetAttackStartDelay(
        float newDelay)
    {
        attackStartDelay =
            Mathf.Max(
                newDelay,
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