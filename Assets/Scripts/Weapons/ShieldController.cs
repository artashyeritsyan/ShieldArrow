using System;
using System.Collections;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
    private const float SECOND = 1f;

    [Header("Shield")]
    [SerializeField] private Collider shieldCollider;

    [Header("Attack")]
    [SerializeField] private float attackSpeed = 1f;
    [SerializeField] private float activeTime = 0.25f;

    private GameObject owner;
    private Team ownerTeam;
    private CoreStats ownerStats;

    private bool canAttack = true;
    private bool isAttacking;
    private bool isInitialized;

    public event Action OnShieldAttackStarted;
    public event Action OnShieldAttackFinished;
    public event Action OnProjectileReflected;

    private void Awake()
    {
        if (shieldCollider != null)
        {
            shieldCollider = gameObject.GetComponent<Collider>();
            //shieldCollider.enabled = false;
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
            Debug.LogError($"{name}: Shield is not initialized.");
            return false;
        }

        if (shieldCollider == null)
        {
            Debug.LogError($"{name}: Shield collider is missing.");
            return false;
        }

        return true;
    }

    private IEnumerator ShieldAttackRoutine()
    {
        canAttack = false;
        isAttacking = true;

        shieldCollider.enabled = true;
        OnShieldAttackStarted?.Invoke();

        yield return new WaitForSeconds(
            Mathf.Max(activeTime, 0f));

        shieldCollider.enabled = false;
        isAttacking = false;

        OnShieldAttackFinished?.Invoke();

        float cooldown =
            SECOND / Mathf.Max(attackSpeed, 0.01f);

        yield return new WaitForSeconds(cooldown);

        canAttack = true;
    }

    public bool CanReflectProjectile(Team projectileTeam)
    {
        return isAttacking && projectileTeam != ownerTeam;
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

    private void OnEnable()
    {
        canAttack = true;
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        canAttack = false;
        isAttacking = false;

        if (shieldCollider != null)
            shieldCollider.enabled = false;
    }
}