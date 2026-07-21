using UnityEngine;

public enum EnemyCombatType
{
    Archer,
    Crossbower,
    Shieldman
}

public class EnemyController : MonoBehaviour, IDamageable
{
    [Header("Enemy Type")]
    [SerializeField]
    private EnemyCombatType combatType =
        EnemyCombatType.Archer;

    [Header("Movement")]
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Attack Range")]
    [SerializeField] private float rangedAttackRange = 12f;
    [SerializeField] private float shieldAttackRange = 2f;

    [Header("Stats")]
    [SerializeField] private CoreStats coreStats;

    public CoreStats CoreStats => coreStats;
    public Team Team => Team.Enemy;

    [Header("Combat Equipment")]
    [Tooltip("Bow used when enemy type is Archer.")]
    [SerializeField] private BowWeapon bowWeapon;

    [Tooltip("Crossbow used when enemy type is Crossbower.")]
    [SerializeField] private CrossbowWeapon crossbowWeapon;

    [Tooltip("Shield used when enemy type is Shieldman.")]
    [SerializeField] private ShieldController shieldController;

    private MonoBehaviour activeEquipmentBehaviour;
    private ICombatEquipment combatEquipment;

    [Header("Animation")]
    [SerializeField] protected Animator animator;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip[] attackSoundEffects;
    [SerializeField] private AudioClip[] reloadSoundEffects;
    [SerializeField] private AudioClip[] hitSoundEffects;

    [Header("State")]
    [SerializeField] private bool isDead;
    [SerializeField] private bool isInactive;
    [SerializeField] private bool isPaused;

    private PlayerController currentTarget;

    private void Awake()
    {
        ResolveCombatEquipment();
    }

    private void Start()
    {
        InitializeStats();
        InitializeCombatEquipment();
        SubscribeToEquipmentEvents();
    }

    private void Update()
    {
        if (!CanAct())
        {
            UpdateAnimationState();
            return;
        }

        currentTarget = GetClosestPlayer();

        if (currentTarget == null)
        {
            UpdateAnimationState();
            return;
        }

        Vector3 targetPosition =
            currentTarget.transform.position;

        RotateTowards(targetPosition);

        float sqrDistance =
            (targetPosition - transform.position)
            .sqrMagnitude;

        if (IsTargetWithinAttackRange(sqrDistance))
        {
            HandleAttack();
        }

        UpdateAnimationState();
    }

    private void InitializeStats()
    {
        if (coreStats == null)
        {
            Debug.LogError(
                $"{name}: CoreStats is not assigned.");

            return;
        }

        coreStats.currentHP = coreStats.maxHP;
    }

    private void ResolveCombatEquipment()
    {
        DisableAllEquipment();

        switch (combatType)
        {
            case EnemyCombatType.Archer:
                SelectEquipment(bowWeapon);
                break;

            case EnemyCombatType.Crossbower:
                SelectEquipment(crossbowWeapon);
                break;

            case EnemyCombatType.Shieldman:
                SelectEquipment(shieldController);
                break;

            default:
                Debug.LogError(
                    $"{name}: Unsupported enemy combat type: " +
                    $"{combatType}.");

                break;
        }

        if (activeEquipmentBehaviour == null)
        {
            Debug.LogError(
                $"{name}: Equipment for {combatType} " +
                $"is not assigned.");
        }
    }

    private void SelectEquipment(
        MonoBehaviour equipmentBehaviour)
    {
        if (equipmentBehaviour == null)
            return;

        combatEquipment =
            equipmentBehaviour as ICombatEquipment;

        if (combatEquipment == null)
        {
            Debug.LogError(
                $"{name}: " +
                $"{equipmentBehaviour.GetType().Name} " +
                $"must implement {nameof(ICombatEquipment)}.");

            return;
        }

        activeEquipmentBehaviour =
            equipmentBehaviour;

        activeEquipmentBehaviour.enabled = true;
    }

    private void DisableAllEquipment()
    {
        SetEquipmentEnabled(bowWeapon, false);
        SetEquipmentEnabled(crossbowWeapon, false);
        SetEquipmentEnabled(shieldController, false);

        activeEquipmentBehaviour = null;
        combatEquipment = null;
    }

    private void SetEquipmentEnabled(
        MonoBehaviour equipment,
        bool value)
    {
        if (equipment != null)
        {
            equipment.enabled = value;
        }
    }

    private void InitializeCombatEquipment()
    {
        if (combatEquipment == null)
            return;

        if (coreStats == null)
            return;

        combatEquipment.Initialize(
            gameObject,
            Team.Enemy,
            coreStats);
    }

    private void SubscribeToEquipmentEvents()
    {
        if (combatEquipment == null)
            return;

        combatEquipment.OnAttack +=
            HandleAttackEvent;

        combatEquipment.OnReloadStarted +=
            HandleReloadStarted;

        combatEquipment.OnReloadFinished +=
            HandleReloadFinished;
    }

    private void UnsubscribeFromEquipmentEvents()
    {
        if (combatEquipment == null)
            return;

        combatEquipment.OnAttack -=
            HandleAttackEvent;

        combatEquipment.OnReloadStarted -=
            HandleReloadStarted;

        combatEquipment.OnReloadFinished -=
            HandleReloadFinished;
    }

    private bool CanAct()
    {
        return
            !isDead &&
            !isInactive &&
            !isPaused;
    }

    private bool IsTargetWithinAttackRange(
        float targetSqrDistance)
    {
        float attackRange =
            GetAttackRange();

        return targetSqrDistance <=
               attackRange * attackRange;
    }

