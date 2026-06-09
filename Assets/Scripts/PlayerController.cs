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

    //[Header("Dash")]
    //[SerializeField] float dashForce = 200f;
    //[SerializeField] float dashRecoverTime = 1f;

    //[Header("Weapons")]
    //[SerializeField] GameObject[] weapons;
    //[SerializeField] float weaponSwitchDelay = 0.2f;

    [Header("Rotation")]
    [SerializeField] float rotationSpeed = 15f;
    //[SerializeField] float maxAngle = 60f;
    public bool canRotate = false;

    [Header("Aim")]
    [SerializeField] Transform aimTarget;
    [SerializeField] float aimDistance = 5f;
    [SerializeField] float maxAngle = 60f;
    [SerializeField] float aimSpeed = 4f;

    float currentAimAngle = 0f; // relative to forward

    [Header("Animation")]
    public Animator animator;

    Rigidbody rb;
    Vector3 moveDirection;

    private InputHandler input;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentHp = maxHp;
        canRotate = false;
        baseRotation = transform.rotation;
        input = GetComponent<InputHandler>();
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
        rb.AddForce(moveDirection * speed * Time.deltaTime);

        if ((moveDirection == Vector3.zero))
        {
            animator.SetBool("Running", false);
        }
        else
        {
            animator.SetBool("Running", true);
        }
    }

    

    Quaternion baseRotation;

    //void HandleMouseLookClamped()
    //{
    //    //Vector2 mousePos = Input.mousePosition;

    //    Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

    //    Vector2 direction2D = (mousePos - screenCenter);

    //    if (direction2D.magnitude < 10f) return;

    //    Vector3 direction = new Vector3(direction2D.x, 0f, direction2D.y).normalized;

    //    Vector3 camForward = Camera.main.transform.forward;
    //    Vector3 camRight = Camera.main.transform.right;

    //    camForward.y = 0f;
    //    camRight.y = 0f;

    //    Vector3 worldDirection = (camRight * direction.x + camForward * direction.z).normalized;

    //    if (worldDirection.sqrMagnitude < 0.001f) return;

    //    Quaternion lookRotation = Quaternion.LookRotation(worldDirection);

    //    float angle = Quaternion.Angle(baseRotation, lookRotation);

    //    if (angle > maxAngle)
    //    {
    //        float sign = Mathf.Sign(Vector3.SignedAngle(
    //            baseRotation * Vector3.forward,
    //            worldDirection,
    //            Vector3.up));

    //        lookRotation = baseRotation * Quaternion.Euler(0f, sign * maxAngle, 0f);
    //    }

    //    transform.rotation = Quaternion.Slerp(
    //        transform.rotation,
    //        lookRotation,
    //        rotationSpeed * Time.deltaTime
    //    );
    //}

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