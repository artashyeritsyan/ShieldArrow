using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;



enum PlayerClass { 
    Archer, 
    Knight 
}

public class PlayerController : MonoBehaviour
{

    [Header("Stats")]
    [SerializeField] private PlayerStatsProfile baseProfile;
    [SerializeField] PlayerStats stats;
    [SerializeField] float speedMultiplier = 2000f;

    [Header("Skill")] 

    // Some skill data

    //[Header("Weapons")]
    //[SerializeField] GameObject[] weapons;
    //[SerializeField] float weaponSwitchDelay = 0.2f;

    [Header("Rotation")]
    [SerializeField] float rotationSpeed = 15f;
    [SerializeField] float maxAngle = 60f;
    public bool canRotate = false;

    [Header("Aim")]
    [SerializeField] Transform aimTarget;
    [SerializeField] Color aimColor;
    [SerializeField] float aimDistance = 5f;
    [SerializeField] float aimSpeed = 4f;

    private AimsManager aimsManager;
    private float currentAimAngle = 0f;

    [Header("Weapon")]
    public GameObject weapon;
    public WeaponScript weaponScript;
    //public GameObject arrow;

    [Header("Animation")]
    public Animator animator;


    Rigidbody rb;
    private InputHandler input;
    private Vector3 moveDirection;

    void Awake()
    {
        stats = baseProfile.GetStats().Clone();
        PlayerInput playerInput = GetComponent<PlayerInput>();
        Debug.Log("Player index: " + playerInput.playerIndex);

    }

    void Start()
    {
        InitialiseValues();
    }

    void Update()
    {
        HandleMovement();

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
        HandleAttack();
    }

    private void InitialiseValues()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<InputHandler>();

        aimsManager = FindFirstObjectByType<AimsManager>();
        aimsManager.AddPlayerAim(aimTarget, aimColor);

        // Init Weapon
        InitialiseWeapon();

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

    public virtual void Shoot() {
        weaponScript.Shoot();
    }
    public virtual void Dead() { }
    

    // ------------------- Private functions ------------------
    // --------------------------------------------------------

    void HandleMovement()
    {
        moveDirection = new Vector3(input.move.x, 0, input.move.y);

        rb.AddForce(stats.core.speed * speedMultiplier * Time.deltaTime * moveDirection);

        if ((moveDirection == Vector3.zero))
        {
            animator.SetBool("Running", false);
        }
        else
        {
            animator.SetBool("Running", true);
        }
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
            Shoot();
        }
    }

    private void InitialiseWeapon()
    {
        weaponScript = transform.GetComponentInChildren<WeaponScript>();
        weapon = weaponScript.gameObject;
    }


    // Will be used for Ultra Attack

    //IEnumerator AttackCooldown()
    //{


    //}



}