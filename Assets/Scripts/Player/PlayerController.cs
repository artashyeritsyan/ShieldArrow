using Unity.VisualScripting;
using UnityEngine;

public enum PlayerClass
{
    Archer,
    Knight,
    Crossbower,
    Barbarian,
    Assasin,
    Mage
}

public enum Team
{
    Player,
    Enemy
}

public enum InteractionType
{
    None,
    ClassChange,
    Door,
    Shop
}

public class PlayerController : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] protected PlayerClass playerClass;
    [SerializeField] protected Color playerColor;

    [SerializeField] private PlayerStatsProfile baseProfile;
    [SerializeField] protected PlayerStats stats;

    [SerializeField] private float speedMultiplier = 4000f;

    [Header("Movement")]
    [SerializeField] private float speedTransitionTime = 10f;

    private Vector3 moveDirection;
    private Vector3 lerpedDirection;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float maxAngle = 60f;
    [SerializeField] private bool canRotate = true;

    public bool CanRotate => canRotate;

    [Header("Aim")]
    [SerializeField] private Transform aimTarget;
    [SerializeField] private TrajectoryPreview trajectory;
    [SerializeField] private Color aimColor;
    [SerializeField] private float aimDistance = 5f;
    [SerializeField] private float aimSpeed = 3f;
    public float MaxAimAngle => maxAngle;

    public Team Team => Team.Player;

    private Camera mainCamera;
    private AimsManager aimsManager;
    //private float currentAimAngle;
    private Vector3 aimCenterDirection;

    [Header("Combat Equipment")]
    [SerializeField] private MonoBehaviour combatEquipmentBehaviour;
    protected ICombatEquipment combatEquipment;

    [Header("Attack Speed")]
    [SerializeField] private float attackSpeedMultiplier = 1f;


    [Header("Animation")]
    [SerializeField] protected Animator animator;
    [SerializeField] private float animTransitionTime = 5f;
    [SerializeField] private float animLayerTransitionTime = 5f;

    private float blendValue;
    protected float layerTargetWeight;
    protected float layerCurrentWeight;

    [Header("Sound Effects")]
    [SerializeField] protected AudioClip[] attackSoundEffects;

    [Header("Main Modifiers")]
    [SerializeField] protected bool isDead;

    protected bool isPause;
    protected bool canMove;
    protected bool canAttack;

    protected Rigidbody rb;
    protected InputHandler input;

    public InteractionType CurrentInteraction { get; private set; }
    public GameObject CurrentInteractionObject { get; private set; }


    protected virtual void Awake()
    {
        InitializeStats();
        GetRequiredComponents();
        ResolveCombatEquipment();

        AfterAwake();
    }

    protected virtual void Start()
    {
        SubscribeToEvents();
        InitializeValues();
        InitializeCombatEquipment();
        SubscribeToEquipmentEvents();

        layerTargetWeight = 1f;

        AfterStart();
    }

    protected virtual void Update()
    {

        if (!isPause)
        {
            HandleAim();

            if (!isDead)
            {
                HandleMovement();
                HandleAttack();

                //HandleAim();

            }

            HandleAnimation();
            AfterUpdate();
        }
    }

    private void InitializeStats()
    {
        mainCamera = Camera.main;
        aimCenterDirection = transform.forward;
        aimCenterDirection.y = 0f;
        aimCenterDirection.Normalize();

        if (mainCamera == null)
        {
            Debug.LogError($"{name}: Main Camera was not found.");
        }

        if (baseProfile == null)
        {
            Debug.LogError($"{name}: PlayerStatsProfile is not assigned.");

            return;
        }

        stats = baseProfile.GetStats().Clone();
        stats.core.currentHP = stats.core.maxHP;

    }

    private void GetRequiredComponents()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<InputHandler>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (rb == null)
        {
            Debug.LogError($"{name}: Rigidbody component is missing.");
        }

        if (input == null)
        {
            Debug.LogError($"{name}: InputHandler component is missing.");
        }
    }

    private void InitializeValues()
    {
        aimsManager = FindFirstObjectByType<AimsManager>();
        EnableCollider();

        if (aimsManager != null && aimTarget != null)
        {
            aimsManager.AddPlayerAim(aimTarget, playerColor);
        }

        // just for making sure

        canMove = true;
        canAttack = false;
        canRotate = true;

        attackSpeedMultiplier = Mathf.Max(attackSpeedMultiplier, 0.01f);
    }

    public float GetRotation()
    {
        return transform.eulerAngles.y;
    }
    private void ResolveCombatEquipment()
    {
        if (combatEquipmentBehaviour == null)
        {
            Debug.LogWarning($"{name}: No combat equipment is assigned.");

            return;
        }

        combatEquipment = combatEquipmentBehaviour as ICombatEquipment;

        if (combatEquipment == null)
        {
            Debug.LogError(
                $"{name}: Assigned component " +
                $"{combatEquipmentBehaviour.GetType().Name} " +
                $"does not implement " +
                $"{nameof(ICombatEquipment)}.");
        }
    }

    private void InitializeCombatEquipment()
    {
        if (combatEquipment == null || stats == null)
        {
            return;
        }

        combatEquipment.Initialize(gameObject, Team, stats.core);

        /*
         * Apply the player's current multiplier
         * to any equipment type.
         */
        combatEquipment.SetActionSpeedMultiplier(attackSpeedMultiplier);
    }

    private void SubscribeToEquipmentEvents()
    {
        if (combatEquipment == null)
            return;

        combatEquipment.OnAttack +=
            HandleEquipmentAttack;

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
            HandleEquipmentAttack;

        combatEquipment.OnReloadStarted -=
            HandleReloadStarted;

        combatEquipment.OnReloadFinished -=
            HandleReloadFinished;
    }

    private void HandleMovement()
    {
        if (rb == null ||
            input == null ||
            stats == null)
        {
            return;
        }

        if (canMove)
        {
            moveDirection =
                new Vector3(
                    input.move.x,
                    0f,
                    input.move.y);
        }
        else
        {
            moveDirection =
                Vector3.zero;
        }

        lerpedDirection =
            Vector3.Lerp(
                lerpedDirection,
                moveDirection,
                Time.deltaTime *
                speedTransitionTime);

        rb.AddForce(
            stats.core.speed *
            speedMultiplier *
            Time.deltaTime *
            lerpedDirection);
    }

    private void HandleAttack()
    {
        if (input == null ||
            !input.attack ||
            !canAttack ||
            combatEquipment == null)
        {
            return;
        }

        combatEquipment.Use();
    }

    private void HandleEquipmentAttack()
    {
        if (isDead)
            return;

        PlayAttackSound();

        if (animator != null)
        {

            animator.SetFloat("AttackSpeed", combatEquipment != null ? combatEquipment.EffectiveActionSpeed : 1f);

            animator.SetTrigger("Attack");
        }

        OnEquipmentAttack();
    }

    private void HandleReloadStarted()
    {
        if (isDead)
            return;

        if (animator != null)
        {
            animator.ResetTrigger("Attack");

            animator.SetFloat(
                "AttackSpeed",
                combatEquipment != null
                    ? combatEquipment
                        .ActionSpeedMultiplier
                    : 1f);

            animator.SetBool("Reloading", true);

            animator.SetTrigger("Reload");
        }

        OnEquipmentReloadStarted();
    }

    private void HandleReloadFinished()
    {
        if (animator != null)
        {
            animator.SetBool("Reloading", false);
        }

        OnEquipmentReloadFinished();
    }

    private void HandleAnimation()
    {
        if (animator == null)
            return;

        float movementAmount = moveDirection.magnitude;

        blendValue =
            Mathf.Lerp(
                blendValue,
                movementAmount,
                Time.deltaTime *
                animTransitionTime);

        layerCurrentWeight =
            Mathf.Lerp(
                layerCurrentWeight,
                layerTargetWeight,
                animLayerTransitionTime *
                Time.deltaTime);

        if (animator.layerCount > 1)
        {
            animator.SetLayerWeight(1, layerCurrentWeight);
        }

        animator.SetFloat("Blend", blendValue);

        animator.SetBool("Dead", isDead);

        AdditiveAnimation();
    }

    private void HandleAim()
    {
        if (input == null ||
            mainCamera == null ||
            aimTarget == null)
        {
            return;
        }

        Ray pointerRay = mainCamera.ScreenPointToRay(input.look);

        Plane aimPlane =
            new Plane(
                Vector3.up,
                transform.position);

        if (!aimPlane.Raycast(pointerRay, out float distance))
        {
            return;
        }

        Vector3 aimPoint = pointerRay.GetPoint(distance);

        // Aim icon always follows the pointer freely.
        aimTarget.position = aimPoint + Vector3.up * 0.05f;

        Vector3 desiredDirection = aimPoint - transform.position;

        desiredDirection.y = 0f;

        if (desiredDirection.sqrMagnitude <= 0.0001f)
            return;

        desiredDirection.Normalize();

        if (canRotate)
        {
            // Free 360-degree rotation.
            RotateCharacter(desiredDirection);
            //aimCenterDirection = transform.forward;
            //aimCenterDirection.y = 0f;
            //aimCenterDirection.Normalize();

            return;
        }

        // Restricted rotation: -maxAngle to +maxAngle.
        float angle = Vector3.SignedAngle(
            aimCenterDirection,
            desiredDirection,
            Vector3.up);

        float clampedAngle = Mathf.Clamp(
            angle,
            -maxAngle,
            maxAngle);

        Vector3 clampedDirection =
            Quaternion.AngleAxis(
                clampedAngle,
                Vector3.up) *
            aimCenterDirection;

        RotateCharacter(clampedDirection);
    }

    public void TakeDamage(
            DamageData damageData,
            CoreStats attackerStats)
    {
        if (isDead || stats == null)
            return;

        DamageSystem.ProcessDamage(
            damageData,
            attackerStats,
            stats.core);

        OnDamageReceived();

        if (stats.core.currentHP <= 0f)
        {
            Die();
        }
    }

    private void OnDamageReceived()
    {
        //TODO: remove this log
        Debug.Log(
            $"{name} received damage. " +
            $"Remaining HP: " +
            $"{stats.core.currentHP}/" +
            $"{stats.core.maxHP}");

        // Fix sound 
        //PlayRandomSound(hitSoundEffects);

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
            animator.SetBool(
                "Reloading",
                false);

            animator.SetBool(
                "Dead",
                true);
        }

        //DisableEnemyColliders();  // TODO: Implemen this part

        //Destroy(gameObject, 2f);
        //gameObject.SetActive(false);

        DisableColliders();
    }

    void DisableColliders()
    {
        gameObject.GetComponent<Collider>().enabled = false;
    }

    void EnableCollider()
    {
        gameObject.GetComponent<Collider>().enabled = true;
    }

    private void RotateCharacter(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed *
                Time.deltaTime);
    }

    public Vector3 GetClampedAimDirection(Vector3 targetPosition)
    {
        Vector3 desiredDirection = targetPosition - transform.position;
        desiredDirection.y = 0f;

        if (desiredDirection.sqrMagnitude <= 0.0001f)
            return transform.forward;

        desiredDirection.Normalize();

        if (canRotate)
            return desiredDirection;

        return Vector3.RotateTowards(
            transform.forward,
            desiredDirection,
            maxAngle * Mathf.Deg2Rad,
            0f
        ).normalized;
    }

    public void SetCanRotate(bool value)
    {
        canRotate = value;
    }

    protected float GetScaledEquipmentDuration(
        float baseDuration)
    {
        float multiplier =
            combatEquipment != null
                ? combatEquipment
                    .ActionSpeedMultiplier
                : attackSpeedMultiplier;

        return Mathf.Max(
            baseDuration /
            Mathf.Max(multiplier, 0.01f),
            0f);
    }

    public void SetAttackSpeedMultiplier(
        float multiplier)
    {
        attackSpeedMultiplier =
            Mathf.Max(
                multiplier,
                0.01f);

        combatEquipment?
            .SetActionSpeedMultiplier(
                attackSpeedMultiplier);
    }

    public void AddAttackSpeedMultiplier(
        float amount)
    {
        attackSpeedMultiplier =
            Mathf.Max(
                attackSpeedMultiplier +
                amount,
                0.01f);

        combatEquipment?
            .SetActionSpeedMultiplier(
                attackSpeedMultiplier);
    }

    public void UpgradeAttackSpeed(
        float amount)
    {
        AddAttackSpeedMultiplier(
            amount);
    }

    public float GetAttackSpeedMultiplier()
    {
        return attackSpeedMultiplier;
    }

    public float GetEffectiveAttackSpeed()
    {
        return combatEquipment != null
            ? combatEquipment
                .EffectiveActionSpeed
            : 0f;
    }

    public ICombatEquipment GetCombatEquipment()
    {
        return combatEquipment;
    }

    public virtual void GetHitAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
    }

    public virtual void PlayAttackSound()
    {
        PlayRandomSound(
            attackSoundEffects);
    }

    protected void PlayRandomSound(
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


    public void SetInteraction(InteractionType interaction)
    {
        CurrentInteraction = interaction;
    }

    public void ClearInteraction(InteractionType interaction)
    {
        if (CurrentInteraction == interaction)
            CurrentInteraction = InteractionType.None;
    }

    public bool CanInteractWith(InteractionType interaction)
    {
        return CurrentInteraction == interaction;
    }


    public PlayerClass GetPlayerClass()
    {
        return playerClass;
    }

    public Color GetPlayerColor()
    {
        return playerColor;
    }

    public PlayerStats GetStats()
    {
        return stats;
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
    }

    public bool GetCanMove()
    {
        return canMove;
    }

    public void SetCanAttack(bool value)
    {
        canAttack = value;
    }

    public bool GetCanAttack()
    {
        return canAttack;
    }

    public void SetPaused(bool value)
    {
        isPause = value;
    }

    void HandleGameStateChanged(GameStates gameState)
    {
        switch (gameState)
        {
            case GameStates.Menu:
                isPause = true;
                break;

            case GameStates.Playing:
                isPause = false;
                //canRotate = true;
                break;
            case GameStates.Pause:
                isPause = true;
                break;
        }
    }

    void HandleLevelStateChanged(LevelStates levelState)
    {
        switch (levelState)
        {
            case LevelStates.Lobby:
                canAttack = false;
                canRotate = true;
                SetTrajectoryEnable(false);
                break;
            case LevelStates.Aisle: 
                canAttack = true;
                canRotate = false;
                SetTrajectoryEnable(true);
                break;
            case LevelStates.Room:
                canAttack = false;
                canRotate = true;
                SetTrajectoryEnable(false);
                break;
            case LevelStates.Boss:
                canAttack = true;
                canRotate = false;
                SetTrajectoryEnable(true);
                break;
            default:
                canAttack = false;
                break;  
        }
    }

    void SetTrajectoryEnable(bool value)
    {
        trajectory.gameObject.SetActive(value);
    }
    private void SubscribeToEvents()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null.", this);
            return;
        }

        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        GameManager.Instance.OnLevelStateChanged += HandleLevelStateChanged;

    }

    protected virtual void AfterAwake()
    {
    }

    protected virtual void AfterStart()
    {
    }

    protected virtual void AfterUpdate()
    {
    }

    protected virtual void OnEquipmentAttack()
    {
    }

    protected virtual void OnEquipmentReloadStarted()
    {
    }

    protected virtual void OnEquipmentReloadFinished()
    {
    }

    public virtual void Dead()
    {
    }

    public virtual void AdditiveAnimation()
    {
    }

    protected virtual void OnDestroy()
    {
        UnsubscribeFromEquipmentEvents();

        if (aimsManager != null && aimTarget != null)
        {
            aimsManager.RemovePlayerAim(aimTarget);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            GameManager.Instance.OnLevelStateChanged -= HandleLevelStateChanged;
        }
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.CompareTag("Enemy"))
    //    {
    //        TakeDamage(collision.gameObject.GetComponent<EnemyController>().CoreStats);)
    //    }
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerChoosing"))
        {

        }

        //if (other.CompareTag("NextLevel"))
        //{
        //    GameManager.Instance.NextLevel();
        //}
    }
}