    private float GetAttackRange()
    {
        switch (combatType)
        {
            case EnemyCombatType.Shieldman:
                return Mathf.Max(
                    shieldAttackRange,
                    0f);

            case EnemyCombatType.Archer:
            case EnemyCombatType.Crossbower:
                return Mathf.Max(
                    rangedAttackRange,
                    0f);

            default:
                return 0f;
        }
    }

    private void HandleAttack()
    {
        if (combatEquipment == null)
            return;

        /*
         * Equipment controls its own availability:
         *
         * Bow:
         * cooldown and spread shooting.
         *
         * Crossbow:
         * burst and reload.
         *
         * Shield:
         * active collider and cooldown.
         */
        combatEquipment.Use();
    }

    private PlayerController GetClosestPlayer()
    {
        if (PlayerManager.Instance == null)
            return null;

        float minimumSqrDistance =
            float.MaxValue;

        PlayerController closestPlayer =
            null;

        foreach (
            PlayerController player in
            PlayerManager.Instance.GetPlayers())
        {
            if (player == null)
                continue;

            Vector3 difference =
                player.transform.position -
                transform.position;

            float sqrDistance =
                difference.sqrMagnitude;

            if (sqrDistance >= minimumSqrDistance)
                continue;

            minimumSqrDistance =
                sqrDistance;

            closestPlayer = player;
        }

        return closestPlayer;
    }

    private void RotateTowards(
        Vector3 targetPosition)
    {
        Vector3 direction =
            targetPosition -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction.normalized);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed *
                Time.deltaTime);
    }

    private void HandleAttackEvent()
    {
        if (isDead || isInactive)
            return;

        PlayAttackSound();

        if (animator == null)
            return;

        /*
         * Bow spread:
         * one event for the full spread.
         *
         * Crossbow:
         * one event for every burst shot.
         *
         * Shield:
         * one event when shield animation starts.
         */
        animator.SetTrigger("Attack");

        animator.SetFloat(
            "AttackSpeed",
            GetEquipmentAttackSpeed());
    }

    private float GetEquipmentAttackSpeed()
    {
        switch (combatType)
        {
            case EnemyCombatType.Archer:
                return bowWeapon != null
                    ? bowWeapon.GetShootingSpeed()
                    : 1f;

            case EnemyCombatType.Crossbower:
                return crossbowWeapon != null
                    ? crossbowWeapon.GetShootingSpeed()
                    : 1f;

            case EnemyCombatType.Shieldman:
                return shieldController != null
                    ? shieldController.GetAttackSpeed()
                    : 1f;

            default:
                return 1f;
        }
    }

    private void HandleReloadStarted()
    {
        if (isDead || animator == null)
            return;

        animator.ResetTrigger("Attack");
        animator.SetBool("Reloading", true);
        animator.SetTrigger("Reload");

        PlayRandomSound(
            reloadSoundEffects);
    }

    private void HandleReloadFinished()
    {
        if (animator == null)
            return;

        animator.SetBool(
            "Reloading",
            false);
    }

    public void TakeDamage(
        DamageData damageData,
        CoreStats attackerStats)
    {
        if (isDead || coreStats == null)
            return;

        DamageSystem.ProcessDamage(
            damageData,
            attackerStats,
            coreStats);

        OnDamageReceived();

        if (coreStats.currentHP <= 0f)
        {
            Die();
        }
    }

    private void OnDamageReceived()
    {
        Debug.Log(
            $"{name} received damage. " +
            $"Remaining HP: " +
            $"{coreStats.currentHP}/" +
            $"{coreStats.maxHP}");

        PlayRandomSound(
            hitSoundEffects);

        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        UnsubscribeFromEquipmentEvents();

        if (activeEquipmentBehaviour != null)
        {
            activeEquipmentBehaviour.enabled =
                false;
        }

        if (animator != null)
        {
            animator.SetBool(
                "Reloading",
                false);

            animator.SetBool(
                "Dead",
                true);
        }

        DisableEnemyColliders();

        Destroy(gameObject, 2f);
    }

    private void DisableEnemyColliders()
    {
        Collider[] colliders =
            GetComponentsInChildren<Collider>();

        foreach (
            Collider enemyCollider in
            colliders)
        {
            enemyCollider.enabled = false;
        }
    }

    private void UpdateAnimationState()
    {
        if (animator == null)
            return;

        animator.SetFloat("Blend", 0f);
        animator.SetBool("Dead", isDead);
        animator.SetBool(
            "Inactive",
            isInactive);
    }

    public void PlayAttackSound()
    {
        PlayRandomSound(
            attackSoundEffects);
    }

    private void PlayRandomSound(
        AudioClip[] sounds)
    {
        if (sounds == null ||
            sounds.Length == 0)
        {
            return;
        }

        int randomIndex =
            Random.Range(
                0,
                sounds.Length);

        float randomVolume =
            Random.Range(
                0.7f,
                0.9f);

        float randomPitch =
            Random.Range(
                0.8f,
                1.2f);

        AudioManager.Play(
            sounds[randomIndex],
            randomVolume,
            randomPitch);
    }

    public void SetInactive(bool value)
    {
        isInactive = value;

        if (animator != null)
        {
            animator.SetBool(
                "Inactive",
                isInactive);
        }
    }

    public void SetPaused(bool value)
    {
        isPaused = value;
    }

    private void OnDestroy()
    {
        UnsubscribeFromEquipmentEvents();
    }
}