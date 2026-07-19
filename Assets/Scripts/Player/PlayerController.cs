using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public enum PlayerClass { 
    Archer, 
    Knight,
    Crossbower,
    Barbarian,
    Assasin,
    Mage,
}

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected PlayerClass playerClass;
    [SerializeField] private PlayerStatsProfile baseProfile;
    [SerializeField] PlayerStats stats;
    [SerializeField] float speedMultiplier = 3000f;

    [Header("Skill")]

    // Some skill data

    //[Header("Weapons")]
    //[SerializeField] GameObject[] weapons;
    //[SerializeField] float weaponSwitchDelay = 0.2f;

    [Header("Movement")]
    [SerializeField] float speedTransitionTime = 10f;
    private Vector3 moveDirection;
    private Vector3 lerpedDirection;

    [Header("Rotation")]
    [SerializeField] float rotationSpeed = 15f;
    [SerializeField] float maxAngle = 60f;
    public bool canRotate = false;

    [Header("Aim")]
    [SerializeField] Transform aimTarget;
    [SerializeField] TrajectoryPreview trajectory;
    [SerializeField] Color aimColor;
    [SerializeField] float aimDistance = 5f;
    [SerializeField] float aimSpeed = 3f;

    private AimsManager aimsManager;
    private float currentAimAngle = 0f;

    [Header("Weapon")]
    protected GameObject weapon;
    //private WeaponScript weaponScript;
    //public GameObject arrow;

    [Header("Animation")]
    [SerializeField] protected Animator animator;
    [SerializeField] float animTransitionTime = 5f;
    private float blendValue;

    [SerializeField] float animLayerTransitionTime = 5f;
    protected float layerTargetWeight = 0;
    protected float layerCurrentWeight = 0;

    [Header("Sound Effects")]
    public AudioClip[] attackSoundEffects;

    [Header("Main Modifiers")]
    [SerializeField] protected bool isDead;
    protected bool isPause;
    protected bool canMove;
    protected bool canAttack;


    Rigidbody rb;
    private InputHandler input;


    // FOR future me
    // Do NOT use START() or AWAKE() in Childs
    // Use AfterStart() and AfterAwake() instead, and override them.
    protected virtual void Awake()
    {
        stats = baseProfile.GetStats().Clone();

    }

    protected virtual void Start()
    {
        InitialiseValues();

        layerTargetWeight = 1;
    }

    protected virtual void Update()
    {
        if (!isDead)
        {
            HandleMovement();
            HandleAttack();
        }

        HandleAnimation();

        // Implement it to listen the event inRoom(bool). and if in it is true than set canRotate = true
        if (canRotate)
        {
            HandleRotation();
        }
        else
        {
            //HandleMouseLookClamped();
            HandleAim();
        }


    }

    private void InitialiseValues()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<InputHandler>();
        animator = GetComponent<Animator>();

        aimsManager = FindFirstObjectByType<AimsManager>();
        aimsManager.AddPlayerAim(aimTarget, aimColor);

        canMove = true;
        canRotate = false;
    }

    // Predefine for further development
    public void DealDamage(CoreStats target)
    {
        DamageData dmg = new DamageData
        {
            baseDamage = stats.core.damage,
            canCrit = true,
            critChance = 0.2f,
            critMultiplier = 2f
        };

        DamageSystem.ProcessDamage(dmg, stats.core, target);
    }
    public PlayerClass GetPlayerClass() { return playerClass; }


    public virtual void GetHitAnimation()
    {
        animator.SetTrigger("Hit");
    }


    public virtual void AfterAwake() { }
    public virtual void AfterStart() { }
    public virtual void AfterUpdate() { }

    public virtual void Dead() { }
    public virtual void AdditiveAnimation() { }
    public virtual void Attack() { }

    public virtual void PlayAttackSound() { }



    // ------------------- Private functions ------------------
    // --------------------------------------------------------

    void HandleMovement()
    {
        // for smooth stop if canMove = false
        if (canMove)
        {
            moveDirection = new Vector3(input.move.x, 0, input.move.y);
        }
        else
        {
            moveDirection = Vector3.zero;
        }

        lerpedDirection = Vector3.Lerp(lerpedDirection, moveDirection, Time.deltaTime * speedTransitionTime);

        rb.AddForce(stats.core.speed * speedMultiplier * Time.deltaTime * lerpedDirection);

    }

    void HandleAnimation()
    {
        float playerVector = Mathf.Sqrt(Mathf.Pow(moveDirection.x, 2) + Mathf.Pow(moveDirection.z, 2));
        blendValue = Mathf.Lerp(blendValue, playerVector, Time.deltaTime * animTransitionTime);

        layerCurrentWeight = Mathf.Lerp(layerCurrentWeight, layerTargetWeight, animLayerTransitionTime * Time.deltaTime);
        animator.SetLayerWeight(1, layerCurrentWeight);

        animator.SetFloat("Blend", blendValue);

        animator.SetBool("Dead", isDead);

        AdditiveAnimation();
    }

    void RotateCharacter(Vector3 direction)
    {
        if (direction == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    void HandleAim()
    {
        float inputX = input.look.x;

        currentAimAngle += inputX * aimSpeed;
        currentAimAngle = Mathf.Clamp(currentAimAngle, -maxAngle, maxAngle);

        Quaternion rotation = Quaternion.Euler(0f, currentAimAngle, 0f);
        Vector3 direction = rotation * Vector3.forward;

        aimTarget.localPosition = Vector3.forward * aimDistance;

        RotateCharacter(direction);
    }

    void HandleRotation()
    {
        if (moveDirection == Vector3.zero) return;

        currentAimAngle = 0f;

        RotateCharacter(moveDirection.normalized);
    }

    void HandleAttack()
    {
        if (input.attack)
        {
            Attack();
        }

    }




    // Will be used for Ultra Attack

    //IEnumerator AttackCooldown()
    //{


    //}

    public void SetCanMove(bool isCanMove) { canMove = isCanMove; }
    public bool GetCanMove() { return canMove; } 

}