using System;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class ShieldController : MonoBehaviour, ICombatEquipment
{
    private const float SECOND = 1f;

    [Header("Shield")]
    [SerializeField] private Collider shieldCollider;
    [SerializeField] private Transform attackPoint;
    [SerializeField]private Vector3 attackSize = new Vector3(1.5f, 1f, 1.5f);

    [SerializeField] private LayerMask attackLayer;

    [SerializeField] private ParticleSystem attackEffect;


    [Header("Attack")]
    [SerializeField] private float attackSpeed = 1f;
    [SerializeField] private float attackStartDelay = 0f;
    [SerializeField] private float activeTime = 0.25f;
    [SerializeField] private float effectiveRange = 2f;

    [Header("Runtime Speed Modifier")]
    [SerializeField] private float actionSpeedMultiplier = 1f;

    //[SerializeField] public bool isEnemyShield;

    private GameObject owner;
    private Team ownerTeam;
    private CoreStats ownerStats;
    private DamageData damageData;

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
            shieldCollider = GetComponent<Collider>();
        }

        if (shieldCollider != null)
        {
            shieldCollider.enabled = true;
        }
    }

    private void Start()
    {
    }

    void InitializeEffect()
    {
        if (attackEffect != null)
        {
            var main = attackEffect.main;

            main.startLifetime = activeTime;
            Color ownerColor = ownerTeam == Team.Player ? owner.GetComponent<PlayerController>().GetColor() : owner.GetComponent<EnemyController>().GetColor();

            main.startColor = ownerColor;
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
            shieldCollider.enabled = true;
        }

        damageData = new DamageData
        {
            baseDamage = ownerStats.damage,
            canCrit = false,
            critChance = 0f,
            critMultiplier = 1f
        };

        InitializeEffect();
    }

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

    private void PerformAttackHit()
    {
        if (attackPoint == null)
        {
            Debug.LogError($"{name}: Attack point is missing.");

            return;
        }

        Collider[] hits = Physics.OverlapBox(
            attackPoint.position,
            attackSize * 0.5f,
            attackPoint.rotation,
            attackLayer,
            QueryTriggerInteraction.Ignore
        );

        //Debug.Log("the hits are " + hits);
        foreach (Collider hit in hits)
        {
            IDamageable target = hit.GetComponentInParent<IDamageable>();

            if (target == null)
                continue;

            if (target.Team == ownerTeam)
                continue;

            target.TakeDamage(damageData, ownerStats);
        }
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

    private void OnDrawGizmos()
    {
        if (attackPoint == null)
            return;

        Gizmos.matrix = Matrix4x4.TRS(
            attackPoint.position,
            attackPoint.rotation,
            Vector3.one
        );

        Gizmos.DrawWireCube(Vector3.zero, attackSize);
    }
    private IEnumerator ShieldAttackRoutine()
    {
        canAttack = false;

        OnAttack?.Invoke();

        float scaledAttackStartDelay = GetScaledDuration(attackStartDelay);

        if (scaledAttackStartDelay > 0f)
        {
            yield return new WaitForSeconds(scaledAttackStartDelay);
        }

        isAttacking = true;

        OnShieldAttackStarted?.Invoke();
        PerformAttackHit();

        if (attackEffect != null)
        {
            attackEffect.GetComponent<ParticleSystem>().Play();
        }


        float scaledActiveTime = GetScaledDuration(activeTime);

        if (scaledActiveTime > 0f)
        {
            yield return new WaitForSeconds(scaledActiveTime);
        }

        isAttacking = false;

        OnShieldAttackFinished?.Invoke();

        float totalAttackInterval = SECOND / EffectiveActionSpeed;


        float remainingCooldown =
            Mathf.Max(
                totalAttackInterval -
                scaledAttackStartDelay -
                scaledActiveTime,
                0f);

        if (remainingCooldown > 0f)
        {
            yield return new WaitForSeconds(remainingCooldown);
        }

        canAttack = true;
    }

    private float GetScaledDuration(float baseDuration)
    {
        return Mathf.Max(baseDuration / ActionSpeedMultiplier, 0f);
    }

    public void SetActionSpeedMultiplier(float multiplier)
    {
        actionSpeedMultiplier = Mathf.Max(multiplier, 0.01f);
    }

    public void AddActionSpeedMultiplier(float amount)
    {
        actionSpeedMultiplier =Mathf.Max(actionSpeedMultiplier + amount,0.01f);
    }

    //public bool CanReflectProjectile(Team projectileTeam)
    //{
    //    return isAttacking && projectileTeam != ownerTeam;
    //    //return projectileTeam != ownerTeam;
    //}

    public void NotifyProjectileReflected()
    {
        //damageData.baseDamage *= 1.5f;
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

    public void SetActiveTime(float newActiveTime)
    {
        activeTime = Mathf.Max(newActiveTime, 0f);
    }

    public float GetEffectiveRange()
    {
        return EffectiveRange;
    }

    public void SetEffectiveRange(float newRange)
    {
        effectiveRange = Mathf.Max(newRange, 0f);
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