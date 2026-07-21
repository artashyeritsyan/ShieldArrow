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

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected PlayerClass playerClass;
    [SerializeField] private PlayerStatsProfile baseProfile;
    [SerializeField] protected PlayerStats stats;
    [SerializeField] private float speedMultiplier = 3000f;

    [Header("Movement")]
    [SerializeField] private float speedTransitionTime = 10f;

    private Vector3 moveDirection;
    private Vector3 lerpedDirection;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float maxAngle = 60f;

    public bool canRotate;

    [Header("Aim")]
    [SerializeField] private Transform aimTarget;
    [SerializeField] private TrajectoryPreview trajectory;
    [SerializeField] private Color aimColor;
    [SerializeField] private float aimDistance = 5f;
    [SerializeField] private float aimSpeed = 3f;

    private AimsManager aimsManager;
    private float currentAimAngle;

    [Header("Combat Equipment")]
    [Tooltip(
        "Assign a BowWeapon, CrossbowWeapon, ShieldController, " +
        "or another component implementing ICombatEquipment.")]
    [SerializeField] private MonoBehaviour combatEquipmentBehaviour;

    protected ICombatEquipment combatEquipment;

    [Header("Animation")]
    [SerializeField] protected Animator animator;
    [SerializeField] private float animTransitionTime = 5f;

    private float blendValue;

    [SerializeField] private float animLayerTransitionTime = 5f;

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

    protected virtual void Awake()
    {
        InitializeStats();
        GetRequiredComponents();
        ResolveCombatEquipment();

        AfterAwake();
    }

    protected virtual void Start()
    {
        InitializeValues();
        InitializeCombatEquipment();
        SubscribeToEquipmentEvents();

        layerTargetWeight = 1f;

        AfterStart();
    }

    protected virtual void Update()
    {
        if (!isDead && !isPause)
        {
            HandleMovement();
            HandleAttack();

            if (canRotate)
            {
                HandleRotation();
            }
            else
            {
                HandleAim();
            }
        }

        HandleAnimation();
        AfterUpdate();
    }

    private void InitializeStats()
    {
        if (baseProfile == null)
        {
            Debug.LogError(
                $"{name}: PlayerStatsProfile is not assigned.");

            return;
        }

        stats = baseProfile.GetStats().Clone();
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
            Debug.LogError(
                $"{name}: Rigidbody component is missing.");
        }

        if (input == null)
        {
            Debug.LogError(
                $"{name}: InputHandler component is missing.");
        }
    }

    private void InitializeValues()
    {
        aimsManager = FindFirstObjectByType<AimsManager>();

        if (aimsManager != null && aimTarget != null)
        {
            aimsManager.AddPlayerAim(
                aimTarget,
                aimColor);
        }

        canMove = true;
        canAttack = true;
        canRotate = false;
    }

    private void ResolveCombatEquipment()
    {
        if (combatEquipmentBehaviour == null)
        {
            Debug.LogWarning(
                $"{name}: No combat equipment is assigned.");

            return;
        }

        combatEquipment = combatEquipmentBehaviour as ICombatEquipment;

        if (combatEquipment == null)
        {
            Debug.LogError(
                $"{name}: Assigned component " +
                $"{combatEquipmentBehaviour.GetType().Name} " +
                $"does not implement {nameof(ICombatEquipment)}.");
        }
    }

    private void InitializeCombatEquipment()
    {
        if (combatEquipment == null ||
            stats == null)
        {
            return;
        }

        combatEquipment.Initialize(
            gameObject,
            Team.Player,
            stats.core);
    }

    private void SubscribeToEquipmentEvents()
    {
        if (combatEquipment == null)
            return;

        combatEquipment.OnAttack += HandleEquipmentAttack;
        combatEquipment.OnReloadStarted += HandleReloadStarted;
        combatEquipment.OnReloadFinished += HandleReloadFinished;
    }

    private void UnsubscribeFromEquipmentEvents()
    {
        if (combatEquipment == null)
            return;

        combatEquipment.OnAttack -= HandleEquipmentAttack;
        combatEquipment.OnReloadStarted -= HandleReloadStarted;
        combatEquipment.OnReloadFinished -= HandleReloadFinished;
    }

    private void HandleMovement()
    {
        if (rb == null || input == null || stats == null)
            return;

        if (canMove)
        {
            moveDirection = new Vector3(
                input.move.x,
                0f,
                input.move.y);
        }
        else
        {
            moveDirection = Vector3.zero;
        }

        lerpedDirection = Vector3.Lerp(
            lerpedDirection,
            moveDirection,
            Time.deltaTime * speedTransitionTime);

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

        /*
         * Use() returns false when:
         *
         * Bow is on cooldown.
         * Crossbow is shooting or reloading.
         * Shield is attacking or cooling down.
         */
        combatEquipment.Use();
    }

    private void HandleEquipmentAttack()
    {
        if (isDead)
            return;

        PlayAttackSound();

        if (animator != null)
        {
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

        float movementAmount =
            moveDirection.magnitude;

        blendValue = Mathf.Lerp(
            blendValue,
            movementAmount,
            Time.deltaTime * animTransitionTime);

        layerCurrentWeight = Mathf.Lerp(
            layerCurrentWeight,
            layerTargetWeight,
            animLayerTransitionTime * Time.deltaTime);

        if (animator.layerCount > 1)
        {
            animator.SetLayerWeight(
                1,
                layerCurrentWeight);
        }

        animator.SetFloat(
            "Blend",
            blendValue);

        animator.SetBool(
            "Dead",
            isDead);

        AdditiveAnimation();
    }

    private void HandleAim()
    {
        if (input == null)
            return;

        float inputX = input.look.x;

        currentAimAngle +=
            inputX * aimSpeed;

        currentAimAngle = Mathf.Clamp(
            currentAimAngle,
            -maxAngle,
            maxAngle);

        Quaternion rotation =
            Quaternion.Euler(
                0f,
                currentAimAngle,
                0f);

        Vector3 direction =
            rotation * Vector3.forward;

        if (aimTarget != null)
        {
            aimTarget.localPosition =
                Vector3.forward * aimDistance;
        }

        RotateCharacter(direction);
    }

    private void HandleRotation()
    {
        if (moveDirection.sqrMagnitude <= 0.0001f)
            return;

        currentAimAngle = 0f;

        RotateCharacter(
            moveDirection.normalized);
    }

    private void RotateCharacter(Vector3 direction)
    {
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

    public virtual void GetHitAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
    }

    public virtual void PlayAttackSound()
    {
        PlayRandomSound(attackSoundEffects);
    }

    protected void PlayRandomSound(AudioClip[] sounds)
    {
        if (sounds == null ||
            sounds.Length == 0)
        {
            return;
        }

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

    public PlayerClass GetPlayerClass()
    {
        return playerClass;
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

    /*
     * Child-class hooks.
     *
     * Derived player classes don't need Awake(), Start(), or Update().
     * Override these methods instead.
     */

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
    }
}