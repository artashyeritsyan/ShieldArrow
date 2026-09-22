using System.Collections;
using UnityEngine;

public enum EnemyCombatType
{
    Archer,
    Crossbower,
    Shieldman
}

public class EnemyController : MonoBehaviour, IDamageable
{
    [Header("Enemy Class")]
    [SerializeField] private EnemyCombatType combatType = EnemyCombatType.Archer;
    [SerializeField] private Color enemyColor = Color.red;
    private Renderer[] renderers;
    private Color[] originalColors;
    [SerializeField] private float flashDuration = 0.1f;

    [Header("Movement")]
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Stats")]
    [SerializeField] private EnemyStats baseStats;
    private CoreStats stats;


    public CoreStats CoreStats => stats;
    public Team Team => Team.Enemy;

    [Header("Combat Equipment")]
    [SerializeField] private MonoBehaviour combatEquipmentBehaviour;

    [SerializeField] private float equipmentSpeedMultiplier = 1f;

    private ICombatEquipment combatEquipment;

    [Header("Animation")]
    [SerializeField] protected Animator animator;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip[] attackSoundEffects;
    [SerializeField] private AudioClip[] reloadSoundEffects;
    [SerializeField] private AudioClip[] hitSoundEffects;
    [SerializeField] private AudioClip[] deadSoundEffects;


    [Header("State")]
    [SerializeField] private bool isDead;
    [SerializeField] private bool isInactive;
    //[SerializeField] private bool isPaused;

    private PlayerController currentTarget;

    [SerializeField] private Coin coinPrefab;

