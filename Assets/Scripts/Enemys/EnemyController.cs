using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
    [Header("Movement")]
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Stats")]
    [SerializeField] private CoreStats coreStats;

    public CoreStats CoreStats => coreStats;
    public Team Team => Team.Enemy;

    [Header("Combat Equipment")]
    [Tooltip("Assign BowWeapon, CrossbowWeapon, or ShieldController.")]
    [SerializeField] private MonoBehaviour combatEquipmentBehaviour;

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
        if (coreStats != null)
        {
            coreStats.currentHP = coreStats.maxHP;
        }
        else
        {
            Debug.LogError(
                $"{name}: CoreStats is not assigned.");
        }

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

        RotateTowards(currentTarget.transform.position);
        HandleAttack();
        UpdateAnimationState();
    }

    private void ResolveCombatEquipment()
    {
        if (combatEquipmentBehaviour == null)
        {
            Debug.LogWarning(
                $"{name}: No combat equipment assigned.");

            return;
        }

        combatEquipment =
            combatEquipmentBehaviour as ICombatEquipment;

        if (combatEquipment == null)
        {
            Debug.LogError(
                $"{name}: {combatEquipmentBehaviour.GetType().Name} " +
                $"must implement {nameof(ICombatEquipment)}.");
        }
    }

    private void InitializeCombatEquipment()
    {
        if (combatEquipment == null)
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

        combatEquipment.OnAttack += HandleAttackEvent;
        combatEquipment.OnReloadStarted += HandleReloadStarted;
        combatEquipment.OnReloadFinished += HandleReloadFinished;
        combatEquipment.OnActionFinished += HandleActionFinished;
    }

    private void UnsubscribeFromEquipmentEvents()
    {
        if (combatEquipment == null)
            return;

        combatEquipment.OnAttack -= HandleAttackEvent;
        combatEquipment.OnReloadStarted -= HandleReloadStarted;
        combatEquipment.OnReloadFinished -= HandleReloadFinished;
        combatEquipment.OnActionFinished -= HandleActionFinished;
    }

    private bool CanAct()
    {
        return !isDead &&
               !isInactive &&
               !isPaused;
    }

    private void HandleAttack()
    {
        if (combatEquipment == null)
            return;

        /*
         * The equipment decides whether it can currently act.
         *
         * Bow:
         * - returns false during its cooldown.
         *
         * Crossbow:
         * - returns false while bursting or reloading.
         *
         * Shield:
         * - returns false while attacking or cooling down.
         */
        combatEquipment.Use();
    }

    private PlayerController GetClosestPlayer()
    {
        if (PlayerManager.Instance == null)
            return null;

        float minimumSqrDistance = float.MaxValue;
        PlayerController closestPlayer = null;

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

            minimumSqrDistance = sqrDistance;
            closestPlayer = player;
        }

        return closestPlayer;
    }

    private void RotateTowards(Vector3 targetPosition)
    {
        Vector3 direction =
            targetPosition - transform.position;

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
                rotationSpeed * Time.deltaTime);
    }

    private void HandleAttackEvent()
    {
        if (isDead || isInactive)
            return;

        PlayAttackSound();

        if (animator == null)
            return;

        /*
         * This event is invoked once for every actual attack.
         *
         * Crossbow with 3 shots:
         * - event invoked 3 times
         * - attack animation plays 3 times
         *
         * Bow spread:
         * - event invoked once
         *
         * Shield attack:
         * - event invoked once
         */
        animator.SetTrigger("Attack");

        float animationSpeed =
            combatEquipment?.GetAnimationSpeed() ?? 1f;

        animator.SetFloat(
            "AttackSpeed",
            Mathf.Max(animationSpeed, 0.01f));
    }

    private void HandleReloadStarted()
    {
        if (isDead || animator == null)
            return;

        animator.ResetTrigger("Attack");
        animator.SetBool("Reloading", true);
        animator.SetTrigger("Reload");

        PlayRandomSound(reloadSoundEffects);
    }

    private void HandleReloadFinished()
    {
        if (animator == null)
            return;

        animator.SetBool("Reloading", false);
    }

    private void HandleActionFinished()
    {
        /*
         * Optional event.
         *
         * Useful when an equipment action has fully finished:
         * - bow cooldown completed;
         * - crossbow reload completed;
         * - shield attack and cooldown completed.
         *
         * No enemy-side cooldown is necessary because the equipment
         * already controls whether Use() succeeds.
         */
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
            $"{name} received damage. Remaining HP: " +
            $"{coreStats.currentHP}/{coreStats.maxHP}");

        PlayRandomSound(hitSoundEffects);

        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        // Update the enemy health bar here.
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        UnsubscribeFromEquipmentEvents();

        if (combatEquipmentBehaviour != null)
        {
            combatEquipmentBehaviour.enabled = false;
        }

        if (animator != null)
        {
            animator.SetBool("Reloading", false);
            animator.SetBool("Dead", true);
        }

        DisableEnemyColliders();

        Destroy(gameObject, 2f);
    }

    private void DisableEnemyColliders()
    {
        Collider[] colliders =
            GetComponentsInChildren<Collider>();

        foreach (Collider enemyCollider in colliders)
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
        animator.SetBool("Inactive", isInactive);
    }

    public void PlayAttackSound()
    {
        PlayRandomSound(attackSoundEffects);
    }

    private void PlayRandomSound(AudioClip[] sounds)
    {
        if (sounds == null || sounds.Length == 0)
            return;

        int randomIndex =
            Random.Range(0, sounds.Length);

        float randomVolume =
            Random.Range(0.7f, 0.9f);

        float randomPitch =
            Random.Range(0.8f, 1.2f);

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