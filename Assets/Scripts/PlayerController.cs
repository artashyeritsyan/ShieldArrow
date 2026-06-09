using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

enum PlayerId
{
    singlePlayer,
    firstPlayer,
    secondPlayer
}

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] float maxHp = 100f;
    [SerializeField] float speed = 100f;
    float currentHp;

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

    float currentAimAngle = 0f;

    [Header("Animation")]
    public Animator animator;

    Rigidbody rb;
    Vector3 moveDirection;

    private InputHandler input;

    void Awake()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        Debug.Log("Player index: " + playerInput.playerIndex);
    }

    void Start()
    {
        
        rb = GetComponent<Rigidbody>();
        currentHp = maxHp;
        canRotate = false;
        input = GetComponent<InputHandler>();

        aimsManager = FindFirstObjectByType<AimsManager>();
        aimsManager.AddPlayerAim(aimTarget, aimColor);
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

    void HandleMovement()
    {
        rb.AddForce(speed * Time.deltaTime * moveDirection);

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

        // Reset aim angle so switching modes feels correct
        currentAimAngle = 0f;

        RotateCharacter(moveDirection.normalized);
    }

    void HandleAttack()
    {
        //if (Mouse.current.leftButton.isPressed)
    }

    public void OnMove(InputValue input)
    {
        Vector2 move = input.Get<Vector2>();
        moveDirection = new Vector3(move.x, 0f, move.y);
    }



}