    private void Awake()
    {
        ResolveCombatEquipment();
        ConfigureClassAnimation();

        renderers = GetComponentsInChildren<Renderer>();

        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].material.color;
        }
    }

    private void Start()
    {

        InitializeStats();
        InitializeCombatEquipment();
        SubscribeToEquipmentEvents();

        SetInactive(true);
    }

    private void Update()
    {
        if (!CanAct())
        {
            //Debug.LogWarning("Cant ACT");

            UpdateAnimationState();
            return;
        }

        currentTarget = GetClosestPlayer();

        if (currentTarget == null)
        {
            UpdateAnimationState();
            return;
        }

        Vector3 targetPosition = currentTarget.transform.position;

        RotateTowards(targetPosition);

        float targetSqrDistance = (targetPosition - transform.position).sqrMagnitude;

        if (IsTargetWithinAttackRange(targetSqrDistance))
        {
            HandleAttack();
        }

        UpdateAnimationState();
    }

    private void ResolveCombatEquipment()
    {
        if (combatEquipmentBehaviour == null)
        {
            Debug.LogError($"{name}: Combat equipment is not assigned.");

            return;
        }

        combatEquipment = combatEquipmentBehaviour as ICombatEquipment;

        if (combatEquipment == null)
        {
            Debug.LogError(
                $"{name}: " +
                $"{combatEquipmentBehaviour.GetType().Name} " +
                $"must implement " +
                $"{nameof(ICombatEquipment)}.");

            return;
        }

        ValidateEquipmentForClass();
    }

    private void ValidateEquipmentForClass()
    {
        bool isValid = combatType switch
            {
                EnemyCombatType.Archer => combatEquipmentBehaviour is BowWeapon,

                EnemyCombatType.Crossbower => combatEquipmentBehaviour is CrossbowWeapon,

                EnemyCombatType.Shieldman => combatEquipmentBehaviour is ShieldController,

                _ => false
            };

        if (!isValid)
        {
            Debug.LogWarning(
                $"{name}: Enemy class is {combatType}, " +
                $"but assigned equipment is " +
                $"{combatEquipmentBehaviour.GetType().Name}.");
        }
    }

    private void ConfigureClassAnimation()
    {
        if (animator == null)
            return;

        //animator.SetInteger("CombatType", (int)combatType);
    }

    private void InitializeStats()
    {
        if (baseStats.GetStats() == null)
        {
            Debug.LogError($"{name}: CoreStats is not assigned.");

            return;
        }
        stats = baseStats.GetStats().Clone();
        stats.currentHP = stats.maxHP;
        Debug.Log(stats.currentHP);
    }

    private void InitializeCombatEquipment()
    {
        if (combatEquipment == null || stats == null)
        {
            return;
        }
        
        combatEquipment.Initialize(gameObject, Team.Enemy, stats);

        combatEquipment.SetActionSpeedMultiplier(equipmentSpeedMultiplier);
    }

    private void SubscribeToEquipmentEvents()
    {
        if (combatEquipment == null)
            return;

        combatEquipment.OnAttack += HandleAttackEvent;

        combatEquipment.OnReloadStarted += HandleReloadStarted;

        combatEquipment.OnReloadFinished += HandleReloadFinished;
    }

    private void UnsubscribeFromEquipmentEvents()
    {
        if (combatEquipment == null)
            return;

        combatEquipment.OnAttack -= HandleAttackEvent;

        combatEquipment.OnReloadStarted -= HandleReloadStarted;

        combatEquipment.OnReloadFinished -= HandleReloadFinished;
    }

    private bool CanAct()
    {
        return !isDead && !isInactive && (GameManager.Instance.CurrentGameState != GameStates.Pause) && combatEquipment != null;
    }

    private bool IsTargetWithinAttackRange(float targetSqrDistance)
    {
        if (combatEquipment == null)
            return false;

        float attackRange = combatEquipment.EffectiveRange;

        return targetSqrDistance <= attackRange * attackRange;
    }

    private void HandleAttack()
    {
        combatEquipment?.Use();
    }

    private PlayerController GetClosestPlayer()
    {
        if (PlayerManager.Instance == null)
            return null;

        float minimumSqrDistance = float.MaxValue;

        PlayerController closestPlayer = null;

        foreach (PlayerController player in PlayerManager.Instance.GetPlayers())
        {
            if (player == null)
            {
                Debug.LogWarning("NO CLOSEST PLAYER");
                continue;
            }

            Vector3 difference = player.transform.position - transform.position;

            float sqrDistance = difference.sqrMagnitude;

            if (sqrDistance >= minimumSqrDistance)
            {
                continue;
            }

            minimumSqrDistance = sqrDistance;

            closestPlayer = player;
        }
        Debug.Log("CLosest Player is " + closestPlayer.name);
        return closestPlayer;
    }

    private void RotateTowards(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                rotationSpeed * Time.deltaTime);
    }

    private void HandleAttackEvent()
    {
        if (isDead || isInactive)
            return;

        PlayAttackSound();

        if (animator == null)
            return;

        animator.SetFloat("AttackSpeed", combatEquipment.EffectiveActionSpeed);
        animator.SetFloat("AttackSpeed", 1);

        animator.SetTrigger("Attack");
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

    public void SetEquipmentSpeedMultiplier(float multiplier)
    {
        equipmentSpeedMultiplier = Mathf.Max(multiplier, 0.01f);

        combatEquipment?.SetActionSpeedMultiplier(equipmentSpeedMultiplier);
    }

    public void UpgradeEquipmentSpeed(float multiplierIncrease)
    {
        equipmentSpeedMultiplier =
            Mathf.Max(
                equipmentSpeedMultiplier +
                multiplierIncrease,
                0.01f);

        combatEquipment?.SetActionSpeedMultiplier(equipmentSpeedMultiplier);
    }

    public void DamageFlash()
    {
        //StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = Color.white;
        }

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = originalColors[i];
        }
    }

    public float GetEquipmentSpeedMultiplier()
    {
        return equipmentSpeedMultiplier;
    }

    public Color GetColor()
    {
        return enemyColor;
    }

    public void TakeDamage(DamageData damageData, CoreStats attackerStats)
    {
        if (isDead || stats == null)
            return;

        DamageSystem.ProcessDamage(damageData, attackerStats, stats);

        OnDamageReceived();

        DamageFlash();

        if (stats.currentHP <= 0f)
        {
            Die();
        }
    }

    private void OnDamageReceived()
    {
        // TODO: Remove this log
        Debug.Log(
            $"{name} received damage. " +
            $"Remaining HP: " +
            $"{stats.currentHP}/" +
            $"{stats.maxHP}");

        // Playing damaged sound
        PlayRandomSound(hitSoundEffects);

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

        if (combatEquipmentBehaviour != null)
        {
            combatEquipmentBehaviour.enabled =
                false;
        }

        if (animator != null)
        {
            animator.SetBool("Reloading", false);

            animator.SetBool("Dead", true);
        }

        DisableEnemyColliders();

        PlayRandomSound(deadSoundEffects);

        SpawnCoin();

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
        animator.SetBool("Inactive", isInactive);
    }

    private void SpawnCoin()
    {
        ObjectPoolManager.Instance.Spawn(coinPrefab, transform.position, Quaternion.identity);
    }

    private void PlayAttackSound()
    {
        PlayRandomSound(attackSoundEffects);
    }

    private void PlayRandomSound(AudioClip[] sounds)
    {
        if (sounds == null || sounds.Length == 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, sounds.Length);
        float randomVolume = Random.Range(0.5f, 0.6f);
        float randomPitch = Random.Range(0.9f, 1.1f);

        AudioManager.Play(sounds[randomIndex], randomVolume, randomPitch);
    }

    public void SetInactive(bool value)
    {
        isInactive = value;

        if (animator != null)
        {
            animator.SetBool("Inactive", isInactive);
        }
    }


    private void OnDestroy()
    {
        UnsubscribeFromEquipmentEvents();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter" +  other.name);
        if (other.CompareTag("UpperBorder"))
        {
            SetInactive(false);

            Debug.Log("Enemy" + gameObject.name + "Activates");
        }
    }




}